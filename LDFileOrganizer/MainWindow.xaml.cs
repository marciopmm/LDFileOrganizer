using LDFileOrganizer.Analyzers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LDFileOrganizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AnalysisResult _result;
        private IAnalyzer _analyzer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (txtPath.Text != folder.SelectedPath)
                {
                    txtPath.Text = folder.SelectedPath;
                    _result = null;
                }
            }
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPath())
                return;

            Thread t = new Thread(new ThreadStart(AnalyzeThread));
            t.Start();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPath())
                return;

            Thread t = new Thread(new ThreadStart(GoThread));
            t.Start();
        }

        private void AnalyzeThread()
        {
            string path = null;
            txtPath.Dispatcher.Invoke(new Action(() => { path = txtPath.Text; }));
            
            StringBuilder sb = new StringBuilder();

            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Cursor = Cursors.Wait;
                    lblStatus.Content = "Analyzing folders...";
                    btnAnalyze.IsEnabled = false;
                    btnGo.IsEnabled = false;
                }));

                _analyzer = new DefaultAnalyzer(dir);
                _analyzer.FileAnalyzed += analyzer_FileAnalyzed;
                _result = _analyzer.Analyze();
                sb.AppendLine("Analysis of folder \"" + dir.FullName + "\" has returned the following results:");
                sb.AppendLine();
                sb.AppendLine("Subfolders found: " + _result.FoldersAffected.ToString());
                sb.AppendLine("Files found in folder and subfolders: " + _result.FilesAffected.ToString());
                sb.AppendLine("Duplicated files in folder and subfolders: " + _result.DuplicatedFiles.Length.ToString());
            }
            else
                MessageBox.Show("The chosen folder does not exists.", "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                txbResult.Text = sb.ToString();
                this.Cursor = null;
                lblStatus.Content = "Ready";
                progBar.Value = 0.0d;
                btnAnalyze.IsEnabled = true;
                btnGo.IsEnabled = true;
            }));
        }

        private void analyzer_FileAnalyzed(object sender, Tuple<string, double> e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txbResult.Text = "Analyzing file: " + e.Item1;
                progBar.Value = e.Item2;
            }));
        }

        private void GoThread()
        {
            string path = null;
            bool? duplicated = null, subFolders = null, numerals = null;
            txtPath.Dispatcher.Invoke(new Action(() => { path = txtPath.Text; }));
            txtPath.Dispatcher.Invoke(new Action(() => { 
                duplicated = chkDuplicated.IsChecked;
                subFolders = chkSubfolders.IsChecked;
                numerals = chkNumerals.IsChecked;
            }));

            StringBuilder sb = new StringBuilder();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (dir.Exists)
                {
                    _analyzer.FolderAnalyzed += analyzer_FolderAnalyzed;
                    _analyzer.FileRenamed += analyzer_FileRenamed;
                    _analyzer.DuplicatedFileRemoved += analyzer_DuplicatedFileRemoved;
                    _analyzer.DeletingDuplicatedFilesStarted += analyzer_DeletingDuplicatedFilesStarted;
                    _analyzer.DeletingFolderStarted += analyzer_DeletingFolderStarted;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.Cursor = Cursors.Wait;
                        lblStatus.Content = "Organizing and cleaning folders...";
                        btnAnalyze.IsEnabled = false;
                        btnGo.IsEnabled = false;
                    }));

                    AnalysisResult result2 = _analyzer.Execute(duplicated, subFolders, numerals);
                    sb.AppendLine("Analysis of folder \"" + dir.FullName + "\" has returned the following results after the execution:");
                    sb.AppendLine("Subfolders found: " + result2.FoldersAffected.ToString());
                    sb.AppendLine("Files found in folder and subfolders: " + result2.FilesAffected.ToString());
                    sb.AppendLine("Duplicated files in folder and subfolders: " + result2.DuplicatedFiles.Length.ToString());
                }
                else
                    MessageBox.Show("The chosen folder does not exists.", "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                txbResult.Text = sb.ToString();
                this.Cursor = null;
                lblStatus.Content = "Ready";
                progBar.Value = 0.0d;
                btnAnalyze.IsEnabled = true;
                btnGo.IsEnabled = true;
            }));
        }

        private void analyzer_DeletingFolderStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                progBar.Value = 0.0d;
                lblStatus.Content = "Deleting folders...";
            }));
        }

        private void analyzer_DuplicatedFileRemoved(object sender, Tuple<string, double> e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txbResult.Text = "Deleting file: " + e.Item1;
                progBar.Value = e.Item2;
            }));
        }

        private void analyzer_DeletingDuplicatedFilesStarted(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                progBar.Value = 0.0d;
                lblStatus.Content = "Removing duplicated files...";
            }));
        }

        private void analyzer_FileRenamed(object sender, Tuple<string, double> e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txbResult.Text = "Renaming file: " + e.Item1;
                progBar.Value = e.Item2;
            }));
        }

        private void analyzer_FolderAnalyzed(object sender, Tuple<string, double> e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txbResult.Text = "Analyzing folder: " + e.Item1;
                progBar.Value = e.Item2;
            }));
        }

        private bool CheckPath()
        {
            if (string.IsNullOrEmpty(txtPath.Text))
            {
                MessageBox.Show("Please choose or type a folder to analyze.", "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            return true;
        }
    }
}
