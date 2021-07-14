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
        public const string StorageUriCustomKey = "StorageUri";
        public const string PrimaryStorageAccountKeyCustomKey = "PrimaryStorageAccountKey";
        public const string StorageAccountNameCustomKey = "StorageAccountName";
        public const string StorageConnectionStringCustomKey = "StorageConnectionString";
        public const string CosmosEndpointUriCustomKey = "CosmosEndpointUri";
        public const string CosmosPrimaryKeyCustomKey = "CosmosPrimaryKey";

        public static void CreateAndSaveEnvironmentVariablesFile(string ConnectionString, string filePath)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(StorageConnectionStringCustomKey, ConnectionString);

            CreateAndSaveEnvironmentVariablesFile(dict, filePath);
        }

        public static void CreateAndSaveEnvironmentVariablesFile(
            string ConnectionString, 
            string CosmosEndpointUri, 
            string CosmosPrimaryKey, 
            string filePath)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(StorageConnectionStringCustomKey, ConnectionString);
            dict.Add(CosmosEndpointUriCustomKey, CosmosEndpointUri);
            dict.Add(CosmosPrimaryKeyCustomKey, CosmosPrimaryKey);

            CreateAndSaveEnvironmentVariablesFile(dict, filePath);
        }

        public static void CreateAndSaveEnvironmentVariablesFile(Dictionary<string, string> dict, string filePath)
        {
            string privateKey = GenerateNaivePersonalPrivateKey();

            var serializedDict = JsonConvert.SerializeObject(dict);
            File.WriteAllText(filePath, AesCrypto.Encrypt(serializedDict, privateKey));
        }

        public static string LoadEnvironmentVariablesFile(string filePath)
        {
            return AesCrypto.Decrypt(File.ReadAllText(filePath), GenerateNaivePersonalPrivateKey());
        }

        private static string _environmentVariablesFilePath;
        public static string EnvironmentVariablesFilePath
        {
            get 
            {
                if (string.IsNullOrEmpty(_environmentVariablesFilePath))
                    _environmentVariablesFilePath = GetDefaultEnvironmentVariablesFilePath();

                return _environmentVariablesFilePath;
            }
            internal set
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException("path is null or empty or whitespace");
                if (Path.IsPathRooted(value))
                    throw new ArgumentException("path is not rooted");
                if (File.Exists(value))
                    throw new ArgumentException("file already exists");
                if (value.EndsWith("/"))
                    throw new ArgumentException("path is not well formed");

                _environmentVariablesFilePath = value;
            }
        }

        public static string GetDefaultEnvironmentVariablesFilePath()
        {
            return Path.Combine(GetDefaultEnvironmentVariablesFolderPath(), "_environmentVariablesInfo.json");
        }

        private static string GetDefaultEnvironmentVariablesFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static void SetEnvironmentVariablesFileName(string newFileName)
        {
            if (string.IsNullOrEmpty(newFileName) || string.IsNullOrWhiteSpace(newFileName))
                throw new ArgumentNullException("file name is null or empty or whitespace");

            EnvironmentVariablesFilePath = Path.Combine(GetDefaultEnvironmentVariablesFolderPath(), newFileName);
        }

        private static Dictionary<string, string> _environmentVariables;
        private static Dictionary<string, string> EnvironmentVariables
        {
            get
            {
                if (_environmentVariables == null)
                {
                    if (!File.Exists(EnvironmentVariablesFilePath))
                        throw new ApplicationException("Environment variables file path not found");

                    _environmentVariables = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        LoadEnvironmentVariablesFile(EnvironmentVariablesFilePath));
                    if (_environmentVariables == null)
                        throw new ApplicationException("Environment variables not found");
                }

                return _environmentVariables;
            }
        }

        public static string GenerateNaivePersonalPrivateKey()
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

        public static bool TryGetCosmosEndpointUri(out string value)
        {
            return TryGetValue(CosmosEndpointUriCustomKey, out value);
        }
        
        public static bool TryGetCosmosPrimaryKey(out string value)
        {
            return TryGetValue(CosmosPrimaryKeyCustomKey, out value);
        }

        #endregion
    }
}
