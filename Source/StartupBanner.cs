using MelonLoader;

namespace ImprovedPackagers
{
    internal static class StartupBanner
    {
        private const string Reset = "\u001b[0m";
        private const string White = "\u001b[97m";
        private const string Green = "\u001b[1;38;5;46m";
        private const string Purple1 = "\u001b[1;38;5;141m";
        private const string Purple2 = "\u001b[1;38;5;135m";
        private const string Purple3 = "\u001b[38;5;99m";
        private const string Purple4 = "\u001b[38;5;93m";
        private const string Purple5 = "\u001b[38;5;57m";
        private const string Purple6 = "\u001b[38;5;54m";

        public static void Print()
        {
            MelonLogger.Msg(string.Empty);
            MelonLogger.Msg($"{Purple6}────────────{Reset} {Green}STAYING PORTED BY{Reset} {Purple6}────────────{Reset}");
            PrintRow(Purple1, "  ██████╗  ███████╗ ██╗   ██╗ ███████╗  ", "Improved Packagers PORTED ", $"v{ImprovedPackagers.Version}");
            PrintRow(Purple2, " ██╔════╝  ██╔════╝ ██║   ██║ ██╔════╝  ", "Schedule I ", "0.4.6f13 | IL2CPP");
            PrintRow(Purple3, " ██║  ███╗ ███████╗ ██║   ██║ ███████╗  ", "Build: ", ImprovedPackagers.BuildIdentity);
            PrintRow(Purple4, " ██║   ██║ ╚════██║ ╚██╗ ██╔╝ ╚════██║  ", "Original: ", "GuysWeForgotDre");
            PrintRow(Purple5, " ╚██████╔╝ ███████║  ╚████╔╝  ███████║  ", "Ported by ", "GSVS UK ACM");
            MelonLogger.Msg($"{Purple6}  ╚═════╝  ╚══════╝   ╚═══╝   ╚══════╝  {Reset}  {White}Schedule I f13 compatibility port{Reset}");
            MelonLogger.Msg($"{Purple6}────────────────────────────────────────────{Reset}{Green}◆{Reset}{Purple6}────────────────────────{Reset}");
        }

        private static void PrintRow(string artColor, string art, string label, string value)
        {
            MelonLogger.Msg($"{artColor}{art}{Reset}  {White}{label}{Reset}{Green}{value}{Reset}");
        }
    }
}
