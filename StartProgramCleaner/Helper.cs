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
            var programs = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
            var startmenu = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            return string.Format("{0}\\{1}", startmenu.Name, programs.Name);
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
            var i = 0;

            try
            {
                foreach (var d in Directory.GetDirectories(sDir))
                {
                    i++;
                    System.Diagnostics.Debug.WriteLine(i.ToString("00#", null) + " = " + d);

                    // check if the current folder `d` is empty
                    if (Directory.GetFiles(d).Length == 0 && Directory.GetDirectories(d).Length == 0)
                    {
                        var sd = new ShortcutDetails
                                     {
                                         Name = Path.GetFileName(d),
                                         Target = "Empty Directory",
                                         ShortcutPath = d,
                                         Description = "Empty Directory"
                                     };
                        EmptyDirectories.Add(sd);
                        continue;
                    }
                    
                    foreach (var f in Directory.GetFiles(d))
                    {
                        var e = Path.GetExtension(f);
                        if (string.IsNullOrEmpty(e) || e.ToLower() != ".lnk") continue;

                        var targetFilepath = GetTarget(f);
                        if (System.IO.File.Exists(targetFilepath) || Directory.Exists(targetFilepath)) continue;

                        var sd = new ShortcutDetails
                                     {
                                         Name = Path.GetFileNameWithoutExtension(f),
                                         Target = targetFilepath,
                                         ShortcutPath = f,
                                         Description = GetDescription(f)
                                     };
                        ShortcutFiles.Add(sd);
                    }

                    DirRecurseSearch(d);
                }
            }
            catch (Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
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
