using System;
using System.Linq;

namespace Ccf.Ck.Models.KraftModule
{
    internal class KraftModuleVersion
    {
        private string _ModuleVersion;
        private string _RequiredVersion;

        public KraftModuleVersion(string moduleVersion, string requiredVersion)
        {
            _ModuleVersion = moduleVersion;
            _RequiredVersion = requiredVersion;
        }

        internal bool IsEqualOrHigher()
        {
            if (_RequiredVersion.Contains("-") || _RequiredVersion.Contains("||") || _RequiredVersion.Contains("<") || _RequiredVersion.Contains(">"))
            {
                throw new Exception($"Currently we don't support ranges in the version: {_RequiredVersion}");
            }
            Version requiredVersionParsed;
            Version moduleVersionParsed;
            //^2.2.1 include everything greater than a particular version in the same major range
            if (_RequiredVersion.FirstOrDefault(c => c == '^').Equals('^'))
            {
                _RequiredVersion = _RequiredVersion.Replace("^", string.Empty);
                requiredVersionParsed = new Version(_RequiredVersion);
                moduleVersionParsed = new Version(_ModuleVersion);
                return moduleVersionParsed.CompareTo(requiredVersionParsed, 1) >= 0;
            }
            //~2.2.0 include everything greater than a particular version in the same minor range
            if (_RequiredVersion.FirstOrDefault(c => c == '~').Equals('~'))
            {
                _RequiredVersion = _RequiredVersion.Replace("~", string.Empty);
                requiredVersionParsed = new Version(_RequiredVersion);
                moduleVersionParsed = new Version(_ModuleVersion);
                return moduleVersionParsed.CompareTo(requiredVersionParsed, 2) >= 0;
            }

            return false;
        }
    }

    public static class VersionExtensions
    {
        public static int CompareTo(this Version version, Version otherVersion, int significantParts)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (otherVersion == null)
            {
                return 1;
            }

            if (version.Major != otherVersion.Major && significantParts >= 1)
                if (version.Major > otherVersion.Major)
                    return 1;
                else
                    return -1;

            if (version.Minor != otherVersion.Minor && significantParts >= 2)
                if (version.Minor > otherVersion.Minor)
                    return 1;
                else
                    return -1;

            if (version.Build != otherVersion.Build && significantParts >= 3)
                if (version.Build > otherVersion.Build)
                    return 1;
                else
                    return -1;

            if (version.Revision != otherVersion.Revision && significantParts >= 4)
                if (version.Revision > otherVersion.Revision)
                    return 1;
                else
                    return -1;

            return 0;
        }
    }
}
