namespace StartProgramCleaner
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public partial class MainWindow : Form
    {
        public int SelIndex;

        public MainWindow()
        {
            this.InitializeComponent();
            this.InitializeControls();
            this.InitializeControlEvents();
        }

        public void InitializeControls()
        {
        }

        public void InitializeControlEvents()
        {
            this.bgWorker.DoWork += this.bgWorker_DoWork;
            this.bgWorker.RunWorkerCompleted += this.bgWorker_RunWorkerCompleted;
            this.bgWorker.ProgressChanged += this.bgWorker_ProgressChanged;

            this.Shown += this.MainWindow_Shown;

            this.lvFiles.ItemSelectionChanged += this.lvFiles_ItemSelectionChanged;
            this.lvFiles.DoubleClick += this.lvFiles_DoubleClick;

            this.btnDeleteAll.Click += this.DeleteButtons_Click;

            this.btnSelAll.Click += this.SelectionButtons_Click;
            this.btnSelInvert.Click += this.SelectionButtons_Click;
            this.btnSelNone.Click += this.SelectionButtons_Click;

            this.btnRescan.Click += this.btnRescan_Click;
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            this.bgWorker.RunWorkerAsync();
        }

        private void SelectionButtons_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.lvFiles.Items)
            {
                if (sender == this.btnSelAll)
                {
                    item.Checked = true;
                }
                else if (sender == this.btnSelInvert)
                {
                    item.Checked = !item.Checked;
                }
                else if (sender == this.btnSelNone)
                {
                    item.Checked = false;
                }
            }
        }

        private void DeleteButtons_Click(object sender, EventArgs e)
        {
            if (sender == this.btnDeleteAll)
            {
                /* first remove the labels, and then folders */

                foreach (ListViewItem item in this.lvFiles.Items)
                {
                    if (!item.Checked)
                    {
                        continue;
                    }

                    if (item.Text == @"Deleted")
                    {
                        continue;
                    }

                    var sd = (ShortcutDetails)item.Tag;
                    if (sd.Description == "Empty Directory")
                    {
                        continue;
                    }

                    item.Text = @"Deleting > " + item.Text;
                    Helper.ShortcutFiles.Remove(item.Tag);
                    Helper.DeleteFile(sd.ShortcutPath);
                    item.Text = @"Deleted";
                }

                foreach (ListViewItem item in this.lvFiles.Items)
                {
                    if (!item.Checked)
                    {
                        continue;
                    }

                    if (item.Text == @"Deleted")
                    {
                        continue;
                    }

                    var sd = (ShortcutDetails)item.Tag;
                    if (sd.Description != "Empty Directory")
                    {
                        continue;
                    }

                    item.Text = @"Deleting > " + item.Text;
                    Helper.EmptyDirectories.Remove(item.Tag);
                    Helper.DeleteFolder(sd.ShortcutPath);
                    item.Text = @"Deleted";
                }
            }

            this.bgWorker_RunWorkerCompleted(this.bgWorker, new RunWorkerCompletedEventArgs(null, null, false));
        }

        private void lvFiles_DoubleClick(object sender, EventArgs e)
        {
            var sd = (ShortcutDetails)this.lvFiles.Items[this.SelIndex].Tag;
            Helper.OpenInExplorer(sd.ShortcutPath);
        }

        private void lvFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var sd = (ShortcutDetails)e.Item.Tag;

            this.lblStatus.Text = sd.Description;

            this.SelIndex = e.ItemIndex;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            this.bgWorker.RunWorkerAsync();
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                this.lblStatus.Text = @"Scanning ...";
                this.btnDeleteAll.Enabled = false;
                this.lvFiles.Enabled = false;
            }
            else if (e.ProgressPercentage == 2)
            {
                this.lblStatus.Text = string.Format(
                    this.lblStatus.Tag.ToString(), Helper.ShortcutFiles.Count, Helper.EmptyDirectories.Count);
                this.btnDeleteAll.Enabled = true;
                this.lvFiles.Enabled = true;
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.lvFiles.Items.Clear();

            foreach (ShortcutDetails sd in Helper.ShortcutFiles)
            {
                var item = new ListViewItem(sd.Name);
                item.SubItems.Add(sd.Target);
                item.SubItems.Add(sd.ShortcutPath);
                item.ImageIndex = 0;

                item.Tag = sd;

                this.lvFiles.Items.Add(item);
            }

            foreach (ShortcutDetails sd in Helper.EmptyDirectories)
            {
                var item = new ListViewItem(sd.Name);
                item.SubItems.Add(sd.Target);
                item.SubItems.Add(sd.ShortcutPath);
                item.ImageIndex = 1;

                item.Tag = sd;

                this.lvFiles.Items.Add(item);
            }

            foreach (ColumnHeader ch in this.lvFiles.Columns)
            {
                ch.Width = -1;
            }

            this.bgWorker_ProgressChanged(this.bgWorker, new ProgressChangedEventArgs(2, null));
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.bgWorker.ReportProgress(1);

            Helper.GetShortcuts();
        }
    }
}