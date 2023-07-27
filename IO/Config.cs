using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LocalAdmin.V2.IO;

public class Config
{
    private static readonly string[] SplitArray = { ": " };

    public bool RestartOnCrash = true;
    public bool EnableHeartbeat = true;
    public bool LaLiveViewUseUtc;

    public bool LaShowStdoutStderr;
    public bool LaNoSetCursor = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public bool EnableTrueColor = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public bool EnableLaLogs = true;
    public bool LaLogsUseUtc;
    public bool LaLogsUseZForUtc;
    public bool LaLogAutoFlush = true;
    public bool LaLogStdoutStderr = true;
    public bool LaDeleteOldLogs = true;
    public ushort LaLogsExpirationDays = 90;
    public bool DeleteOldRoundLogs;
    public ushort RoundLogsExpirationDays = 180;
    public bool CompressOldRoundLogs;
    public ushort RoundLogsCompressionThresholdDays = 14;
    public uint HeartbeatSpanMaxThreshold = 30;
    public uint HeartbeatRestartInSeconds = 11;

    public int LaToSlBufferSize = 25000;
    public int SlToLaBufferSize = 200000;

    public string LaLiveViewTimeFormat = "yyyy-MM-dd HH:mm:ss.fff zzz";

    public string SerializeConfig()
    {
        var sb = new StringBuilder();

        sb.Append("restart_on_crash: ");
        sb.AppendLine(RestartOnCrash.ToString().ToLowerInvariant());

        sb.Append("enable_heartbeat: ");
        sb.AppendLine(EnableHeartbeat.ToString().ToLowerInvariant());

        sb.Append("la_live_view_use_utc: ");
        sb.AppendLine(LaLiveViewUseUtc.ToString().ToLowerInvariant());

        sb.Append("la_live_view_time_format: ");
        sb.AppendLine(LaLiveViewTimeFormat);

        sb.Append("la_show_stdout_and_stderr: ");
        sb.AppendLine(LaShowStdoutStderr.ToString().ToLowerInvariant());

        sb.Append("la_no_set_cursor: ");
        sb.AppendLine(LaNoSetCursor.ToString().ToLowerInvariant());

        sb.Append("enable_true_color: ");
        sb.AppendLine(EnableTrueColor.ToString().ToLowerInvariant());

        sb.Append("enable_la_logs: ");
        sb.AppendLine(EnableLaLogs.ToString().ToLowerInvariant());

        sb.Append("la_logs_use_utc: ");
        sb.AppendLine(LaLogsUseUtc.ToString().ToLowerInvariant());

        sb.Append("la_logs_use_Z_for_utc: ");
        sb.AppendLine(LaLogsUseZForUtc.ToString().ToLowerInvariant());

        sb.Append("la_log_auto_flush: ");
        sb.AppendLine(LaLogAutoFlush.ToString().ToLowerInvariant());

        sb.Append("la_log_stdout_and_stderr: ");
        sb.AppendLine(LaLogStdoutStderr.ToString().ToLowerInvariant());

        sb.Append("la_delete_old_logs: ");
        sb.AppendLine(LaDeleteOldLogs.ToString().ToLowerInvariant());

        sb.Append("la_logs_expiration_days: ");
        sb.AppendLine(LaLogsExpirationDays.ToString());

        sb.Append("delete_old_round_logs: ");
        sb.AppendLine(DeleteOldRoundLogs.ToString().ToLowerInvariant());

        sb.Append("round_logs_expiration_days: ");
        sb.AppendLine(RoundLogsExpirationDays.ToString());

        sb.Append("compress_old_round_logs: ");
        sb.AppendLine(CompressOldRoundLogs.ToString().ToLowerInvariant());

        sb.Append("round_logs_compression_threshold_days: ");
        sb.AppendLine(RoundLogsCompressionThresholdDays.ToString());

        sb.Append("heartbeat_span_max_threshold: ");
        sb.AppendLine(HeartbeatSpanMaxThreshold.ToString());

        sb.Append("heartbeat_restart_in_seconds: ");
        sb.AppendLine(HeartbeatRestartInSeconds.ToString());

        sb.Append("la_to_sl_buffer_size: ");
        sb.AppendLine(LaToSlBufferSize.ToString());

        sb.Append("sl_to_la_buffer_size: ");
        sb.AppendLine(SlToLaBufferSize.ToString());

        return sb.ToString();
    }

