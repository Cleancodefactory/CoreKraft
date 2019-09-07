using System;

namespace Ccf.Ck.Models.Enumerations
{
    /// <summary>
    /// This enumeration serves as more abstract replacement of DbType related enumerations.
    /// The enumeration aims to specify less types and generalize them in a form which is more adequate 
    /// for extendable system that in theory may support simultaneously a number of storages - databases, files, remote services,
    /// which undertand types in different ways and the components of the system want to avoid dealing with that all the time.
    /// Through this unified enumeration the types are generalized and only the DataLoaders (sometimes the custom plugins too)
    /// have to use types specific to certain storage and only whne dealing directly with it. This way the data loaders have
    /// to convert the types from this enumeration to more specific ones only when unavoidable.
    /// 
    /// The types specified with this enumeration are not the types actually contained in the ParameterResolverValue.Value, but the types
    /// to which these values have to be converted and stored long term and/or for database operations. So, these are instructions for the 
    /// components that need to know this.
    /// 
    /// Usage:
    /// There are two enums denoting the Type and Size respectively
    /// The usage of ...Size enum is optional.
    /// The goal is to stimulate generic data storage description where types that need size (like texts) or can be represented by storage
    /// types supporting different domains or precision. It is supported to specify concrete sizes where needed, but database schemas (for example)
    /// can be operated in more generic manner in which types are not explicitly specified from the point of view of the code. Insted they are
    /// only "nuanced" as smaller, bigger and unlimited and only the storage components know the exact types under which they are stored.
    /// Anyway - this is stimulated, but not required. Being generalized such approach will contribute to the portability of the software that adheres
    /// to the principle and we recommend you to go that way if it is possible and acceptable.
    /// </summary>
    [Flags]
    public enum EValueDataType
    {
        any         = 0x0000,

        Int         = 0x000001,

        Text        = 0x000002,

        Real        = 0x000003,

        Decimal     = 0x000004,

        Fixed       = 0x000005,

        Binary      = 0x000006,

        Byte        = 0x000007,

        Boolean     = 0x000008,

        Bits        = 0x000009,

        Date        = 0x00000A,

        DateTime    = 0x00000B,

        Time        = 0x00000C,

        TimeInterval= 0x00000D,

        UUID        = 0x00000E,
        
        UInt        = 0x00000F,

        // Advanced types
        AdvancedMask = 0x00FF00, // reserved mask for comparison and extracting the advanced field type
        Collection = 0x000100 // Must support IEnumerable of the type (flags above) specified - no other conditions

    }
    /// <summary>
    /// The sizes are not applicable to all types and should be ignored in such cases (All types that do not have variations).
    /// </summary>
    [Flags]
    public enum EValueDataSize {
        Normal      = 0x000000,

        Short       = 0x010000,
        Small       = 0x010000,

        Long        = 0x100000,
        Big         = 0x100000,

        Unlimited   = 0x110000,
        Memo        = 0x110000

    }
    

}
