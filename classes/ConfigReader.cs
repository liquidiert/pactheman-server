using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace pactheman_server {
    public class ConfigReader {
        public Dictionary<string, dynamic> config { get; set; }
        private static readonly Lazy<ConfigReader> lazy = new Lazy<ConfigReader>(() => new ConfigReader());
        public static ConfigReader Instance { get => lazy.Value; }
        private ConfigReader() {
            // not dry due to async :(
            var config_file_content = File.ReadAllText("Content/config.yaml");
            var yaml_input = new StringReader(config_file_content);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            this.config = deserializer.Deserialize<Dictionary<string, dynamic>>(yaml_input);
        }

        public string CurrentMoveBehavior(string ghostName) {
            return this.config["ghosts"][ghostName]["move_behavior"]["current"];
        }

        public async Task<bool> Reset() {
            try {
                var config_file_content = await File.ReadAllTextAsync("Content/config.yaml");
                var yaml_input = new StringReader(config_file_content);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                this.config = deserializer.Deserialize<Dictionary<string, dynamic>>(yaml_input);
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> Save() {
            var serializer = new SerializerBuilder().Build();
            try {
                await File.WriteAllTextAsync("Content/config.yaml", serializer.Serialize(config));
                return true;
            } catch {
                return false;
            }
        }
    }
}