    public static Config DeserializeConfig(string[] lines)
    {
        var cfg = new Config();

        foreach (var line in lines)
        {
            if (!line.Contains(": ", StringComparison.Ordinal))
                continue;

            var sp = line.Split(SplitArray, StringSplitOptions.None);
            if (sp.Length != 2)
                continue;

            switch (sp[0].ToLowerInvariant())
            {
                case "restart_on_crash" when bool.TryParse(sp[1], out var b):
                    cfg.RestartOnCrash = b;
                    break;

                case "enable_heartbeat" when bool.TryParse(sp[1], out var b):
                    cfg.EnableHeartbeat = b;
                    break;

                case "la_live_view_use_utc" when bool.TryParse(sp[1], out var b):
                    cfg.LaLiveViewUseUtc = b;
                    break;

                case "la_live_view_time_format":
                    cfg.LaLiveViewTimeFormat = sp[1];
                    break;

                case "la_show_stdout_and_stderr" when bool.TryParse(sp[1], out var b):
                    cfg.LaShowStdoutStderr = b;
                    break;

                case "la_no_set_cursor" when bool.TryParse(sp[1], out var b):
                    cfg.LaNoSetCursor = b;
                    break;

                case "enable_true_color" when bool.TryParse(sp[1], out var b):
                    cfg.EnableTrueColor = b;
                    break;

                case "enable_la_logs" when bool.TryParse(sp[1], out var b):
                    cfg.EnableLaLogs = b;
                    break;

                case "la_logs_use_utc" when bool.TryParse(sp[1], out var b):
                    cfg.LaLogsUseUtc = b;
                    break;

                case "la_logs_use_Z_for_utc" when bool.TryParse(sp[1], out var b):
                    cfg.LaLogsUseZForUtc = b;
                    break;

                case "la_log_auto_flush" when bool.TryParse(sp[1], out var b):
                    cfg.LaLogAutoFlush = b;
                    break;

                case "la_log_stdout_and_stderr" when bool.TryParse(sp[1], out var b):
                    cfg.LaLogStdoutStderr = b;
                    break;

                case "la_delete_old_logs" when bool.TryParse(sp[1], out var b):
                    cfg.LaDeleteOldLogs = b;
                    break;

                case "la_logs_expiration_days" when ushort.TryParse(sp[1], out var b):
                    cfg.LaLogsExpirationDays = b;
                    break;

                case "delete_old_round_logs" when bool.TryParse(sp[1], out var b):
                    cfg.DeleteOldRoundLogs = b;
                    break;

                case "round_logs_expiration_days" when ushort.TryParse(sp[1], out var b):
                    cfg.RoundLogsExpirationDays = b;
                    break;

                case "compress_old_round_logs" when bool.TryParse(sp[1], out var b):
                    cfg.CompressOldRoundLogs = b;
                    break;

                case "round_logs_compression_threshold_days" when ushort.TryParse(sp[1], out var b):
                    cfg.RoundLogsCompressionThresholdDays = b;
                    break;

                case "heartbeat_restart_in_seconds" when ushort.TryParse(sp[1], out var b):
                    cfg.HeartbeatRestartInSeconds = b;
                    break;

                case "heartbeat_span_max_threshold" when ushort.TryParse(sp[1], out var b):
                    cfg.HeartbeatSpanMaxThreshold = b;
                    break;

                case "la_to_sl_buffer_size" when int.TryParse(sp[1], out var b):
                    if (b <= 100)
                        b = 101;
                    cfg.LaToSlBufferSize = b;
                    break;

                case "sl_to_la_buffer_size" when int.TryParse(sp[1], out var b):
                    if (b <= 350)
                        b = 351;
                    cfg.SlToLaBufferSize = b;
                    break;
            }

            if (!cfg.EnableLaLogs || cfg.LaLogsExpirationDays == 0)
                cfg.LaDeleteOldLogs = false;

            if (cfg.RoundLogsExpirationDays == 0)
                cfg.DeleteOldRoundLogs = false;

            if (cfg.RoundLogsCompressionThresholdDays == 0)
                cfg.CompressOldRoundLogs = false;

            if (cfg.DeleteOldRoundLogs && cfg.RoundLogsExpirationDays <= cfg.RoundLogsCompressionThresholdDays)
                cfg.CompressOldRoundLogs = false;
        }

        return cfg;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine(RestartOnCrash ? "- Server will automatically restart after a crash." : "- Server will NOT automatically restart after a crash.");
        sb.AppendLine(EnableHeartbeat ? "- LocalAdmin will attempt to detect silent server crashes (heartbeat enabled)." : "- LocalAdmin will NOT attempt to detect silent server crashes (heartbeat DISABLED).");
        sb.AppendLine(LaLiveViewUseUtc ? "- LocalAdmin live view will use UTC timezone." : "- LocalAdmin live view will use local timezone.");
        sb.AppendLine($"- LocalAdmin live will use the following timestamp format: {LaLiveViewTimeFormat}");
        sb.AppendLine($"- UTC timezone will be displayed as \"{(LaLogsUseZForUtc ? "Z" : "+00:00")}\".");
        sb.AppendLine(LaShowStdoutStderr ? "- Standard outputs (that contain a lot of debug information) will be displayed." : "- Standard outputs (that contain a lot of debug information) will NOT be displayed.");
        sb.AppendLine(LaNoSetCursor ? "- Cursor position management is DISABLED." : "- Cursor position management is ENABLED.");
        sb.AppendLine(EnableTrueColor ? "- True Color output is ENABLED." : "- True Color output is DISABLED.");
        sb.AppendLine(EnableLaLogs ? "- LocalAdmin logs are ENABLED." : "- LocalAdmin logs are DISABLED.");

        if (EnableLaLogs)
        {
            sb.AppendLine(LaLogsUseUtc ? "- LocalAdmin logs will use UTC timezone." : "- LocalAdmin logs will use local timezone.");
            sb.AppendLine(LaLogAutoFlush ? "- LocalAdmin logs auto flushing is ENABLED." : "- LocalAdmin logs auto flushing is DISABLED.");
            sb.AppendLine(LaLogStdoutStderr ? "- Standard outputs will be logged." : "- Standard outputs will NOT be logged.");

            if (LaDeleteOldLogs)
            {
                sb.Append("- Delete LocalAdmin logs older than ");
                sb.Append(LaLogsExpirationDays);
                sb.AppendLine(" days");
            }
            else sb.AppendLine("- Do not delete old LocalAdmin logs.");
        }

        if (DeleteOldRoundLogs)
        {
            sb.Append("- Delete round logs older than ");
            sb.Append(RoundLogsExpirationDays);
            sb.AppendLine(" days");
        }
        else sb.AppendLine("- Do not delete old round logs.");

        if (CompressOldRoundLogs)
        {
            sb.Append("- Compress round logs older than ");
            sb.Append(RoundLogsCompressionThresholdDays);
            sb.AppendLine(" days");
        }
        else sb.AppendLine("- Do not compress old round logs.");

        return sb.ToString();
    }
}