using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static SubstManager.Log;

namespace SubstManager
{
    [Verb("alias", HelpText = "Manage aliases")]
    class AliasOptions
    {
        [Value(0, HelpText = "Name of the alias", MetaName = "alias")]
        public string? Name { get; set; }

        [Value(1, HelpText = "Path for alias", MetaName = "path")]
        public string? Path { get; set; }
    }

    [Verb("unalias", HelpText = "Removes an alias")]
    class UnaliasOptions
    {
        [Value(0, HelpText = "Name of the alias", MetaName = "alias", Required = true)]
        public string Name { get; set; } = string.Empty;
    }

    [Verb("config", HelpText = "Manage global configuration")]
    class ConfigOptions
    {
        [Value(0, HelpText = "Name of configuration key", MetaName = "config key")]
        public string? Key { get; set; }

        [Value(1, HelpText = "Value of configuration key", MetaName = "config value")]
        public string? Value { get; set; }
    }

    [Verb("switch", HelpText = "Switches active alias and also mounts the drive")]
    class SwitchOptions
    {
        [Value(0, HelpText = "Alias name", Required = true, MetaName = "alias")]
        public string Alias { get; set; } = string.Empty;
    }

    [Verb("mount", HelpText = "Mounts active alias")]
    class MountOptions
    {

    }

    [Verb("unmount", HelpText = "Unmounts active alias")]
    class UnmountOptions
    {

    }

    [Verb("remote", HelpText = "Switches alias to remote access")]
    class RemoteOptions
    {
        [Value(0, HelpText = "Alias name", Required = true, MetaName = "alias")]
        public string? Alias { get; set; }
    }
    

    [Verb("local", HelpText = "Switches alias to local cached access")]
    class LocalOptions
    {
        [Value(0, HelpText = "Alias name", Required = true, MetaName = "alias")]
        public string? Alias { get; set; }
    }

    [Verb("update", HelpText = "Updates files missing from local cache")]
    class UpdateOptions
    {

    }

    [Verb("fetch", HelpText = "Show changes since last update")]
    class FetchOptions
    {

    }

    [Verb("status", HelpText = "Shows the status")]
    class StatusOptions
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            var verbs = new[]
            {
                typeof(AliasOptions),
                typeof(UnaliasOptions),
                typeof(ConfigOptions),
                typeof(SwitchOptions),
                typeof(MountOptions),
                typeof(StatusOptions),
                typeof(UnmountOptions),
                typeof(RemoteOptions),
                typeof(LocalOptions),
                typeof(UpdateOptions),
                typeof(FetchOptions)
            };

