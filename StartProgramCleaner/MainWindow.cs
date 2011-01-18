using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace StartProgramCleaner
{
    public partial class MainWindow : Form
    {
        public int SelIndex;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();
            InitializeControlEvents();
        }

        public void InitializeControls()
        {
        }

        public void InitializeControlEvents()
        {
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += bgWorker_ProgressChanged;

            Shown += MainWindow_Shown;

            lvFiles.ItemSelectionChanged += lvFiles_ItemSelectionChanged;
            lvFiles.DoubleClick += lvFiles_DoubleClick;

            btnDeleteAll.Click += DeleteButtons_Click;

            btnSelAll.Click += SelectionButtons_Click;
            btnSelInvert.Click += SelectionButtons_Click;
            btnSelNone.Click += SelectionButtons_Click;

            btnRescan.Click += btnRescan_Click;
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        private void SelectionButtons_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvFiles.Items)
            {
                if (sender == btnSelAll)
                {
                    item.Checked = true;
                }
                else if (sender == btnSelInvert)
                {
                    item.Checked = !item.Checked;
                }
                else if (sender == btnSelNone)
                {
                    item.Checked = false;
                }
            }
        }

        private void DeleteButtons_Click(object sender, EventArgs e)
        {
            if (sender == btnDeleteAll)
            {
                foreach (ListViewItem item in lvFiles.Items)
                {
                    if (item.Checked)
                    {
                        item.Text = @"Deleting > " + item.Text;
                        var sd = (ShortcutDetails) item.Tag;
                        if (sd.Description != "Empty Directory")
                        {
                            Helper.ShortcutFiles.Remove(item.Tag);
                            Helper.DeleteFile(sd.ShortcutPath);
                        }
                        else
                        {
                            Helper.EmptyDirectories.Remove(item.Tag);
                            Helper.DeleteFolder(sd.ShortcutPath);
                        }
                        item.Text = @"Deleted";
                    }
                }
            }

            bgWorker_RunWorkerCompleted(bgWorker, new RunWorkerCompletedEventArgs(null, null, false));
        }

        private void lvFiles_DoubleClick(object sender, EventArgs e)
        {
            var sd = (ShortcutDetails) lvFiles.Items[SelIndex].Tag;
            Helper.OpenInExplorer(sd.ShortcutPath);
        }

        private void lvFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var sd = (ShortcutDetails) e.Item.Tag;

            lblStatus.Text = sd.Description;

            SelIndex = e.ItemIndex;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                lblStatus.Text = @"Scanning ...";
                btnDeleteAll.Enabled = false;
                lvFiles.Enabled = false;
            }
            else if (e.ProgressPercentage == 2)
            {
                lblStatus.Text = string.Format(lblStatus.Tag.ToString(),
                                               Helper.ShortcutFiles.Count,
                                               Helper.EmptyDirectories.Count);
                btnDeleteAll.Enabled = true;
                lvFiles.Enabled = true;
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lvFiles.Items.Clear();

            foreach (ShortcutDetails sd in Helper.ShortcutFiles)
            {
                var item = new ListViewItem(sd.Name);
                item.SubItems.Add(sd.Target);
                item.SubItems.Add(sd.ShortcutPath);
                item.ImageIndex = 0;

                item.Tag = sd;

                lvFiles.Items.Add(item);
            }

            foreach (ShortcutDetails sd in Helper.EmptyDirectories)
            {
                var item = new ListViewItem(sd.Name);
                item.SubItems.Add(sd.Target);
                item.SubItems.Add(sd.ShortcutPath);
                item.ImageIndex = 1;

                item.Tag = sd;

                lvFiles.Items.Add(item);
            }

            foreach (ColumnHeader ch in lvFiles.Columns)
            {
                ch.Width = -1;
            }

            bgWorker_ProgressChanged(bgWorker, new ProgressChangedEventArgs(2, null));
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bgWorker.ReportProgress(1);

            Helper.GetShortcuts();
        }
    }
}