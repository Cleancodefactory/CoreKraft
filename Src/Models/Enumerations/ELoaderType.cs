using System;

namespace Ccf.Ck.Models.Enumerations
{
    [Flags]
    public enum ELoaderType
    {
        //VIEWS:		0x0001,
        //RESOURCES:	0x0002,
        //LOOKUPS:	0x0004,
        //RULES:		0x0008,
        //SCRIPTS:	0x0010,
        //DATA:		0x0020,
        //METADATA:	0x0040,
        //RVIEWS:		0x0080, // Read only views
        //ALL:		0xFFFF
        None = 0x0000,
        ViewLoader = 0x0001,
        ResourceLoader = 0x0002,
        LookupLoader = 0x0004,
        CustomPlugin = 0x0008,
        DataLoader = 0x0020,
        All = 0xFFFF
    }
}