            Parser.Default.ParseArguments(args, verbs)
                .WithParsed(result =>
                {
                    switch (result)
                    {
                        case AliasOptions alias:
                            RunAlias(alias);
                            return;
                        case UnaliasOptions unalias:
                            RunUnalias(unalias);
                            return;
                        case ConfigOptions config:
                            RunConfig(config);
                            return;
                        case SwitchOptions sw:
                            RunSwitch(sw);
                            return;
                        case MountOptions _:
                            RunMount();
                            return;
                        case UnmountOptions _:
                            RunUnmount();
                            return;
                        case LocalOptions local:
                            RunLocal(local);
                            return;
                        case RemoteOptions remote:
                            RunRemote(remote);
                            return;
                        case StatusOptions _:
                            RunStatus();
                            return;
                    }
                });
        }

        private static void RunStatus()
        {
            var cfg = Config.Load();

            Info("status:");
            PushIndent();

            if (cfg.TryGetValue(Config.ActiveAlias, out string alias))
            {
                Info($"active alias: {alias}");

                if (cfg.TryGetValue(Config.AliasStates, out Dictionary<string, State> states) &&
                    states.TryGetValue(alias, out var state))
                {
                    Info($"state:        {state.ToString().ToLower()}");
                }
                else
                {
                    Info("state:        remote");
                }

                if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) &&
                    aliases.TryGetValue(alias, out var path))
                {
                    Info($"remote path:  {path}");
                }
                else
                {
                    Info($"remote path:  <missing>");
                }

                if (cfg.TryGetValue(Config.AliasLocals, out Dictionary<string, string> locals) &&
                         locals.TryGetValue(alias, out var local))
                {
                    Info($"local path:   {local}");
                }
                else
                {
                    Info($"local path:   <missing>");
                }
            }
            else
            {
                Info($"active alias: <missing>");
            }

            PopIndent();
        }

        private static void RunRemote(RemoteOptions options)
        {
            throw new NotImplementedException();
        }

        private static void RunLocal(LocalOptions options)
        {
            throw new NotImplementedException();
        }

        private static void RunMount()
        {
            var cfg = Config.Load();

            if (!cfg.TryGetValue(Config.Drive, out string drive))
            {
                Error($"Missing configuration value '{Config.Drive}'");
                return;
            }

            if (!cfg.TryGetValue(Config.ActiveAlias, out string alias))
            {
                Error($"No active alias found");
                return;
            }

            if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) &&
                aliases.TryGetValue(alias, out var path) && path != null)
            {
                Info($"Mounting alias '{alias}' on drive '{drive}' in as remote '{path}'");

                Subst(drive, path);
            }
            else
            {
                Error($"Cannot find path for active alias");
                return;
            }
        }

        private static void RunUnmount()
        {
            var cfg = Config.Load();

            if (!cfg.TryGetValue(Config.Drive, out string drive))
            {
                Error($"Missing configuration value '{Config.Drive}'");
                return;
            }

            Info($"Unmounting drive '{drive}'");

            Subst(drive, "/D");
        }

        private static void RunSwitch(SwitchOptions options)
        {
            var cfg = Config.Load();

            if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) &&
                aliases.TryGetValue(options.Alias, out var path))
            {
                if (cfg.TryGetValue(Config.ActiveAlias, out string activeAlias) &&
                    activeAlias != options.Alias)
                {
                    Info($"Alias '{options.Alias}' is already active");
                }
                else
                {
                    cfg.SetValue(Config.ActiveAlias, options.Alias);

                    Info($"Alias '{options.Alias}' is now active");

                    cfg.Save();

                    RunMount();
                }
            }
            else
            {
                Error($"Cannot find alias '{options.Alias}'");
            }
        }

        private static void Subst(params string[] arguments)
        {
            var psi = new ProcessStartInfo("subst.exe");

            foreach (var argument in arguments)
            {
                psi.ArgumentList.Add(argument);
            }

            var process = Process.Start(psi);
            process.WaitForExit();
        }

        private static void RunUnalias(UnaliasOptions options)
        {
            var cfg = Config.Load();

            if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) &&
                aliases.Remove(options.Name))
            {
                //TODO: active alias
                Info($"Removed alias '{options.Name}'");

                cfg.SetValue(Config.Aliases, aliases);
                cfg.Save();
            }
            else
            {
                Error($"Alias '{options.Name}' not found");
            }
        }

        private static void RunAlias(AliasOptions options)
        {
            var cfg = Config.Load();

            if (options.Name == null)
            {
                if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) && aliases.Count > 0)
                {
                    Info("Aliases:");
                    PushIndent();

                    var pad = aliases.Max(a => a.Key.Length);
                    cfg.TryGetValue<string>(Config.ActiveAlias, out var activeAlias);

                    foreach (var (alias, path) in aliases)
                    {
                        var isActive = activeAlias == alias;

                        Info($"{(isActive ? '*' : ' ')}{alias.PadRight(pad)} {path}");
                    }

                    PopIndent();
                }
                else
                {
                    Info("No aliases defined");
                }
            }
            else if (options.Path == null)
            {
                if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases) &&
                    aliases.TryGetValue(options.Name, out var path))
                {
                    Info("Alias:");
                    PushIndent();
                    Info($"{options.Name}\t{path}");
                    PopIndent();
                }
                else
                {
                    Info($"Alias '{options.Name}' not found");
                }
            }
            else
            {
                if (cfg.TryGetValue(Config.Aliases, out Dictionary<string, string> aliases))
                {
                    if (aliases.ContainsKey(options.Name))
                    {
                        aliases[options.Name] = options.Path;

                        cfg.SetValue(Config.Aliases, aliases);

                        Info($"Updated alias '{options.Name}' with path '{options.Path}'");
                    }
                    else
                    {
                        aliases.Add(options.Name, options.Path);

                        cfg.SetValue(Config.Aliases, aliases);

                        Info($"Created alias '{options.Name}' with path '{options.Path}'");
                    }
                }
                else
                {
                    cfg.SetValue(Config.Aliases, new Dictionary<string, string> { { options.Name, options.Path } });

                    Info($"Created alias '{options.Name}' with path '{options.Path}'");
                }
            }

            cfg.Save();
        }

        private static void RunConfig(ConfigOptions options)
        {
            var cfg = Config.Load();

            if (options.Key == null)
            {
                Info("Config values:");
                PushIndent();

                var pad = Config.PublicKeys.Max(k => k.Length);

                foreach (var key in Config.PublicKeys.OrderBy(k => k))
                {
                    if (cfg.TryGetValueRaw(key, out var value))
                    {
                        Info($"{key.PadRight(pad)} {value}");
                    }
                    else
                    {
                        Info($"{key.PadRight(pad)} <missing>");
                    }
                }

                PopIndent();
            }
            else if (options.Value == null)
            {
                if (cfg.TryGetValueRaw(options.Key, out var value))
                {
                    Info("Config value:");
                    PushIndent();
                    Info($"{options.Key} {value}");
                    PopIndent();
                }
                else
                {
                    Info($"Config value '{options.Key}' not found");
                }
            }
            else
            {
                if (Config.PublicKeys.Contains(options.Key))
                {
                    cfg.SetValue(options.Key, options.Value);

                    Info($"Config value '{options.Key}' set to '{options.Value}'");
                }
                else
                {
                    Error($"Cannot set config value '{options.Key}' because it is not a public value");
                }

                cfg.Save();
            }
        }
    }
}
