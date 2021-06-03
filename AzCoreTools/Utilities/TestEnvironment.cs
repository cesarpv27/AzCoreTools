using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoreTools.Cryptography;
using System.Net.NetworkInformation;
using System.Linq;

namespace AzCoreTools.Utilities
{
    public sealed class TestEnvironment
    {
        private const string StorageUriCustomKey = "StorageUri";
        private const string PrimaryStorageAccountKeyCustomKey = "PrimaryStorageAccountKey";
        private const string StorageAccountNameCustomKey = "StorageAccountName";
        private const string StorageConnectionStringCustomKey = "StorageConnectionString";

        private static string _environmentVariablesFilePath = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "_environmentVariablesInfo.json");
        public static void SetEnvironmentVariablesFilePath(string newEnvironmentVariablesFilePath)
        {
            _environmentVariablesFilePath = newEnvironmentVariablesFilePath;
        }

        private static Dictionary<string, string> _environmentVariables;
        private static Dictionary<string, string> EnvironmentVariables
        {
            get
            {
                if (_environmentVariables == null)
                {
                    if (!File.Exists(_environmentVariablesFilePath))
                        throw new ApplicationException("Environment variables file path not found");

                    _environmentVariables = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        AesCrypto.Decrypt(File.ReadAllText(_environmentVariablesFilePath), GenerateNaivePersonalPrivateKey()));
                    if (_environmentVariables == null)
                        throw new ApplicationException("Environment variables not found");
                }

                return _environmentVariables;
            }
        }

        private static string GenerateNaivePersonalPrivateKey()
        {
            string separator = "-";
            return $"{Environment.UserName}" +
                $"{separator}{Environment.MachineName}" +
                $"{separator}{NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault()}";
        }

        public static bool TryGetValue(string key, out string value)
        {
            return EnvironmentVariables.TryGetValue(key, out value);
        }

        #region Custom TryGet

        public static bool TryGetPrimaryStorageAccountKey(out string value)
        {
            return TryGetValue(PrimaryStorageAccountKeyCustomKey, out value);
        }

        public static bool TryGetStorageUri(out string value)
        {
            return TryGetValue(StorageUriCustomKey, out value);
        }

        public static bool TryGetStorageAccountName(out string value)
        {
            return TryGetValue(StorageAccountNameCustomKey, out value);
        }

        public static bool TryGetStorageConnectionString(out string value)
        {
            return TryGetValue(StorageConnectionStringCustomKey, out value);
        }

        #endregion
    }
}
