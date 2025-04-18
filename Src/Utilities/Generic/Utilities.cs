﻿using Ccf.Ck.Libs.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.Generic
{
    public class Utilities
    {
        #region Functions
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

        public static bool CheckNullOrEmpty<TSource, TProperty>(TSource source, Expression<Func<TSource, TProperty>> propertyLambda, bool throwException)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda.ToString()}' refers to a method, not a property.");
            }
            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda.ToString()}' refers to a field, not a property.");
            }
            if (!propInfo.ReflectedType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"Expression '{propertyLambda.ToString()}' refers to a property that is not from type {type}.");
            }

            object value = propInfo.GetValue(source);
            bool result = false;
            if (value != null && value.GetType() == typeof(string))
            {
                result = string.IsNullOrEmpty(value as string);
            }
            if (result && throwException)
            {
                throw new NullReferenceException($"Variable {propInfo.Name} is null or empty");
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
                string testFileName = Path.Combine(dirInfo.FullName, Guid.NewGuid().ToString());
                try
                {
                    using (FileStream stream = new FileStream(testFileName, FileMode.Create, FileAccess.Write, FileShare.Write, 4096))
                    {
                        var bytes = Encoding.UTF8.GetBytes("");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                finally
                {
                    File.Delete(testFileName);
                }                
                
                try
                {
                    if (recursive)
                    {
                        foreach (DirectoryInfo directory in dirInfo.GetDirectories())
                        {
                            hasWritePermissions = HasWritePermissionOnDir(directory, recursive);
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

        public static Assembly LoadAssembly(List<string> dirs, string additionalPath, string fileName, string requestingAssembly)
        {
            bool found = false;
            foreach (string dir in dirs)
            {
                string asmFullName = Path.Combine(dir, additionalPath, fileName);

                try
                {
                    if (File.Exists(asmFullName))
                    {
                        Assembly loadedAssembly = Assembly.LoadFile(asmFullName);
                        if (loadedAssembly != null)
                        {
                            found = true;
                            return loadedAssembly;
                        }
                    }
                    else
                    {
                        KraftLogger.LogError($"Method: LoadAssembly(FileExists): The file {asmFullName} requested by {requestingAssembly} was not found!");
                    }
                }
                catch
                {
                    //do nothing
                    continue;
                }
                finally
                {
                    if (!found)
                    {
                        KraftLogger.LogError($"Method: LoadAssembly: The file {asmFullName} requested by {requestingAssembly} was not found!");
                    }
                }
            }
            return null;
        }

        public class RestartReason
        {
            public string Reason { get; set; }
            public string Description { get; set; }
        }
        #endregion
    }
}
