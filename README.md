# LocalAdmin V2 [![GitHub release](https://badgen.net/github/release/northwood-studios/LocalAdmin-V2)](https://GitHub.com/northwood-studios/LocalAdmin-V2/releases/) [![Github all releases](https://img.shields.io/github/downloads/northwood-studios/LocalAdmin-V2/total.svg)](https://GitHub.com/northwood-studios/LocalAdmin-V2/releases/) [![GitHub latest commit](https://badgen.net/github/last-commit/northwood-studios/LocalAdmin-V2)](https://GitHub.com/northwood-studios/LocalAdmin-V2/commit/) [![GitHub watchers](https://img.shields.io/github/watchers/northwood-studios/LocalAdmin-V2.svg?style=social&label=Watch&maxAge=2592000)](https://GitHub.com/northwood-studios/LocalAdmin-V2/watchers/)
Second version of LocalAdmin, tool for hosting dedicated servers for SCP: Secret Laboratory.

This version is compatible with game version 9.0.0 and newer. Compatible both with Windows and Linux x64.

# Syntax
Windows: `LocalAdmin.exe [port] [arguments] [-- arguments passthrough]`

Linux: `./LocalAdmin [port] [arguments] [-- arguments passthrough]`

# Arguments list
| Long version | Short version | Description |
| --- | --- | --- |
| --reconfigure | -r | Opens configuration editor. |
| --config [path to file] | | Changes LocalAdmin config path. |
| --logs [path to logs folder] | | Changes LocalAdmin logs directory. |
| --gameLogs [path to logs folder] | | Changes game logs directory.<br>**It applies only to the Log Cleaner in LocalAdmin, not the game itself!** |
| --useDefault | -d | Uses the default config if no config is present. |
| --printStd | -s | Redirects stdout and stderr of the game to the LocalAdmin live view. |
| --noSetCursor | -c | Disables setting console cursor position. |
| --printControl | -p | Enables printing control messages. |
| --noLogs | -l | Disables LocalAdmin logging. |
| --noAutoFlush | -n | Disables auto flush of LocalAdmin log files.<br>**Not compatible with --noLogs argument.** |
| --noAlign | -a | Disables multiline log entries alignment. |
| --dismissPluginManagerSecurityWarning | | Dismisses Plugin Manager security warning. |
| --disableTrueColor | | Disables True Color output. |
| --restartsLimit |  | Specifies a limit of auto-restarts in a specified time window.<br>Setting this argument to 0 disables auto-restarts.<br>Setting this argument to -1 disables the limit.<br>*Default value: 4* |
| --restartsTimeWindow |  | Specifies a time window (in seconds) for the auto-restarts limit.<br>Setting this argument to 0 disables resetting the amount of auto-restarts after a specified amount of time.<br>*Default value: 480* |
| --logLengthLimit |  | Specifies the limit of characters in LocalAdmin log file.<br>Suffixes `k`, `M`, `G` and `T` are supported, eg. `5G` is equal to `5000000000` characters.<br>Setting this argument to 0 disables the limit.<br>*Default value: 25G* |
| --logEntriesLimit |  | Specifies the limit of entries in LocalAdmin log file.<br>Suffixes `k`, `M`, `G` and `T` are supported, eg. `5G` is equal to `5000000000` entries.<br>Setting this argument to 0 disables the limit.<br>*Default value: 10G* |
