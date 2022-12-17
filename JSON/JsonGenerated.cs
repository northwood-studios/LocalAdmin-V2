#pragma warning disable

namespace Utf8Json.Resolvers
{
    using System;
    using Utf8Json;

    public class GeneratedResolver : global::Utf8Json.IJsonFormatterResolver
    {
        public static readonly global::Utf8Json.IJsonFormatterResolver Instance = new GeneratedResolver();

        GeneratedResolver()
        {

        }

        public global::Utf8Json.IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly global::Utf8Json.IJsonFormatter<T> formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    formatter = (global::Utf8Json.IJsonFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(14)
            {
                {typeof(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.InstalledPlugin>), 0 },
                {typeof(global::System.Collections.Generic.List<string>), 1 },
                {typeof(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.Dependency>), 2 },
                {typeof(global::System.Collections.Generic.List<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>), 3 },
                {typeof(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginVersionCache>), 4 },
                {typeof(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginAlias>), 5 },
                {typeof(global::LocalAdmin.V2.PluginsManager.InstalledPlugin), 6 },
                {typeof(global::LocalAdmin.V2.PluginsManager.Dependency), 7 },
                {typeof(global::LocalAdmin.V2.PluginsManager.ServerPluginsConfig), 8 },
                {typeof(global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset), 9 },
                {typeof(global::LocalAdmin.V2.PluginsManager.GitHubRelease), 10 },
                {typeof(global::LocalAdmin.V2.Core.PluginVersionCache), 11 },
                {typeof(global::LocalAdmin.V2.Core.PluginAlias), 12 },
                {typeof(global::LocalAdmin.V2.Core.DataJson), 13 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key)) return null;

            switch (key)
            {
                case 0: return new global::Utf8Json.Formatters.DictionaryFormatter<string, global::LocalAdmin.V2.PluginsManager.InstalledPlugin>();
                case 1: return new global::Utf8Json.Formatters.ListFormatter<string>();
                case 2: return new global::Utf8Json.Formatters.DictionaryFormatter<string, global::LocalAdmin.V2.PluginsManager.Dependency>();
                case 3: return new global::Utf8Json.Formatters.ListFormatter<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>();
                case 4: return new global::Utf8Json.Formatters.DictionaryFormatter<string, global::LocalAdmin.V2.Core.PluginVersionCache>();
                case 5: return new global::Utf8Json.Formatters.DictionaryFormatter<string, global::LocalAdmin.V2.Core.PluginAlias>();
                case 6: return new Utf8Json.Formatters.LocalAdmin.V2.PluginsManager.InstalledPluginFormatter();
                case 7: return new Utf8Json.Formatters.LocalAdmin.V2.PluginsManager.DependencyFormatter();
                case 8: return new Utf8Json.Formatters.LocalAdmin.V2.PluginsManager.ServerPluginsConfigFormatter();
                case 9: return new Utf8Json.Formatters.LocalAdmin.V2.PluginsManager.GitHubReleaseAssetFormatter();
                case 10: return new Utf8Json.Formatters.LocalAdmin.V2.PluginsManager.GitHubReleaseFormatter();
                case 11: return new Utf8Json.Formatters.LocalAdmin.V2.Core.PluginVersionCacheFormatter();
                case 12: return new Utf8Json.Formatters.LocalAdmin.V2.Core.PluginAliasFormatter();
                case 13: return new Utf8Json.Formatters.LocalAdmin.V2.Core.DataJsonFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning disable 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.LocalAdmin.V2.PluginsManager
{
    using System;
    using Utf8Json;


    public sealed class InstalledPluginFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.PluginsManager.InstalledPlugin>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public InstalledPluginFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("TargetVersion"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("CurrentVersion"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("FileHash"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InstallationDate"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("UpdateDate"), 4},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("TargetVersion"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("CurrentVersion"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("FileHash"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InstallationDate"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("UpdateDate"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.PluginsManager.InstalledPlugin value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.TargetVersion, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.CurrentVersion, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.FileHash, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.InstallationDate, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.UpdateDate, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.PluginsManager.InstalledPlugin Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __TargetVersion__ = default(string?);
            var __TargetVersion__b__ = false;
            var __CurrentVersion__ = default(string?);
            var __CurrentVersion__b__ = false;
            var __FileHash__ = default(string?);
            var __FileHash__b__ = false;
            var __InstallationDate__ = default(global::System.DateTime);
            var __InstallationDate__b__ = false;
            var __UpdateDate__ = default(global::System.DateTime);
            var __UpdateDate__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __TargetVersion__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __TargetVersion__b__ = true;
                        break;
                    case 1:
                        __CurrentVersion__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __CurrentVersion__b__ = true;
                        break;
                    case 2:
                        __FileHash__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __FileHash__b__ = true;
                        break;
                    case 3:
                        __InstallationDate__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __InstallationDate__b__ = true;
                        break;
                    case 4:
                        __UpdateDate__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __UpdateDate__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.PluginsManager.InstalledPlugin(__TargetVersion__, __CurrentVersion__, __FileHash__, __InstallationDate__, __UpdateDate__);
            if(__TargetVersion__b__) ____result.TargetVersion = __TargetVersion__;
            if(__CurrentVersion__b__) ____result.CurrentVersion = __CurrentVersion__;
            if(__FileHash__b__) ____result.FileHash = __FileHash__;
            if(__InstallationDate__b__) ____result.InstallationDate = __InstallationDate__;
            if(__UpdateDate__b__) ____result.UpdateDate = __UpdateDate__;

            return ____result;
        }
    }


    public sealed class DependencyFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.PluginsManager.Dependency>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public DependencyFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("FileHash"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InstallationDate"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("UpdateDate"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("ManuallyInstalled"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InstalledByPlugins"), 4},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("FileHash"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InstallationDate"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("UpdateDate"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ManuallyInstalled"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InstalledByPlugins"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.PluginsManager.Dependency value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.FileHash, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.InstallationDate, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.UpdateDate, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            writer.WriteBoolean(value.ManuallyInstalled);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Serialize(ref writer, value.InstalledByPlugins, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.PluginsManager.Dependency Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __FileHash__ = default(string?);
            var __FileHash__b__ = false;
            var __InstallationDate__ = default(global::System.DateTime);
            var __InstallationDate__b__ = false;
            var __UpdateDate__ = default(global::System.DateTime);
            var __UpdateDate__b__ = false;
            var __ManuallyInstalled__ = default(bool);
            var __ManuallyInstalled__b__ = false;
            var __InstalledByPlugins__ = default(global::System.Collections.Generic.List<string>);
            var __InstalledByPlugins__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __FileHash__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __FileHash__b__ = true;
                        break;
                    case 1:
                        __InstallationDate__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __InstallationDate__b__ = true;
                        break;
                    case 2:
                        __UpdateDate__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __UpdateDate__b__ = true;
                        break;
                    case 3:
                        __ManuallyInstalled__ = reader.ReadBoolean();
                        __ManuallyInstalled__b__ = true;
                        break;
                    case 4:
                        __InstalledByPlugins__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<string>>().Deserialize(ref reader, formatterResolver);
                        __InstalledByPlugins__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.PluginsManager.Dependency(__FileHash__, __InstallationDate__, __UpdateDate__, __ManuallyInstalled__, __InstalledByPlugins__);
            if(__FileHash__b__) ____result.FileHash = __FileHash__;
            if(__InstallationDate__b__) ____result.InstallationDate = __InstallationDate__;
            if(__UpdateDate__b__) ____result.UpdateDate = __UpdateDate__;
            if(__ManuallyInstalled__b__) ____result.ManuallyInstalled = __ManuallyInstalled__;
            if(__InstalledByPlugins__b__) ____result.InstalledByPlugins = __InstalledByPlugins__;

            return ____result;
        }
    }


    public sealed class ServerPluginsConfigFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.PluginsManager.ServerPluginsConfig>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public ServerPluginsConfigFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("InstalledPlugins"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Dependencies"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("LastUpdateCheck"), 2},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("InstalledPlugins"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Dependencies"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("LastUpdateCheck"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.PluginsManager.ServerPluginsConfig value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.InstalledPlugin>>().Serialize(ref writer, value.InstalledPlugins, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.Dependency>>().Serialize(ref writer, value.Dependencies, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Serialize(ref writer, value.LastUpdateCheck, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.PluginsManager.ServerPluginsConfig Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __InstalledPlugins__ = default(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.InstalledPlugin>);
            var __InstalledPlugins__b__ = false;
            var __Dependencies__ = default(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.Dependency>);
            var __Dependencies__b__ = false;
            var __LastUpdateCheck__ = default(global::System.DateTime?);
            var __LastUpdateCheck__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __InstalledPlugins__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.InstalledPlugin>>().Deserialize(ref reader, formatterResolver);
                        __InstalledPlugins__b__ = true;
                        break;
                    case 1:
                        __Dependencies__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.PluginsManager.Dependency>>().Deserialize(ref reader, formatterResolver);
                        __Dependencies__b__ = true;
                        break;
                    case 2:
                        __LastUpdateCheck__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Deserialize(ref reader, formatterResolver);
                        __LastUpdateCheck__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.PluginsManager.ServerPluginsConfig(__InstalledPlugins__, __Dependencies__, __LastUpdateCheck__);
            if(__InstalledPlugins__b__) ____result.InstalledPlugins = __InstalledPlugins__;
            if(__Dependencies__b__) ____result.Dependencies = __Dependencies__;
            if(__LastUpdateCheck__b__) ____result.LastUpdateCheck = __LastUpdateCheck__;

            return ____result;
        }
    }


    public sealed class GitHubReleaseAssetFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public GitHubReleaseAssetFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("name"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("browser_download_url"), 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("name"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("browser_download_url"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteString(value.name);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteString(value.browser_download_url);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }
            

            var __name__ = default(string);
            var __name__b__ = false;
            var __browser_download_url__ = default(string);
            var __browser_download_url__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __name__ = reader.ReadString();
                        __name__b__ = true;
                        break;
                    case 1:
                        __browser_download_url__ = reader.ReadString();
                        __browser_download_url__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset(__name__, __browser_download_url__);

            return ____result;
        }
    }


    public sealed class GitHubReleaseFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.PluginsManager.GitHubRelease>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public GitHubReleaseFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("message"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("id"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("tag_name"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("published_at"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("assets"), 4},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("message"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("id"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("tag_name"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("published_at"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("assets"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.PluginsManager.GitHubRelease value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.message, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteUInt32(value.id);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.tag_name, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.published_at, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>>().Serialize(ref writer, value.assets, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.PluginsManager.GitHubRelease Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }
            

            var __message__ = default(string?);
            var __message__b__ = false;
            var __id__ = default(uint);
            var __id__b__ = false;
            var __tag_name__ = default(string?);
            var __tag_name__b__ = false;
            var __published_at__ = default(global::System.DateTime);
            var __published_at__b__ = false;
            var __assets__ = default(global::System.Collections.Generic.List<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>);
            var __assets__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __message__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __message__b__ = true;
                        break;
                    case 1:
                        __id__ = reader.ReadUInt32();
                        __id__b__ = true;
                        break;
                    case 2:
                        __tag_name__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __tag_name__b__ = true;
                        break;
                    case 3:
                        __published_at__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __published_at__b__ = true;
                        break;
                    case 4:
                        __assets__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::LocalAdmin.V2.PluginsManager.GitHubReleaseAsset>>().Deserialize(ref reader, formatterResolver);
                        __assets__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.PluginsManager.GitHubRelease(__message__, __id__, __tag_name__, __published_at__, __assets__);

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 219
#pragma warning disable 168

namespace Utf8Json.Formatters.LocalAdmin.V2.Core
{
    using System;
    using Utf8Json;


    public sealed class PluginVersionCacheFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.Core.PluginVersionCache>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public PluginVersionCacheFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Version"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("ReleaseId"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("PublishmentTime"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("LastRefreshed"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("DllDownloadUrl"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("DependenciesDownloadUrl"), 5},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Version"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("ReleaseId"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("PublishmentTime"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("LastRefreshed"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("DllDownloadUrl"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("DependenciesDownloadUrl"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.Core.PluginVersionCache value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteString(value.Version);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteUInt32(value.ReleaseId);
            writer.WriteRaw(this.____stringByteKeys[2]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.PublishmentTime, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Serialize(ref writer, value.LastRefreshed, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            writer.WriteString(value.DllDownloadUrl);
            writer.WriteRaw(this.____stringByteKeys[5]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.DependenciesDownloadUrl, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.Core.PluginVersionCache Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }
            

            var __Version__ = default(string);
            var __Version__b__ = false;
            var __ReleaseId__ = default(uint);
            var __ReleaseId__b__ = false;
            var __PublishmentTime__ = default(global::System.DateTime);
            var __PublishmentTime__b__ = false;
            var __LastRefreshed__ = default(global::System.DateTime);
            var __LastRefreshed__b__ = false;
            var __DllDownloadUrl__ = default(string);
            var __DllDownloadUrl__b__ = false;
            var __DependenciesDownloadUrl__ = default(string?);
            var __DependenciesDownloadUrl__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Version__ = reader.ReadString();
                        __Version__b__ = true;
                        break;
                    case 1:
                        __ReleaseId__ = reader.ReadUInt32();
                        __ReleaseId__b__ = true;
                        break;
                    case 2:
                        __PublishmentTime__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __PublishmentTime__b__ = true;
                        break;
                    case 3:
                        __LastRefreshed__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime>().Deserialize(ref reader, formatterResolver);
                        __LastRefreshed__b__ = true;
                        break;
                    case 4:
                        __DllDownloadUrl__ = reader.ReadString();
                        __DllDownloadUrl__b__ = true;
                        break;
                    case 5:
                        __DependenciesDownloadUrl__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __DependenciesDownloadUrl__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.Core.PluginVersionCache(__Version__, __ReleaseId__, __PublishmentTime__, __LastRefreshed__, __DllDownloadUrl__, __DependenciesDownloadUrl__);
            if(__Version__b__) ____result.Version = __Version__;
            if(__ReleaseId__b__) ____result.ReleaseId = __ReleaseId__;
            if(__PublishmentTime__b__) ____result.PublishmentTime = __PublishmentTime__;
            if(__LastRefreshed__b__) ____result.LastRefreshed = __LastRefreshed__;
            if(__DllDownloadUrl__b__) ____result.DllDownloadUrl = __DllDownloadUrl__;
            if(__DependenciesDownloadUrl__b__) ____result.DependenciesDownloadUrl = __DependenciesDownloadUrl__;

            return ____result;
        }
    }


    public sealed class PluginAliasFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.Core.PluginAlias>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public PluginAliasFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Repository"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("Flags"), 1},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Repository"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Flags"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.Core.PluginAlias value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            writer.WriteString(value.Repository);
            writer.WriteRaw(this.____stringByteKeys[1]);
            writer.WriteByte(value.Flags);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.Core.PluginAlias Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                throw new InvalidOperationException("typecode is null, struct not supported");
            }
            

            var __Repository__ = default(string);
            var __Repository__b__ = false;
            var __Flags__ = default(byte);
            var __Flags__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __Repository__ = reader.ReadString();
                        __Repository__b__ = true;
                        break;
                    case 1:
                        __Flags__ = reader.ReadByte();
                        __Flags__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.Core.PluginAlias(__Repository__, __Flags__);

            return ____result;
        }
    }


    public sealed class DataJsonFormatter : global::Utf8Json.IJsonFormatter<global::LocalAdmin.V2.Core.DataJson>
    {
        readonly global::Utf8Json.Internal.AutomataDictionary ____keyMapping;
        readonly byte[][] ____stringByteKeys;

        public DataJsonFormatter()
        {
            this.____keyMapping = new global::Utf8Json.Internal.AutomataDictionary()
            {
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("GitHubPersonalAccessToken"), 0},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("EulaAccepted"), 1},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("PluginManagerWarningDismissed"), 2},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("LastPluginAliasesRefresh"), 3},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("PluginVersionCache"), 4},
                { JsonWriter.GetEncodedPropertyNameWithoutQuotation("PluginAliases"), 5},
            };

            this.____stringByteKeys = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("GitHubPersonalAccessToken"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("EulaAccepted"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("PluginManagerWarningDismissed"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("LastPluginAliasesRefresh"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("PluginVersionCache"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("PluginAliases"),
                
            };
        }

        public void Serialize(ref JsonWriter writer, global::LocalAdmin.V2.Core.DataJson value, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            

            writer.WriteRaw(this.____stringByteKeys[0]);
            formatterResolver.GetFormatterWithVerify<string?>().Serialize(ref writer, value.GitHubPersonalAccessToken, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[1]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Serialize(ref writer, value.EulaAccepted, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[2]);
            writer.WriteBoolean(value.PluginManagerWarningDismissed);
            writer.WriteRaw(this.____stringByteKeys[3]);
            formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Serialize(ref writer, value.LastPluginAliasesRefresh, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[4]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginVersionCache>>().Serialize(ref writer, value.PluginVersionCache, formatterResolver);
            writer.WriteRaw(this.____stringByteKeys[5]);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginAlias>>().Serialize(ref writer, value.PluginAliases, formatterResolver);
            
            writer.WriteEndObject();
        }

        public global::LocalAdmin.V2.Core.DataJson Deserialize(ref JsonReader reader, global::Utf8Json.IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            

            var __GitHubPersonalAccessToken__ = default(string?);
            var __GitHubPersonalAccessToken__b__ = false;
            var __EulaAccepted__ = default(global::System.DateTime?);
            var __EulaAccepted__b__ = false;
            var __PluginManagerWarningDismissed__ = default(bool);
            var __PluginManagerWarningDismissed__b__ = false;
            var __LastPluginAliasesRefresh__ = default(global::System.DateTime?);
            var __LastPluginAliasesRefresh__b__ = false;
            var __PluginVersionCache__ = default(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginVersionCache>);
            var __PluginVersionCache__b__ = false;
            var __PluginAliases__ = default(global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginAlias>);
            var __PluginAliases__b__ = false;

            var ____count = 0;
            reader.ReadIsBeginObjectWithVerify();
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref ____count))
            {
                var stringKey = reader.ReadPropertyNameSegmentRaw();
                int key;
                if (!____keyMapping.TryGetValueSafe(stringKey, out key))
                {
                    reader.ReadNextBlock();
                    goto NEXT_LOOP;
                }

                switch (key)
                {
                    case 0:
                        __GitHubPersonalAccessToken__ = formatterResolver.GetFormatterWithVerify<string?>().Deserialize(ref reader, formatterResolver);
                        __GitHubPersonalAccessToken__b__ = true;
                        break;
                    case 1:
                        __EulaAccepted__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Deserialize(ref reader, formatterResolver);
                        __EulaAccepted__b__ = true;
                        break;
                    case 2:
                        __PluginManagerWarningDismissed__ = reader.ReadBoolean();
                        __PluginManagerWarningDismissed__b__ = true;
                        break;
                    case 3:
                        __LastPluginAliasesRefresh__ = formatterResolver.GetFormatterWithVerify<global::System.DateTime?>().Deserialize(ref reader, formatterResolver);
                        __LastPluginAliasesRefresh__b__ = true;
                        break;
                    case 4:
                        __PluginVersionCache__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginVersionCache>>().Deserialize(ref reader, formatterResolver);
                        __PluginVersionCache__b__ = true;
                        break;
                    case 5:
                        __PluginAliases__ = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.Dictionary<string, global::LocalAdmin.V2.Core.PluginAlias>>().Deserialize(ref reader, formatterResolver);
                        __PluginAliases__b__ = true;
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }

                NEXT_LOOP:
                continue;
            }

            var ____result = new global::LocalAdmin.V2.Core.DataJson(__GitHubPersonalAccessToken__, __EulaAccepted__, __PluginManagerWarningDismissed__, __LastPluginAliasesRefresh__, __PluginVersionCache__, __PluginAliases__);
            if(__GitHubPersonalAccessToken__b__) ____result.GitHubPersonalAccessToken = __GitHubPersonalAccessToken__;
            if(__EulaAccepted__b__) ____result.EulaAccepted = __EulaAccepted__;
            if(__PluginManagerWarningDismissed__b__) ____result.PluginManagerWarningDismissed = __PluginManagerWarningDismissed__;
            if(__LastPluginAliasesRefresh__b__) ____result.LastPluginAliasesRefresh = __LastPluginAliasesRefresh__;
            if(__PluginVersionCache__b__) ____result.PluginVersionCache = __PluginVersionCache__;
            if(__PluginAliases__b__) ____result.PluginAliases = __PluginAliases__;

            return ____result;
        }
    }

}

#pragma warning disable 168
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612
