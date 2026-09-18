using MelonLoader;
using System;

namespace ImprovedPackagers
{
    internal static class StartupBanner
    {
        public static void Print()
        {
            MelonLogger.Msg(string.Empty);
            MelonLogger.Msg(ConsoleColor.Green, "                    STAYING PORTED BY");
            MelonLogger.Msg(ConsoleColor.Green, "  ██████   ███████  ██    ██  ███████     Improved Packagers v2.0.1");
            MelonLogger.Msg(ConsoleColor.Green, " ██        ██       ██    ██  ██          Schedule I 0.4.6f13 | IL2CPP");
            MelonLogger.Msg(ConsoleColor.Green, " ██  ███   ███████  ██    ██  ███████     Build: f13-nexus-rc1");
            MelonLogger.Msg(ConsoleColor.Green, " ██   ██        ██   ██  ██        ██     Original: GuysWeForgotDre");
            MelonLogger.Msg(ConsoleColor.Green, "  ██████   ███████    ████    ███████     Port maintained by GSVS");
            MelonLogger.Msg(ConsoleColor.DarkGreen, " ─────────────────────────────────────────────────────────────────────");
        }
    }
}
