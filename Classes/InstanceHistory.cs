﻿using Bluscream;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace InstanceHistory {
    public partial class InstanceHistoryEntry {
        [JsonProperty("WorldID", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? WorldId { get; set; }

        [JsonProperty("LastJoined", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastJoined { get; set; }
    }

    public partial class InstanceHistoryEntry {
        public static Dictionary<string, InstanceHistoryEntry> FromJson(string json) {
            return JsonConvert.DeserializeObject<Dictionary<string, InstanceHistoryEntry>>(json, Converter.Settings);
        }
    }

    public static class Serialize {
        public static string ToJson(this Dictionary<string, InstanceHistoryEntry> self) {
            return JsonConvert.SerializeObject(self, Converter.Settings);
        }
    }

    internal static class Converter {
        public static readonly JsonSerializerSettings Settings = new() {
            Formatting = Formatting.Indented,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
    public static class InstanceHistory {
        public static FileInfo filePath;
        public static Dictionary<string, InstanceHistoryEntry> Instances { get; set; } = new Dictionary<string, InstanceHistoryEntry>();
        public static void Init(string path) {
            filePath = new FileInfo(path);
            Load(filePath);
        }

        public static void Add(string worldId, string instanceId) {
            if (Instances.Count > (int)Main.HistoryFileLimit.BoxedValue) {
                _ = Instances.PopFirst();
            }

            Instances[instanceId] = new InstanceHistoryEntry() { WorldId = Guid.Parse(worldId), LastJoined = DateTime.Now };
            Save(filePath, Instances);
        }
        public static void Remove(string worldId, string instanceId) {
            throw new NotImplementedException();
        }
        public static void Load(FileInfo file) {
            try {
                if (file.Exists) {
                    Instances = InstanceHistoryEntry.FromJson(file.ReadAllText());
                    MelonLogger.Warning("Loaded {0} Instance History Entries from {1}", Instances.Count, file);
                }
            } catch (JsonSerializationException ex) {
                MelonLogger.Warning("Failed to load \"{0}\": {1}", file, ex.Message);
                file.MoveTo(file.FullName + ".bak");
            }
        }
        public static void Save(FileInfo path, Dictionary<string, InstanceHistoryEntry> list) {
            using StreamWriter writer = new(path.FullName);
            writer.WriteLine(list.ToJson());
        }
    }
}
