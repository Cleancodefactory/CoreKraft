using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Ccf.Ck.Utilities.CookieTicketStore.CookieSerialization
{
    public static class BinaryIOExtensions
    {
        private static readonly Regex _Base64RegEx = new Regex(@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}[AEIMQUYcgkosw048]=|[A-Za-z0-9+/][AQgw]==)?$", RegexOptions.Compiled);

        /// <summary>If <paramref name="value"/> is longer than 8 characters and is a Base64 string then the string is decoded to bytes and written directly (saving 33% space), otherwise the default <see cref="BinaryWriter.Write(string)"/> method is used.</summary>
        public static void WriteString2(this BinaryWriter writer, String value)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (!TryWriteOptimizedBase64(writer, value))
            {
                writer.Write(String2Header_BinaryWriterString);
                writer.Write(value);
            }
        }

        // 1. Is it a pure Base64 string?
        // 2. Is it a JWT Base64 string? (separate Base64 strings separated by ".")
        // 3. Otherwise, write normally.

        // The string-length check is to avoid the relatively expensive Base64-handling code-path with short strings that won't benefit as much.
        private static Boolean TryWriteOptimizedBase64(BinaryWriter writer, String value)
        {
            if (value.Length <= 8) return false;

            if (IsBase64String(value))
            {
                return WriteSimpleBase64String(writer, value);
            }
            else if (IsBase64UrlForJwtString(value, out IReadOnlyList<Char[]> base64CharRuns))
            {
                return WriteJwtBase64String(writer, value, base64CharRuns);
            }
            else
            {
                return false;
            }
        }

        private static Boolean TryConvertFromBase64String(String base64String, out Byte[] bytes)
        {
            try
            {
                if (!string.IsNullOrEmpty(base64String))
                {
                    if ((base64String.Length % 4 == 0) && _Base64RegEx.IsMatch(base64String))
                    {
                        bytes = Convert.FromBase64String(base64String);
                        return true;
                    }
                }
                bytes = Array.Empty<Byte>();
                return false;
            }
            catch (FormatException)
            {
                bytes = Array.Empty<Byte>();
                return false;
            }
        }

        private static Boolean TryConvertFromBase64String(Char[] base64String, out Byte[] bytes)
        {
            try
            {
                bytes = Convert.FromBase64CharArray(base64String, 0, base64String.Length);
                return true;
            }
            catch (FormatException)
            {
                bytes = Array.Empty<Byte>();
                return false;
            }
        }

        private const Byte String2Header_BinaryWriterString = 0x00;    // 0b_0000_0000 followed by BinaryWriter's variable-byte 7-bit length encoding.
        private const UInt16 String2Header_SimpleBase64 = 0x80_00; // 0b_1000_0000 OR'd with the UInt16 string length (only use LSB 14 bits, so maximum length of `2^14 - 1` chars (16,383 chars).
        private const UInt16 String2Header_JwtBase64 = 0x40_00; // 0b_0100_0000 OR'd with the component count, followed by length-prefixed raw binaries.

        private static Boolean WriteSimpleBase64String(BinaryWriter writer, String value)
        {
            if (!TryConvertFromBase64String(value, out Byte[] raw)) return false;

            // Restrict these to 16,383 bytes (2^14 - 1) so it fits into a 16-bit integer while leaving the two MSBs for flags.
            if (raw.Length > 16383) return false;// throw new InvalidOperationException( "Property value exceeds 16,383 bytes in length." );

            // Write a UInt16 (big-endian!!!). If the MSB is 1, then it's an optimized Base64 write which includes the string length in the 14 LSB bits.

            Int32 header = String2Header_SimpleBase64 | raw.Length;
            Byte headerB0 = (Byte)((header >> 8) & 0xFF);
            Byte headerB1 = (Byte)((header) & 0xFF);

            writer.Write(headerB0); // Force big-endian.
            writer.Write(headerB1);

            writer.Write(raw);

            return true;
        }

        private static Boolean WriteJwtBase64String(BinaryWriter writer, String value, IReadOnlyList<Char[]> base64CharRuns)
        {
            if (base64CharRuns.Count > 16383) return false;

            // Convert all segments to bytes first, so we can fallback in case of error without corrupting the output stream.

            Byte[][] buffers = new Byte[base64CharRuns.Count][];

            for (Int32 i = 0; i < buffers.Length; i++)
            {
                Char[] base64Chars = base64CharRuns[i];

                if (!TryConvertFromBase64String(base64Chars, out Byte[] segment)) return false;
                if (segment.Length > 16383) return false; // Too long.

                buffers[i] = segment;
            }

            Int32 header = String2Header_JwtBase64 | buffers.Length;
            Byte headerB0 = (Byte)((header >> 8) & 0xFF);
            Byte headerB1 = (Byte)((header) & 0xFF);

            writer.Write(headerB0); // Force big-endian.
            writer.Write(headerB1);

            for (Int32 i = 0; i < buffers.Length; i++)
            {
                writer.Write((UInt16)buffers[i].Length);
                writer.Write(buffer: buffers[i]);
            }

            return true;
        }

        private static Boolean IsBase64String(String value)
        {
            // I wonder if this method could be optimized using SSE/AVX in .NET Core by using `Vector<T>`?

            // A-Z, a-z, 0-9, +, /, =

            for (Int32 i = 0; i < value.Length; i++)
            {
                Char c = value[i];

                Boolean isBase64Char =
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '+' ||
                    c == '/';

                if (i == value.Length - 2)
                {
                    if (c == '=' && value[i + 1] == '=') isBase64Char = true;
                }

                if (i == value.Length - 1)
                {
                    if (c == '=') isBase64Char = true;
                }

                if (!isBase64Char)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>JWT mandates Base64Url-encoding with different values separated by dots.</summary>
        private static Boolean IsBase64UrlForJwtString(String value, out IReadOnlyList<Char[]> base64Chars)
        {
            // I wonder if this method could be optimized using SSE/AVX in .NET Core by using `Vector<T>`?

            // A-Z, a-z, 0-9, +, /, =

            List<Int32> dotIndexes = null;

            for (Int32 i = 0; i < value.Length; i++)
            {
                Char c = value[i];

                if (c == '.')
                {
                    if (dotIndexes == null) dotIndexes = new List<Int32>(capacity: 4);
                    dotIndexes.Add(i);
                    continue;
                }

                Boolean isBase64Char =
                    (c >= 'A' && c <= 'Z') ||
                    (c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9') ||
                    c == '-' || // Instead of '+'
                    c == '_'    // Instead of '/'
#if REQUIRE_NO_PADDING
					// Base64Url strings have optional trailing padding, but we always remove it when decoding, so for now don't use our encoding if it does have trailing padding, otherwise it will cause the JWT signature to be invalid... I think?
					;
#else
                    // At the same time, JWT states that they're optional - so if they had the padding chars and were then removed, that wouldn't invalidate the signature, would it?
                    || c == '=';
#endif

                if (!isBase64Char)
                {
                    base64Chars = null;
                    return false;
                }
            }

            // Convert to a true Base64 string, with the correct length (Base64Url does not require trailing padding '=' characters):
            if (dotIndexes == null || dotIndexes.Count == 0)
            {
                Char[] run0 = GetBase64Chars(value, 0, value.Length);

                base64Chars = new Char[][] { run0 };
            }
            else
            {
                Char[][] runs = new Char[dotIndexes.Count + 1][];

                Int32 start = 0;
                for (Int32 i = 0; i < dotIndexes.Count + 1; i++)
                {
                    Int32 dotIdx = i < dotIndexes.Count ? dotIndexes[i] : value.Length;
                    Int32 length = dotIdx - start;

                    Char[] run = GetBase64Chars(value, start, length);
                    runs[i] = run;

                    start = dotIdx + 1;
                }

                base64Chars = runs;
            }

            return true;
        }

        private static Char[] GetBase64Chars(String base64Url, Int32 startIndex, Int32 length)
        {
            Char[] chars;
            if (length % 4 == 2)
            {
                chars = new Char[length + 2];
                chars[chars.Length - 1] = '=';
                chars[chars.Length - 2] = '=';
            }
            else if (length % 4 == 3)
            {
                chars = new Char[length + 1];
                chars[chars.Length - 1] = '=';
            }
            else
            {
                chars = new Char[length];
            }

            base64Url.CopyTo(sourceIndex: startIndex, destination: chars, destinationIndex: 0, count: length);

            for (Int32 i = 0; i < length; i++)
            {
                if (chars[i] == '-') chars[i] = '+';
                else if (chars[i] == '_') chars[i] = '/';
            }

            return chars;
        }

        public static String ReadString2(this BinaryReader reader)
        {
            Byte valueHeader = reader.ReadByte();
            if (valueHeader == 0x00)
            {
                // It's a BinaryWriter string:
                return reader.ReadString();
            }
            else if ((valueHeader & 0x80) == 0x80)
            {
                // SimpleBase64 string:

                Byte binaryLengthHigher = (Byte)(valueHeader & 0x3F);
                Byte binaryLengthLower = reader.ReadByte();
                Int32 binaryLength = (binaryLengthHigher << 8) | binaryLengthLower;

                Byte[] binaryValue = reader.ReadBytes(binaryLength);
                String base64Value = Convert.ToBase64String(binaryValue);
                return base64Value;
            }
            else if ((valueHeader & 0x40) == 0x40)
            {
                // JWT Base64Url string:

                Byte componentCountHigher = (Byte)(valueHeader & 0x3F);
                Byte componentCountLower = reader.ReadByte();
                Int32 componentCount = (componentCountHigher << 8) | componentCountLower;

                StringBuilder sb = new StringBuilder(capacity: 1024);
                for (Int32 i = 0; i < componentCount; i++)
                {
                    UInt16 segmentLength = reader.ReadUInt16();
                    Byte[] segmentBytes = reader.ReadBytes(segmentLength);

                    Char[] base64Chars = new Char[(Int32)(segmentLength * 1.4) + 2]; // Allocating a big enough buffer.

                    Int32 charArrayLength = Convert.ToBase64CharArray(inArray: segmentBytes, offsetIn: 0, length: segmentBytes.Length, outArray: base64Chars, offsetOut: 0);

                    if (i > 0) sb.Append('.');

                    ConvertFromBase64ToBase64UrlWhileAppending(base64Chars, charArrayLength, sb);
                }

                return sb.ToString();
            }
            else
            {
                throw new InvalidOperationException("Unrecognized string header prefix.");
            }
        }

        private static void ConvertFromBase64ToBase64UrlWhileAppending(Char[] base64Chars, Int32 charArrayLength, StringBuilder sb)
        {
            // Convert the Base64 chars into Base64UrlChars:

            for (Int32 i = 0; i < charArrayLength; i++)
            {
                if (base64Chars[i] == '+') base64Chars[i] = '-';
                else if (base64Chars[i] == '/') base64Chars[i] = '_';

                if (base64Chars[i] == '=') return; // Don't append any trailing padding characters. iWT states they're optional.
                else if (base64Chars[i] == '\0') return; // We've reached the end of the Base64 data, any nulls are from unused space in the Char[], though the `charArrayLength` parameter should catch that.

                sb.Append(base64Chars[i]);
            }
        }
    }
}
