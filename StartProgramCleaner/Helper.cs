using System;
using System.Collections;
using System.IO;
using IWshRuntimeLibrary;

namespace StartProgramCleaner
{
    public struct ShortcutDetails
    {
        public string Name;
        public string Target;
        public string ShortcutPath;
        public string Description;
    }

    public class Helper
    {
        static readonly WshShellClass Wsh = new WshShellClass();

        static readonly string ProgramsFolder = GetProgramsFolder();
        private static readonly string[] ProgramsPath = {
                                                            Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                                                            Environment.GetEnvironmentVariable("USERPROFILE")
                                                        };

        public static ArrayList ShortcutFiles = new ArrayList();
        public static ArrayList EmptyDirectories = new ArrayList();

        private static string GetProgramsFolder()
        {
            var startmenu = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            return startmenu.Name;
        }

        /// <summary>
        /// Get shortcut target file path
        /// </summary>
        /// <param name="shortcutFile">the shortcut file path</param>
        /// <returns></returns>
        private static string GetTarget(string shortcutFile)
        {
            var shortcut = (IWshShortcut)Wsh.CreateShortcut(shortcutFile);
            return shortcut.TargetPath == string.Empty ? shortcutFile : shortcut.TargetPath;
        }

        private static string GetDescription(string shortcutFile)
        {
            var shll = new WshShell();
            var shortcut = (IWshShortcut)shll.CreateShortcut(shortcutFile);
            return shortcut.Description;
        }

        public static void GetShortcuts()
        {
            ShortcutFiles.Clear();
            EmptyDirectories.Clear();

            foreach (var pr in ProgramsPath)
            {
                DirRecurseSearch(Path.Combine(pr, ProgramsFolder));
            }
        }
        
        private static void DirRecurseSearch(string sDir)
        {
            try
            {
                CheckDirectory(sDir);

                foreach (var dir in Directory.GetDirectories(sDir))
                {
                    DirRecurseSearch(dir);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private static void CheckDirectory(string dir)
        {
            System.Diagnostics.Debug.WriteLine(dir);

            if (IsDirectoryEmpty(dir))
            {
                EmptyDirectories.Add(CreateNewShortcutDetails(
                    Path.GetFileName(dir), "Empty Directory", dir, "Empty Directory"));

                return;
            }
                    
            foreach (var file in Directory.GetFiles(dir))
            {
                System.Diagnostics.Debug.WriteLine(file);

                var e = Path.GetExtension(file);
                if (string.IsNullOrEmpty(e) || e.ToLower() != ".lnk") continue;

                var targetFilePath = GetTarget(file);
                if (IsTargetExists(targetFilePath)) continue;

                ShortcutFiles.Add(CreateNewShortcutDetails(
                    Path.GetFileNameWithoutExtension(file), targetFilePath, file, GetDescription(file)));
            }
        }

        private static bool IsTargetExists(string path)
        {
            return System.IO.File.Exists(path) || Directory.Exists(path);
        }

        private static ShortcutDetails CreateNewShortcutDetails(string name, string target, string shortcutPath, string description)
        {
            return new ShortcutDetails
                       {
                           Name = name,
                           Target = target,
                           ShortcutPath = shortcutPath,
                           Description = description
                       };
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;
        }

        public static void OpenInExplorer(string shortcutFile)
        {
            Wsh.Exec(string.Format("Explorer /select,{0}", shortcutFile));
        }

        public static void DeleteFile(string filepath)
        {
            try
            {
                System.IO.File.Delete(filepath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, @"Delete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public static void DeleteFolder(string folderpath)
        {
            try
            {
                Directory.Delete(folderpath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, @"Delete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
