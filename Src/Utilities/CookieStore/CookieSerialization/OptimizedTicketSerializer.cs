using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;

namespace Ccf.Ck.Utilities.CookieTicketStore.CookieSerialization
{
    public sealed class OptimizedTicketSerializer : TicketSerializer
    {
        private readonly Boolean deflateCompress = true;
        private readonly Boolean optimizeBase64Strings = true;
        private readonly Boolean optimizeClaims = true;

        private readonly PropertiesSerializer propertiesSerializer;

        private readonly ILogger log;

        public OptimizedTicketSerializer(PropertiesSerializer propertiesSerializer, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            this.log = loggerFactory.CreateLogger<OptimizedTicketSerializer>();

            if (propertiesSerializer == null)
            {
                this.propertiesSerializer = this.optimizeBase64Strings ? OptimizedPropertiesSerializer.Default : PropertiesSerializer.Default;
            }
            else
            {
                this.propertiesSerializer = propertiesSerializer;
            }
        }

        #region Write + Serialize

        // This is an IDataSerializer entrypoint.
        public override Byte[] Serialize(AuthenticationTicket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            if (!this.deflateCompress)
            {
                return base.Serialize(ticket);
            }

            // DeflateStream and GZipStream both use the DEFLATE algorithm.
            // But GZipStream adds headers and checksums and in .NET can be up to 50% slower than DeflateStream (see Jeff Atwood's 2008 blog posting).
            // We don't need GZipStream because we don't need the GZip header for interopability with other GZip consumers - so using DeflateStream should be better.
            // Note to self: Update the Optimized.Insights client to use DeflateStream too?

            // Note that I don't want compression to be performed by the CookieManager instead of the Ticket Serializer because that would affect other cookies too (I think...?)
            // Also I need to have control over the first few bytes of the cookie so I can write the format version header so if the original ASP.NET TicketSerializer reads it it will always reject it instead of trying to read it if the DeflateStream accidentally wrote an original TicketSerializer header by coincidence at runtime.

            using (MemoryStream ms = new MemoryStream(capacity: 4096)) // Initial capacity of 4096 because browsers tend to reject cookies longer than that anyway.
            {
                // For compatibility, write the FormatVersion uncompressed first, so ASP.NET Core will reject it because it isn't `0x05 0x00 0x00 0x00` (little-endian).
                ms.Write(OptimizedTicketDataFormat.FormatVersionArr, offset: 0, count: 4);

                using (DeflateStream ds = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                using (BinaryWriter wtr = new BinaryWriter(ds))
                {
                    this.Write(wtr, ticket);
                }

                return ms.ToArray();
            }
        }

        // Despite being public, this method isn't called from anywhere, to my knowledge.
        public override void Write(BinaryWriter writer, AuthenticationTicket ticket)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));

            // This method needs to be entirely reimplemented because TicketSerializer writes properties with a hardcoded call to `PropertiesSerializer.Default.Write(...)`.
            // If deflating, no need to write the format version again, otherwise write it for compatibility.
            if (!this.deflateCompress)
            {
                writer.Write(OptimizedTicketDataFormat.FormatVersion);
            }

            writer.Write(ticket.AuthenticationScheme);

            List<ClaimsIdentity> identities = (List<ClaimsIdentity>)ticket.Principal.Identities;
            if (identities.Count > 255) throw new InvalidOperationException("The number of Identities exceeds 255.");
            writer.Write((Byte)identities.Count);

            foreach (ClaimsIdentity identity in ticket.Principal.Identities)
            {
                this.WriteIdentity(writer, identity);
            }

