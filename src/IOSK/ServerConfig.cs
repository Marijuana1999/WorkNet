using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace IOSK
{
    public static class ServerConfig
    {
        private static string ConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server_ip.txt");
        public static string ServerIP { get; private set; }

        public static void Initialize()
        {
            // اگر فایل وجود دارد بخوان
            if (File.Exists(ConfigFile))
            {
                ServerIP = File.ReadAllText(ConfigFile).Trim();
                if (!string.IsNullOrEmpty(ServerIP))
                    return;
            }

            // اگر نبود یا خالی بود، از کاربر بپرس
            string input = PromptForIP();
            if (!string.IsNullOrWhiteSpace(input))
            {
                ServerIP = input.Trim();
                File.WriteAllText(ConfigFile, ServerIP);
            }
            else
            {
                // اگر کاربر چیزی نزد، آی‌پی لوکال ست شود
                ServerIP = GetLocalIP();
                File.WriteAllText(ConfigFile, ServerIP);
            }
        }

        private static string PromptForIP()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the server IP address (e.g. 192.168.1.101):",
                "Set Server IP",
                   "127.0.0.1");
            return input;
        }

        private static string GetLocalIP()
        {
            try
            {
                string hostName = Dns.GetHostName();
                string localIP = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                return localIP;
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public static string GetNetworkPath(string subPath)
        {
            return $@"\\{ServerIP}\iosk\{subPath}";
        }
    }
}
