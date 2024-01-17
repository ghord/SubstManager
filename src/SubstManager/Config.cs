using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static SubstManager.Log;

namespace SubstManager
{
    public enum State
    {
        Remote,
        Local
    }

    public class Config
    {
        private Dictionary<string, JToken> values_;

        public const string Aliases = "aliases";
        public const string ActiveAlias = "active";
        public const string AliasLocals = "aliases.locals";
        public const string AliasStates = "aliases.states";
        public const string Drive = "subst.drive";
        public const string CacheDirectory = "cache.directory";

        public static IEnumerable<string> PublicKeys { get; } =
            new[] { Drive, CacheDirectory };

        private Config(JObject obj)
        {
            values_ = obj.Properties().ToDictionary(p => p.Name, p => p.Value);
        }

        public static Config LoadFrom(string path)
        {
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var obj = (JObject)JToken.Load(jsonReader);

                return new Config(obj);
            }
        }

        private static string GetConfigurationFilePath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var configurationDirectory = Path.Combine(localAppData, "SubstManager");

            if (!Directory.Exists(configurationDirectory))
            {
                Directory.CreateDirectory(configurationDirectory);
                Info("Created missing configuration directory: " + configurationDirectory);
            }

            var configurationPath = Path.Combine(configurationDirectory, "config.json");
            return configurationPath;
        }

        public void Save()
        {
            var path = GetConfigurationFilePath();

            SaveTo(path);

            Verbose($"Saved configuration file: " + path);
        }


        public static Config Load()
        {
            try
            {
                string configurationPath = GetConfigurationFilePath();

                if (!File.Exists(configurationPath))
                {
                    using (var file = File.Create(configurationPath))
                    using (var writer = new StreamWriter(file, Encoding.UTF8))
                    {
                        writer.Write("{}");
                    }

                    Info("Created missing configuration file: " + configurationPath);
                }

                Verbose("Loaded configuration from: " + configurationPath);

                return Config.LoadFrom(configurationPath);
            }
            catch (Exception e)
            {
                Error("Failed to load configuration file: " + e.Message);
                Environment.Exit(1);

                throw;
            }
        }

        public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value) 
        {
            if (values_.TryGetValue(key, out var token))
            {
                value = token.ToObject<T>();

                if(value != null)
                {
                    return true;
                }

                return false;
            }

            value = default(T);
            return false;
        }

        public bool TryGetValueDictionary<T>(string key, [NotNullWhen(true)]  out  Dictionary<string, T>? value)
        {
            if (values_.TryGetValue(key, out var token))
            {
                value = token.ToObject<Dictionary<string, T>?>();

                if (value != null)
                {
                    return true;
                }

                return false;
            }

            value = null;
            return false;
        }

        public void SetValue<T>(string key, T value) where T : notnull
        {
            values_[key] = JToken.FromObject(value);
        }

        public void SaveTo(string path)
        {
            var result = new JObject();

            foreach (var value in values_)
            {
                result.Add(value.Key, value.Value);
            }

            using (var file = File.Open(path, FileMode.Create))
            using (var writer = new StreamWriter(file, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented })
            {
                result.WriteTo(jsonWriter);
            }
        }

        public bool TryGetValueRaw(string key, [NotNullWhen(true)] out string? value)
        {
            if(values_.TryGetValue(key, out var token))
            {
                value = token.ToString(Formatting.None);
                return true;
            }

            value = null;
            return false;
        }
    }
}
