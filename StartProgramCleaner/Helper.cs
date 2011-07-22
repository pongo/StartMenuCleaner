namespace StartProgramCleaner
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;

    using IWshRuntimeLibrary;

    using Microsoft.VisualBasic.FileIO;

    using File = System.IO.File;

    public struct ShortcutDetails
    {
        public string Description;
        public string Name;
        public string ShortcutPath;
        public string Target;
    }

    public class Helper
    {
        public static ArrayList ShortcutFiles = new ArrayList();
        public static ArrayList EmptyDirectories = new ArrayList();

        private static readonly WshShellClass Wsh = new WshShellClass();

        private static readonly string ProgramsFolder = GetProgramsFolder();

        private static readonly string[] ProgramsPath = {
                                                            Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                                                            Environment.GetEnvironmentVariable("USERPROFILE")
                                                        };
        
        /// <summary>
        /// Gets all wrong shortcuts and empty directories.
        /// </summary>
        public static void GetShortcuts()
        {
            ShortcutFiles.Clear();
            EmptyDirectories.Clear();

            try
            {
                foreach (var pr in ProgramsPath)
                {
                    DirRecurseSearch(Path.Combine(pr, ProgramsFolder));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Opens the shortcutFile in explorer.
        /// </summary>
        /// <param name="shortcutFile">The shortcut file.</param>
        public static void OpenInExplorer(string shortcutFile)
        {
            Wsh.Exec(string.Format("Explorer /select,{0}", shortcutFile));
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public static void DeleteFile(string filepath)
        {
            try
            {
                var dirParent = new DirectoryInfo(filepath).Parent;
                FileSystem.DeleteFile(filepath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                if (dirParent != null && IsDirectoryEmpty(dirParent.FullName))
                {
                    DeleteFolder(dirParent.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        /// <summary>
        /// Deletes the folder.
        /// </summary>
        /// <param name="folderpath">The folderpath.</param>
        public static void DeleteFolder(string folderpath)
        {
            try
            {
                var dirParent = new DirectoryInfo(folderpath).Parent;
                FileSystem.DeleteDirectory(folderpath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                if (dirParent != null && IsDirectoryEmpty(dirParent.FullName))
                {
                    DeleteFolder(dirParent.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Delete", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private static void DirRecurseSearch(string path)
        {
            CheckDirectory(path);

            foreach (var dir in Directory.GetDirectories(path))
            {
                DirRecurseSearch(dir);
            }
        }

        private static void CheckDirectory(string dir)
        {
            Debug.WriteLine(dir);

            if (IsDirectoryEmpty(dir))
            {
                EmptyDirectories.Add(
                    CreateNewShortcutDetails(Path.GetFileName(dir), "Empty Directory", dir, "Empty Directory"));

                return;
            }

            foreach (var file in Directory.GetFiles(dir))
            {
                Debug.WriteLine(file);

                if (IsNotALink(file))
                {
                    continue;
                }

                var targetFilePath = GetShortcutTarget(file);
                if (IsTargetExists(targetFilePath))
                {
                    continue;
                }

                ShortcutFiles.Add(
                    CreateNewShortcutDetails(
                        Path.GetFileNameWithoutExtension(file), targetFilePath, file, GetShortcutDescription(file)));
            }
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0;
        }

        private static bool IsNotALink(string path)
        {
            var extension = Path.GetExtension(path);
            return string.IsNullOrEmpty(extension) || extension.ToLower() != ".lnk";
        }

        private static bool IsTargetExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private static ShortcutDetails CreateNewShortcutDetails(
            string name, string target, string shortcutPath, string description)
        {
            return new ShortcutDetails
                {
                   Name = name, Target = target, ShortcutPath = shortcutPath, Description = description 
                };
        }

        private static string GetProgramsFolder()
        {
            var startmenu = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            return startmenu.Name;
        }

        private static string GetShortcutTarget(string shortcutFile)
        {
            var shortcut = (IWshShortcut)Wsh.CreateShortcut(shortcutFile);
            return shortcut.TargetPath == string.Empty ? shortcutFile : shortcut.TargetPath;
        }

        private static string GetShortcutDescription(string shortcutFile)
        {
            var shll = new WshShell();
            var shortcut = (IWshShortcut)shll.CreateShortcut(shortcutFile);
            return shortcut.Description;
        }
    }
}