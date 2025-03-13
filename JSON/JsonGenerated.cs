using System.Collections.Generic;
using System.Text.Json.Serialization;
using LocalAdmin.V2.JSON.Objects;

namespace LocalAdmin.V2.JSON;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(Dictionary<string, PluginAlias>))]
[JsonSerializable(typeof(DataJson))]
[JsonSerializable(typeof(ServerPluginsConfig))]
internal partial class JsonGenerated : JsonSerializerContext;