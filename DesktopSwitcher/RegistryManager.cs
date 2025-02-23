using DesktopSwitcher.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace DesktopSwitcher
{
    internal static class RegistryManager
    {
        private static RegistryKey registryKeyFromBaseName(string baseName)
        {
            return baseName switch
            {
                "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKEY_USERS" => Registry.Users,
                "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => throw new ArgumentException("Name must start with HKEY_CLASSES_ROOT, HKEY_CURRENT_USER, HKEY_LOCAL_MACHINE, HKEY_USERS, or HKEY_CURRENT_CONFIG", nameof(baseName)),
            };
        }

        /// <summary>
        /// Set a registry key value.
        /// </summary>
        /// <param name="kv">The registry key value to set.</param>
        /// <exception cref="ArgumentException">Thrown when the registry key does not exist.</exception>
        public static void SetKeyValue(RegistryKeyValue kv)
        {
            var baseName = kv.KeyPath.Split(@"\").First();
            var subKeyName = string.Join(@"\", kv.KeyPath.Split(@"\").Skip(1));

            var baseKey = registryKeyFromBaseName(baseName);

            using RegistryKey key = baseKey.OpenSubKey(
                subKeyName,
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryRights.FullControl
            )!;
            
            if (key == null) throw new ArgumentException("Registry key does not exist.", nameof(kv));
            
            key.SetValue(kv.ValueName, kv.Value, kv.ValueKind);
            key.Close();
        }

        /// <summary>
        /// Set multiple registry key values.
        /// </summary>
        /// <param name="kvs">A list of registry key values to set.</param>
        public static void SetKeyValues(IEnumerable<RegistryKeyValue> kvs)
        {
            foreach (var kv in kvs)
            {
                SetKeyValue(kv);
            }
        }

        /// <summary>
        /// Get a registry key value.
        /// </summary>
        /// <param name="keyPath">Path to the registry key.</param>
        /// <param name="valueName">Name of the value at the registry key.</param>
        /// <returns><see cref="RegistryKeyValue"/> or null if the key does not exist.</returns>
        public static RegistryKeyValue? GetKeyValue(string keyPath, string valueName)
        {
            var baseName = keyPath.Split(@"\").First();
            var subKeyName = string.Join(@"\", keyPath.Split(@"\").Skip(1));

            var baseKey = registryKeyFromBaseName(baseName);

            using RegistryKey key = baseKey.OpenSubKey(
                subKeyName,
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryRights.FullControl
            )!;

            if (key == null) return null;
            
            return new RegistryKeyValue()
                {
                    KeyPath = keyPath,
                    ValueName = valueName,
                    Value = key.GetValue(valueName),
                    ValueKind = key.GetValueKind(valueName)
                };
        }

        /// <summary>
        /// Get all values of a registry key.
        /// </summary>
        /// <param name="keyPath">Path to the registry key.</param>
        /// <returns>A list of <see cref="RegistryKeyValue"/> or null if the key does not exist.</returns>
        public static List<RegistryKeyValue>? GetAllKeyValues(string keyPath)
        {
            var baseName = keyPath.Split(@"\").First();
            var subKeyName = string.Join(@"\", keyPath.Split(@"\").Skip(1));

            var baseKey = registryKeyFromBaseName(baseName);

            using RegistryKey key = baseKey.OpenSubKey(
                subKeyName,
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryRights.FullControl
            )!;
            
            if (key == null) return null;

            return key.GetValueNames().Select(valueName => new RegistryKeyValue()
            {
                KeyPath = keyPath,
                ValueName = valueName,
                Value = key.GetValue(valueName),
                ValueKind = key.GetValueKind(valueName)
            }).ToList();
        }

        /// <summary>
        /// Delete a registry key value.
        /// </summary>
        /// <param name="keyValue">Registry key value to delete.</param>
        public static void DeleteKeyValue(string keyPath, string valueName)
        {
            var baseName = keyPath.Split(@"\").First();
            var subKeyName = string.Join(@"\", keyPath.Split(@"\").Skip(1));

            var baseKey = registryKeyFromBaseName(baseName);

            using RegistryKey key = baseKey.OpenSubKey(
                subKeyName,
                RegistryKeyPermissionCheck.ReadWriteSubTree,
                RegistryRights.FullControl
            )!;
            
            if (key == null) return;

            key.DeleteValue(valueName);
        }
    }
}
