using System;
using System.IO;
using ClosedXML.Excel;
using System.Windows.Forms;

namespace IOSK
{
    public static class GlobalFileManager
    {
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string BaseFolder = Path.Combine(DesktopPath, "Companies");

        public static string ProjectsFile => Path.Combine(BaseFolder, "Projects.xlsx");
        public static string DailyReportsFile => Path.Combine(BaseFolder, "DailyReports.xlsx");
        public static string DataBankFile => Path.Combine(BaseFolder, "DataBank.xlsx");

        public static void Initialize()
        {
            try
            {
                if (!Directory.Exists(BaseFolder))
                    Directory.CreateDirectory(BaseFolder);

                CreateProjectsFileIfMissing();
                CreateDailyReportsFileIfMissing();
                CreateDataBankFileIfMissing();
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error creating global files:\n" + ex.Message);
            }
        }

        private static void CreateProjectsFileIfMissing()
        {
            if (File.Exists(ProjectsFile)) return;

            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Projects");
                ws.Cell(1, 1).Value = "Company Name";
                ws.Cell(1, 2).Value = "Project Name";
                ws.Cell(1, 3).Value = "Delivery Date";
                ws.Cell(1, 4).Value = "Request Number";
                ws.Cell(1, 5).Value = "Final Result";
                ws.Cell(1, 6).Value = "Description";
                ws.Row(1).Style.Font.Bold = true;
                wb.SaveAs(ProjectsFile);
            }
        }

        private static void CreateDailyReportsFileIfMissing()
        {
            if (File.Exists(DailyReportsFile)) return;

            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Daily Reports");
                ws.Cell(1, 1).Value = "Username";
                ws.Cell(1, 2).Value = "Last Report";
                ws.Cell(1, 3).Value = "Last Report Date";
                ws.Row(1).Style.Font.Bold = true;
                wb.SaveAs(DailyReportsFile);
            }
        }

        private static void CreateDataBankFileIfMissing()
        {
            if (File.Exists(DataBankFile)) return;

            using (var wb = new XLWorkbook())
            {
                var ws = wb.AddWorksheet("Reviewed");
                ws.Cell(1, 1).Value = "Holding Name";
                ws.Cell(1, 2).Value = "Company Name";
                ws.Cell(1, 3).Value = "Target Sheet";
                ws.Row(1).Style.Font.Bold = true;
                wb.SaveAs(DataBankFile);
            }
        }


        private static readonly string BasePath = AppDomain.CurrentDomain.BaseDirectory;

        // مسیر کاربران آنلاین در شبکه
        public static string OnlineUsersFolder => ServerConfig.GetNetworkPath("OnlineUsers");

        // مسیر کاربران محلی (برای حالت آفلاین)
        public static string LocalUsersFolder
        {
            get
            {
                string localPath = Path.Combine(BasePath, "LocalUsers");
                if (!Directory.Exists(localPath))
                    Directory.CreateDirectory(localPath);
                return localPath;
            }
        }

        // مسیر پشتیبان‌ها (اختیاری ولی مفید)
        public static string BackupFolder
        {
            get
            {
                string backupPath = Path.Combine(BasePath, "Backups");
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);
                return backupPath;
            }
        }

        // تابع کمکی برای اطمینان از وجود فایل یا ساخت خودکارش
        public static void EnsureFileExists(string filePath, string defaultContent = "")
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(filePath))
                File.WriteAllText(filePath, defaultContent);
        }
    }
}