            this.propertiesSerializer.Write(writer, ticket.Properties);
        }

        private const String _DefaultValuePlaceholder = "\0"; // BinaryWriter will write `0x01 0x00`.

        /// <summary>Writes an Identity's Claims using a codebook to intern Claims' ValueType, Issuer, and OriginalIssuer string values.</summary>
        protected override void WriteIdentity(BinaryWriter writer, ClaimsIdentity identity)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (identity == null) throw new ArgumentNullException(nameof(identity));

            writer.Write(identity.AuthenticationType ?? String.Empty);

            if (identity.NameClaimType == ClaimsIdentity.DefaultNameClaimType) writer.Write(_DefaultValuePlaceholder); else writer.Write(identity.NameClaimType);
            if (identity.RoleClaimType == ClaimsIdentity.DefaultRoleClaimType) writer.Write(_DefaultValuePlaceholder); else writer.Write(identity.RoleClaimType);

            // The default TicketDataFormat does optimize "http://www.w3.org/2001/XMLSchema#string" to "\0" but we can be more general!
            // So we build a code-book of all ClaimValueTypes and Issuers to avoid seeing repetitions of "http://www.w3.org/2001/XMLSchema#integer" (claimValueType) and "https://mywebsite.com" (issuer).

            // ClaimsIdentity.Claims is an IEnumerator<Claim> that is basically `this.instanceClaims.Concat( this.externalClaims.Where( c => c != null ) )`. Using ToList() means only iterating it once.
            // I wish C# added `IReadOnlyCollection<T>-iterators` where the first return must be `yield return {Count}`. That'd be cool because then Linq and other optimizations can be done based on a pre-known collection size.

            List<Claim> claims = identity.Claims.ToList();
            Int32 codebookRemainingCapacity = 255 - OptimizedTicketDataFormat.Format100CommonCodebook.Count;
            if (claims.Count > codebookRemainingCapacity) throw new InvalidOperationException("The number of claims exceeds " + codebookRemainingCapacity + "."); // Do this test (assuming each claim will be in the codebook exactly 1) to fail-fast instead of getting a strange error due to Byte overflow or related.

            if (this.optimizeClaims)
            {
                (Dictionary<String, Byte> codebookMap, List<String> codebookValues) = CreateCodebook(OptimizedTicketDataFormat.Format100CommonCodebook, claims);

                writer.Write((Int16)codebookValues.Count);
                foreach (String value in codebookValues.Skip(OptimizedTicketDataFormat.Format100CommonCodebook.Count))
                {
                    writer.Write(value);
                }

                writer.Write((Byte)claims.Count);
                foreach (Claim claim in identity.Claims)
                {
                    // ASP.NET Core's original Serialized format: <Claim.Type>, <Claim.Value>, <Claim.ValueType>, <Claim.Issuer>, <Claim.OriginalIssuer>, <Claim.Properties.Count>, [<Claim.Properties>...]
                    // I wonder if it's worth writing integer claim values using variable-length integer encoding so they'd use less space than string-encoded integers. But I think that's going too far...
                    // I'd have to put `Claim.ValueType` before Claim.Value so the value could be read correctly. As the saving would be around 1 byte per claim (assuming integers in range 0-9) I don't think it's worth the added complexity - and likely wouldn't save anything after DEFLATE.

                    WriteClaim(writer, claim, codebookMap);
                }
            }
            else
            {
                writer.Write((Byte)claims.Count);
                foreach (Claim claim in identity.Claims)
                {
                    this.WriteClaim(writer, claim);
                }
            }

            //

            String bs = identity.BootstrapContext as String;
            if (String.IsNullOrWhiteSpace(bs))
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(bs);
            }

            //

            if (identity.Actor == null)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                this.WriteIdentity(writer, identity.Actor);
            }
        }

        private static (Dictionary<String, Byte> codebookMap, List<String> codebookValues) CreateCodebook(IReadOnlyList<String> common, IEnumerable<Claim> claims)
        {
            Dictionary<String, Byte> codebookMap = new Dictionary<String, Byte>(capacity: common.Count + 8); // Assuming 8 new codebook entries for a given set of Claims
            List<String> codebookValues = new List<String>(capacity: common.Count + 8);

            foreach (String value in common)
            {
                codebookValues.Add(value);
                codebookMap[value] = (Byte)(codebookValues.Count - 1);
            }

            foreach (Claim c in claims)
            {
                if (!codebookMap.ContainsKey(c.Type)) // NetFx Dictionary<K,V> doesn't have TryAdd.
                {
                    codebookValues.Add(c.Type);
                    codebookMap[c.Type] = (Byte)(codebookValues.Count - 1);
                }

                if (!codebookMap.ContainsKey(c.ValueType))
                {
                    codebookValues.Add(c.ValueType);
                    codebookMap[c.ValueType] = (Byte)(codebookValues.Count - 1);
                }

                if (!codebookMap.ContainsKey(c.Issuer))
                {
                    codebookValues.Add(c.Issuer);
                    codebookMap[c.Issuer] = (Byte)(codebookValues.Count - 1);
                }

                if (!codebookMap.ContainsKey(c.OriginalIssuer))
                {
                    codebookValues.Add(c.OriginalIssuer);
                    codebookMap[c.OriginalIssuer] = (Byte)(codebookValues.Count - 1);
                }
            }

            return (codebookMap, codebookValues);
        }

        protected override void WriteClaim(BinaryWriter writer, Claim claim)
        {
            if (this.optimizeClaims)
            {
                throw new NotSupportedException();
            }
            else
            {
                base.WriteClaim(writer, claim);
            }
        }

        private static void WriteClaim(BinaryWriter writer, Claim claim, Dictionary<String, Byte> codebookMap)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (claim == null) throw new ArgumentNullException(nameof(claim));

            writer.Write(codebookMap[claim.Type]);     // Writes a Byte
            writer.Write(claim.Value);                    // Writes a String
            writer.Write(codebookMap[claim.ValueType]); // Writes a Byte
            writer.Write(codebookMap[claim.Issuer]);
            writer.Write(codebookMap[claim.OriginalIssuer]);

            if (claim.Properties.Count > 255) throw new InvalidOperationException("Claim has too many properties to serialize.");

            writer.Write((Byte)claim.Properties.Count);
            foreach (KeyValuePair<String, String> property in claim.Properties)
            {
                writer.Write(property.Key ?? String.Empty);
                writer.Write(property.Value ?? String.Empty);
            }
        }

        #endregion

        #region Read + Deserialize

        // This is an IDataSerializer entrypoint.
        public override AuthenticationTicket Deserialize(Byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (!this.deflateCompress)
            {
                return base.Deserialize(data);
            }

            using (MemoryStream ms = new MemoryStream(data, writable: false))
            {
                // Ensure the FormatVersion is the same before assuming it's a Deflate stream (because a future version might use LZW or LZMA instead).
                {
                    Byte[] format = new Byte[4];
                    if (ms.Read(format, offset: 0, count: format.Length) != format.Length) return null;
                    Int32 dataVersion = BitConverter.ToInt32(format, 0);
                    if (dataVersion != OptimizedTicketDataFormat.FormatVersion)
                    {
                        // Debug-level because otherwise this is going to logspam in production from all the existing users' cookies.
                        this.log.LogDebug(message: "Expected auth cookie data version " + OptimizedTicketDataFormat.FormatVersion + " but encountered " + dataVersion + ".");
                        return null;
                    }
                }

                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress, leaveOpen: false))
                using (BinaryReader reader = new BinaryReader(ds))
                {
                    return this.Read(reader);
                }
            }
        }

        // Despite being public, this method isn't called from anywhere outside OptimizedTicketSerializer and TicketSerializer, to my knowledge.
        public override AuthenticationTicket Read(BinaryReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            try
            {
                if (!this.deflateCompress)
                {
                    if (reader.ReadInt32() != OptimizedTicketDataFormat.FormatVersion) return null;
                }

                String authenticationScheme = reader.ReadString();

                //

                Byte identityCount = reader.ReadByte();

                ClaimsIdentity[] identities = new ClaimsIdentity[identityCount];
                for (Int32 i = 0; i < identityCount; i++)
                {
                    identities[i] = this.ReadIdentity(reader);
                }

                //

                AuthenticationProperties properties = this.propertiesSerializer.Read(reader);

                //

                ClaimsPrincipal cp = new ClaimsPrincipal(identities);

                return new AuthenticationTicket(cp, properties, authenticationScheme);
            }
            catch (Exception ex)
            {
                this.log.LogError(ex, message: "Failed to read serialized AuthenticationTicket.");
                return null;
            }
        }

        protected override ClaimsIdentity ReadIdentity(BinaryReader reader)
        {
            String authenticationType = reader.ReadString();

            String nameClaimType = reader.ReadString();
            if (nameClaimType == _DefaultValuePlaceholder) nameClaimType = ClaimsIdentity.DefaultNameClaimType;

            String roleClaimType = reader.ReadString();
            if (roleClaimType == _DefaultValuePlaceholder) roleClaimType = ClaimsIdentity.DefaultRoleClaimType;

            // TODO: Have a single codebook for the entire ticket? How often do Tickets have multiple Identities?
            // NOTE: I did an experiment where if all Claims had the same Issuer+OriginalIssuer, then each Claim would miss the last 2 bytes - but this doesn't work as Claims have different Issuers anyway ("oidc" and "http://localhost:7000"). Not worth dealing with.
            IReadOnlyList<String> codebook = ReadCodebook(OptimizedTicketDataFormat.Format100CommonCodebook, reader);

            //

            ClaimsIdentity identity = new ClaimsIdentity(authenticationType, nameClaimType, roleClaimType);

            List<Claim> claims = new List<Claim>();

            Byte claimCount = reader.ReadByte();
            for (Byte i = 0; i < claimCount; i++)
            {
                Claim claim = ReadClaim(reader, identity, codebook);
                identity.AddClaim(claim);
            }

            if (reader.ReadBoolean()) // BinaryWriter's Boolean values are 1-byte, not 4.
            {
                identity.BootstrapContext = reader.ReadString();
            }

            if (reader.ReadBoolean())
            {
                identity.Actor = this.ReadIdentity(reader);
            }

            return identity;
        }

        private static IReadOnlyList<String> ReadCodebook(IReadOnlyList<String> common, BinaryReader reader)
        {
            Int16 count = reader.ReadInt16();
            if (count < common.Count) throw new InvalidOperationException("Declared codebook length " + count + " is less than shared codebook length " + common.Count + ".");

            String[] codebook = new String[count];
            for (Int32 i = 0; i < common.Count; i++)
            {
                codebook[i] = common[i];
            }

            for (Int32 i = common.Count; i < count; i++)
            {
                codebook[i] = reader.ReadString();
            }

            return codebook;
        }

        protected override Claim ReadClaim(BinaryReader reader, ClaimsIdentity identity)
        {
            if (this.optimizeClaims)
            {
                throw new NotSupportedException();
            }
            else
            {
                return base.ReadClaim(reader, identity);
            }
        }

        private static Claim ReadClaim(BinaryReader reader, ClaimsIdentity identity, IReadOnlyList<String> codebook)
        {
            Byte claimTypeIdx = reader.ReadByte();
            String claimValue = reader.ReadString();
            Byte claimValueTypeIdx = reader.ReadByte();
            Byte claimIssuerIdx = reader.ReadByte();
            Byte claimOriginalIssuerIdx = reader.ReadByte();

            String claimType = codebook[claimTypeIdx];
            String claimValueType = codebook[claimValueTypeIdx];
            String claimIssuer = codebook[claimIssuerIdx];
            String claimOriginalIssuer = codebook[claimOriginalIssuerIdx];

            Claim claim = new Claim
            (
                type: claimType,
                value: claimValue,
                valueType: claimValueType,
                issuer: claimIssuer,
                originalIssuer: claimOriginalIssuer,
                subject: identity
            );

            // I've never seen any Claims with Properties, so this isn't used. So I won't convert it to use Codebook yet.
            Byte propertiesCount = reader.ReadByte();
            for (Int32 i = 0; i < propertiesCount; i++)
            {
                String propertyKey = reader.ReadString();
                String propertyValue = reader.ReadString();
                claim.Properties.Add(propertyKey, propertyValue);
            }

            return claim;
        }

        #endregion
    }
}