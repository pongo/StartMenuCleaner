using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        static WshShellClass wsh = new WshShellClass();

        static string _programs_folder = GetProgramsFolder();
        static string[] _programs_path = {
                                            Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                                            Environment.GetEnvironmentVariable("USERPROFILE")
                                         };

        public static ArrayList ShortcutFiles = new ArrayList();
        public static ArrayList EmptyDirectories = new ArrayList();

        public Helper()
        {
        }

        private static string GetProgramsFolder()
        {
            var programs = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
            var startmenu = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            var s = string.Format("{0}\\{1}", startmenu.Name, programs.Name);
            return s;
        }

        /// <summary>
        /// Get shortcut target file path
        /// </summary>
        /// <param name="shortcut_file">the shortcut file path</param>
        /// <returns></returns>
        private static string GetTarget(string shortcut_file)
        {
            IWshShortcut shortcut = (IWshShortcut)wsh.CreateShortcut(shortcut_file);

            string t = string.Empty;

            if (shortcut.TargetPath != string.Empty)
            {
                t = shortcut.TargetPath;
            }
            else
            {
                // for URL shortcuts but with has an .LNK extension
                //IWshURLShortcut url_shortcut = (IWshURLShortcut)wsh.CreateShortcut(shortcut_file);

                // anyway .. we'll just assume this is a URL shortcut
                // so let's just return set the TargetPath the same as the shortcut file path.
                t = shortcut_file;
            }
            
            shortcut = null;

            return t;
        }

        private static string GetDescription(string shortcut_file)
        {
            WshShell shll = new WshShell();

            IWshShortcut shortcut = (IWshShortcut)shll.CreateShortcut(shortcut_file);
            
            string d = shortcut.Description;

            shortcut = null;

            return d;
        }

        public static void GetShortcuts()
        {
            ShortcutFiles.Clear();
            EmptyDirectories.Clear();

            foreach (string pr in _programs_path)
            {
                DirRecurseSearch(Path.Combine(pr, _programs_folder), new string[] { "lnk" });
            }
        }
        
        private static void DirRecurseSearch(string sDir, string[] filter)
        {
            int i = 0;

            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    i++;
                    System.Diagnostics.Debug.WriteLine(i.ToString("00#", null) + " = " + d);

                    // check if the current folder `d` is empty
                    if (Directory.GetFiles(d).Length == 0 && Directory.GetDirectories(d).Length == 0)
                    {
                        ShortcutDetails sd = new ShortcutDetails();
                        sd.Name = Path.GetFileName(d);
                        sd.Target = "Empty Directory";
                        sd.ShortcutPath = d;
                        sd.Description = "Empty Directory";

                        EmptyDirectories.Add(sd);

                        continue;
                    }
                    
                    foreach (string f in Directory.GetFiles(d))
                    {
                        string ext = Path.GetExtension(f).ToLower();

                        if (ext == ".lnk")
                        {
                            string target_filepath = GetTarget(f);

                            if (!System.IO.File.Exists(target_filepath) && !Directory.Exists(target_filepath))
                            {
                                ShortcutDetails sd = new ShortcutDetails();
                                sd.Name = Path.GetFileNameWithoutExtension(f);
                                sd.Target = target_filepath;
                                sd.ShortcutPath = f;
                                sd.Description = GetDescription(f);

                                ShortcutFiles.Add(sd);
                            }
                        }
                    }

                    DirRecurseSearch(d, filter);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        public static void OpenInExplorer(string shortcut_file)
        {
            wsh.Exec(string.Format("Explorer /select,{0}", shortcut_file));
        }

        public static void DeleteFile(string filepath)
        {
            try
            {
                System.IO.File.Delete(filepath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Delete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public static void DeleteFolder(string folderpath)
        {
            try
            {
                System.IO.Directory.Delete(folderpath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Delete", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
        }
    }
}
