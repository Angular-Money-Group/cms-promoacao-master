using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;

namespace Bitzar.CMS.Core.Helper
{
    public static class UpdateHelper
    {
        public static List<string> CheckNewVersion(UpdateViewModel viewModel = null)
        {
            var workFolder = GetWorkFolder();

            var vm = viewModel ?? new UpdateViewModel()
            {
                UpdateFolder = CheckFolder(workFolder, "Update"),
                CurrentVersion = Assembly.GetAssembly(typeof(UpdateHelper)).GetName().Version
            };

            var responseFiles = new List<string>();
            DirectoryInfo updateDirectory = new DirectoryInfo(vm.UpdateFolder);
            var files = updateDirectory.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                var file = Path.GetFileNameWithoutExtension(files[i].Name);
                Version.TryParse(file, out Version fileVersion);

                if (fileVersion > vm.CurrentVersion)
                    responseFiles.Add(files[i].FullName);
            }

            return responseFiles;
        }

        public static void Update()
        {
            var vm = CreateUpdateViewModel();

            try
            {
                var responseFiles = CheckNewVersion(vm);

                if (responseFiles.Count > 0)
                {
                    CreateBackup(vm.WorkFolder, vm.BackupFolder);
                    RestoreNewVersion
                    (
                        responseFiles: responseFiles,
                        originFolder: vm.TempFolder,
                        destFolder: vm.WorkFolder,
                        connStr: vm.ConnectionString
                    );
                    RemoveTempFilesAndFolders(vm);
                }
            }
            catch (Exception ex)
            {
                RestoreBackup(vm.BackupFolder, vm.WorkFolder);
                throw ex;
            }
        }

        private static string GetWorkFolder()
        {
            var location = Assembly.GetExecutingAssembly().CodeBase;
            var path = new Uri(Path.GetDirectoryName(location)).AbsolutePath;
            DirectoryInfo parentDir = new DirectoryInfo(path).Parent;

            return $@"{parentDir.FullName}\";
        }

        private static void CreateBackup(string workFolder, string backupFolder)
        {
            try
            {
                RemoveDirectory(backupFolder);
                CopyFromTo(workFolder, backupFolder, new List<string>() { "Backup", "Update", "Copy", "Temp", "App_Browsers" });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void RestoreBackup(string backupFolder, string workFolder)
        {
            try
            {
                CopyFromTo(backupFolder, workFolder, new List<string>());
                RemoveDirectory(backupFolder);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void RestoreNewVersion(List<string> responseFiles, string originFolder, string destFolder,
                                              ConnectionStringSettings connStr)
        {
            try
            {
                for (int i = 0; i < responseFiles.Count; i++)
                {
                    ZipFile.ExtractToDirectory(responseFiles[i], originFolder);
                }

                CopyFromTo(originFolder, destFolder, new List<string>() { "Views", "content", "branding.css" });

                var configuration = WebConfigurationManager.OpenWebConfiguration("~");

                if (!string.IsNullOrWhiteSpace(ConfigurationManager.ConnectionStrings["DatabaseConnection"].Name))
                    configuration.ConnectionStrings.ConnectionStrings.Remove("DatabaseConnection");

                configuration.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("DatabaseConnection", connStr.ConnectionString, connStr.ProviderName));
                configuration.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void DeleteFiles(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (var file in dir.GetFiles())
                    file.Delete();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void RemoveDirectory(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                    dir.Delete(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string CheckFolder(string parentDir, string newDir)
        {
            var newFolder = $@"{parentDir}\{newDir}\";

            try
            {
                if (!Directory.Exists(newFolder))
                    Directory.CreateDirectory(newFolder);

                return newFolder;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void RemoveTempFilesAndFolders(UpdateViewModel vm)
        {
            DeleteFiles(vm.UpdateFolder);
            RemoveDirectory(vm.CopyFolder);
            RemoveDirectory(vm.TempFolder);
            //RemoveDirectory(vm.BackupFolder);
        }

        private static void CopyFromTo(string from, string to, List<string> excluded, string searchPattern = "*",
                                       SearchOption searchOption = SearchOption.AllDirectories, bool replace = true)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(from, searchPattern, searchOption))
                {
                    if (!excluded.Any(s => dirPath.Contains(s)))
                        Directory.CreateDirectory(dirPath.Replace(from, to));
                }

                foreach (string newPath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories))
                {
                    if (!excluded.Any(s => newPath.Contains(s)))
                        File.Copy(newPath, newPath.Replace(from, to), replace);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static UpdateViewModel CreateUpdateViewModel()
        {
            var work = GetWorkFolder();

            return new UpdateViewModel()
            {
                WorkFolder = work,
                CopyFolder = CheckFolder(work, "Copy"),
                TempFolder = CheckFolder(work, "Temp"),
                BackupFolder = CheckFolder(work, "Backup"),
                UpdateFolder = CheckFolder(work, "Update"),
                CurrentVersion = Assembly.GetAssembly(typeof(UpdateHelper)).GetName().Version,
                ConnectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"]
            };
        }
    }

    public class UpdateViewModel
    {
        public string CopyFolder { get; set; }
        public string WorkFolder { get; set; }
        public string TempFolder { get; set; }
        public string BackupFolder { get; set; }
        public string UpdateFolder { get; set; }
        public Version CurrentVersion { get; set; }
        public ConnectionStringSettings ConnectionString { get; set; }
    }
}

