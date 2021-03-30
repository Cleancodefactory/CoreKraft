using Ccf.Ck.Libs.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.Generic
{
    public class Utilities
    {
        public static bool CheckNullOrEmpty<T>(T value, bool throwException)
        {
            bool result;
            if (typeof(T) == typeof(string))
            {
                result = string.IsNullOrEmpty(value as string);
            }
            else
            {
                result = value == null || value.Equals(default(T));
            }
            if (result && throwException)
            {
                throw new NullReferenceException("Variable " + nameof(value) + "is null or empty");
            }
            return result;
        }

        public static void RestartApplication(IHostApplicationLifetime applicationLifetime, RestartReason restartReason, Action<bool> restart = null)
        {
            try
            {
                if (applicationLifetime != null)
                {
                    if (restartReason != null)
                    {
                        KraftLogger.LogDebug($"Method: RestartApplication: Stopping application Reason: {restartReason.Reason} additional info {restartReason.Description}");
                    }
                    applicationLifetime.StopApplication();
                    if (!applicationLifetime.ApplicationStopping.IsCancellationRequested)
                    {
                        Task.Delay(10 * 1000, applicationLifetime.ApplicationStopping);
                    }
                    restart?.Invoke(true);
                }
                else
                {
                    KraftLogger.LogDebug("Method: RestartApplication: applicationLifetime is null.");
                }
            }
            catch (Exception exception)
            {
                KraftLogger.LogError(exception, "Method: RestartApplication(IApplicationLifetime applicationLifetime)");
            }
        }

        public static string GenerateETag(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                return WebEncoders.Base64UrlEncode(md5.ComputeHash(data));
            }
        }

        public static string WithQuotes(string value)
        {
            return $"\"{value}\"";
        }

        public static bool HasWritePermissionOnDir(DirectoryInfo dirInfo, bool recursive = false)
        {
            bool hasWritePermissions = false;
            try
            {
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                FileIOPermission directoryPermission = new FileIOPermission(FileIOPermissionAccess.Write, dirInfo.FullName);
                try
                {
                    directoryPermission.Demand();
                    if (recursive)
                    {
                        foreach (FileInfo file in dirInfo.GetFiles())
                        {
                            directoryPermission = new FileIOPermission(FileIOPermissionAccess.Write, file.FullName);
                            directoryPermission.Demand();
                        }
                        foreach (DirectoryInfo directory in dirInfo.GetDirectories())
                        {
                            return HasWritePermissionOnDir(directory, recursive);
                        }
                    }
                    hasWritePermissions = true;
                }
                catch (SecurityException)
                {
                    return hasWritePermissions;
                }
            }
            catch (Exception)
            {
                return hasWritePermissions;
            }
            return hasWritePermissions;
        }

        public class RestartReason
        {
            public string Reason { get; set; }
            public string Description { get; set; }
        }
    }
}
