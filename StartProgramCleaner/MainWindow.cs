using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace StartProgramCleaner
{
    public partial class MainWindow : Form
    {
        public int _selIndex = 0;

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
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);

            this.Shown += new EventHandler(MainWindow_Shown);

            this.lvFiles.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(lvFiles_ItemSelectionChanged);
            this.lvFiles.DoubleClick += new EventHandler(lvFiles_DoubleClick);

            btnDeleteAll.Click += new EventHandler(DeleteButtons_Click);

            btnSelAll.Click += new EventHandler(SelectionButtons_Click);
            btnSelInvert.Click += new EventHandler(SelectionButtons_Click);
            btnSelNone.Click += new EventHandler(SelectionButtons_Click);

            btnRescan.Click += new EventHandler(btnRescan_Click);
        }

        void btnRescan_Click(object sender, EventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        void SelectionButtons_Click(object sender, EventArgs e)
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

        void DeleteButtons_Click(object sender, EventArgs e)
        {
            if (sender == btnDeleteAll)
            {
                foreach (ListViewItem item in lvFiles.Items)
                {
                    if (item.Checked)
                    {
                        item.Text = "Deleting > " + item.Text;
                        ShortcutDetails sd = (ShortcutDetails)item.Tag;
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
                        item.Text = "Deleted";
                    }
                }
            }

            bgWorker_RunWorkerCompleted(bgWorker, new RunWorkerCompletedEventArgs(null, null, false));
        }

        void lvFiles_DoubleClick(object sender, EventArgs e)
        {
            ShortcutDetails sd = (ShortcutDetails)lvFiles.Items[this._selIndex].Tag;
            Helper.OpenInExplorer(sd.ShortcutPath);
        }

        void lvFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ShortcutDetails sd = (ShortcutDetails)e.Item.Tag;

            lblStatus.Text = sd.Description;

            this._selIndex = e.ItemIndex;
        }

        void MainWindow_Shown(object sender, EventArgs e)
        {
            bgWorker.RunWorkerAsync();
        }

        void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                lblStatus.Text = "Scanning ...";
                btnDeleteAll.Enabled = false;
                lvFiles.Enabled = false;
            }
            else if (e.ProgressPercentage == 2)
            {
                lblStatus.Text = string.Format(lblStatus.Tag.ToString(), Helper.ShortcutFiles.Count, Helper.EmptyDirectories.Count);
                btnDeleteAll.Enabled = true;
                lvFiles.Enabled = true;
            }
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lvFiles.Items.Clear();

            foreach (ShortcutDetails sd in Helper.ShortcutFiles)
            {
                ListViewItem item = new ListViewItem(sd.Name);
                item.SubItems.Add(sd.Target);
                item.SubItems.Add(sd.ShortcutPath);
                item.ImageIndex = 0;

                item.Tag = sd;

                lvFiles.Items.Add(item);
            }

            foreach (ShortcutDetails sd in Helper.EmptyDirectories)
            {
                ListViewItem item = new ListViewItem(sd.Name);
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

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bgWorker.ReportProgress(1);

            Helper.GetShortcuts();
        }
    }
}
