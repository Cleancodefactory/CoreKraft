using Ccf.Ck.Libs.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ccf.Ck.Web.Middleware
{
    //https://copyprogramming.com/howto/make-asp-net-core-server-kestrel-case-sensitive-on-windows#make-aspnet-core-server-kestrel-case-sensitive-on-windows
    public class CaseAwarePhysicalFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider _provider;
        //holds all of the actual paths to the required files
        private static Dictionary<string, string> _paths;
        public bool CaseSensitive { get; set; } = false;
        public CaseAwarePhysicalFileProvider(string root)
        {
            _provider = new PhysicalFileProvider(root);
            _paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        public CaseAwarePhysicalFileProvider(string root, ExclusionFilters filters)
        {
            _provider = new PhysicalFileProvider(root, filters);
            _paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        public IFileInfo GetFileInfo(string subpath)
        {
            var actualPath = GetActualFilePath(subpath);
            if (CaseSensitive && "/" + actualPath.Replace(Path.DirectorySeparatorChar, '/') != subpath)
            {
                KraftLogger.LogError($"File or Directory not found: {subpath}. Please check casing!");
                return new NotFoundFileInfo(subpath);
            }
            return _provider.GetFileInfo(actualPath);
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var actualPath = GetActualFilePath(subpath);
            if (CaseSensitive && actualPath != subpath)
            {
                KraftLogger.LogError($"Directory not found: {subpath}. Please check casing!");
                return NotFoundDirectoryContents.Singleton;
            }
            return _provider.GetDirectoryContents(actualPath);
        }
        public IChangeToken Watch(string filter) => _provider.Watch(filter);
        // Determines (and caches) the actual path for a file
        private string GetActualFilePath(string path)
        {
            // Check if this has already been matched before
            if (_paths.ContainsKey(path)) return _paths[path];
            // Break apart the path and get the root folder to work from
            var currPath = _provider.Root;
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            // Start stepping up the folders to replace with the correct cased folder name
            for (var i = 0; i < segments.Length; i++)
            {
                var part = segments[i];
                var last = i == segments.Length - 1;
                // Ignore the root
                if (part.Equals("~")) continue;
                // Process the file name if this is the last segment
                part = last ? GetFileName(part, currPath) : GetDirectoryName(part, currPath);
                // If no matches were found, just return the original string
                if (part == null) return path;
                // Update the actualPath with the correct name casing
                currPath = Path.Combine(currPath, part);
                segments[i] = part;
            }
            // Save this path for later use
            var actualPath = string.Join(Path.DirectorySeparatorChar, segments);
            _paths.Add(path, actualPath);
            return actualPath;
        }
        // Searches for a matching file name in the current directory regardless of case
        private static string GetFileName(string part, string folder) =>
            new DirectoryInfo(folder).GetFiles().FirstOrDefault(file => file.Name.Equals(part, StringComparison.OrdinalIgnoreCase))?.Name;
        // Searches for a matching folder in the current directory regardless of case
        private static string GetDirectoryName(string part, string folder) =>
            new DirectoryInfo(folder).GetDirectories().FirstOrDefault(dir => dir.Name.Equals(part, StringComparison.OrdinalIgnoreCase))?.Name;
    }
}
