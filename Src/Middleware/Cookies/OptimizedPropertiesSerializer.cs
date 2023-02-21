using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ccf.Ck.Web.Middleware.Cookies
{
    public sealed class OptimizedPropertiesSerializer : PropertiesSerializer
    {
        public new static OptimizedPropertiesSerializer Default { get; } = new OptimizedPropertiesSerializer();

        #region Serialize + Write

        // No need to override this as the behaviour is the same (create a MemoryStream and BinaryWriter)
        //		public override Byte[] Serialize( AuthenticationProperties model )
        //		{
        //			return base.Serialize( model );
        //		}

        public override void Write(BinaryWriter writer, AuthenticationProperties properties)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            // Some property values are Base64 strings, such as OIDC access_token and id_token.
            // As they're Base64 strings, they'll be double-base64 encoded when the Ticket is written to the visitor's web-browser. (Actually they're Base64Url-encoded strings separated by dots)
            // If a raw (JSON) id_token is 1024 bytes, it's 1,366 bytes when Base64-encoded.
            // Those 1,366 bytes then become 1,821 bytes. A 75% increase in size!
            // If it's Base64-encoded only once (during final output) then that's 1,366 vs 1,821 (455 bytes saved: 33%).

            writer.Write((Byte)OptimizedTicketDataFormat.FormatVersion);
            writer.Write((Int16)properties.Items.Count);

            foreach (KeyValuePair<String, String> item in properties.Items)
            {
                writer.Write(item.Key ?? String.Empty);

                writer.WriteString2(item.Value ?? String.Empty);
            }
        }


        #endregion

        #region Deserialize + Read

        // No need to override this as the behaviour is the same (create a MemoryStream and BinaryReader)
        //		public override AuthenticationProperties Deserialize( Byte[] data )
        //		{
        //			return base.Deserialize( data );
        //		}

        public override AuthenticationProperties Read(BinaryReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (reader.ReadByte() != OptimizedTicketDataFormat.FormatVersion) return null;

            Int16 count = reader.ReadInt16();
            if (count == 0) return new AuthenticationProperties();

            Dictionary<String, String> readProperties = new Dictionary<String, String>();

            for (Int32 i = 0; i < count; i++)
            {
                String key = reader.ReadString();
                String value = reader.ReadString2();

                readProperties.Add(key, value); // ASP.NET Core doesn't prevent duplicate keys either.
            }

            return new AuthenticationProperties(readProperties);
        }

        #endregion
    }
}
