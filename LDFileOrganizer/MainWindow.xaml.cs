using LDFileOrganizer.Analyzers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var folder = new System.Windows.Forms.FolderBrowserDialog();
            if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = folder.SelectedPath;
            }
        }

        private void btnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPath())
                return;

            this.Cursor = Cursors.Wait;
            StringBuilder sb = new StringBuilder();

            DirectoryInfo dir = new DirectoryInfo(txtPath.Text);
            if (dir.Exists)
            {
                lblStatus.Content = "Analyzing folder...";
                IAnalyzer analyzer = new DefaultAnalyzer(dir);
                AnalysisResult result = analyzer.Analyze();
                lblStatus.Content = "Ready";
                sb.AppendLine("Analysis of folder \"" + dir.FullName + "\" has returned the following results:");
                sb.AppendLine();
                sb.AppendLine("Subfolders found: " + result.FoldersAffected.ToString());
                sb.AppendLine("Files found in folder and subfolders: " + result.FilesAffected.ToString());
                sb.AppendLine("Duplicated files in folder and subfolders: " + result.DuplicatedFiles.Length.ToString());
            }
            else
                MessageBox.Show("The chosen folder does not exists.", "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            txbResult.Text = sb.ToString();
            this.Cursor = null;
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckPath())
                return;

            this.Cursor = Cursors.Wait;
            StringBuilder sb = new StringBuilder();

            try
            {
                DirectoryInfo dir = new DirectoryInfo(txtPath.Text);
                if (dir.Exists)
                {
                    lblStatus.Content = "Analyzing folder...";
                    IAnalyzer analyzer = new DefaultAnalyzer(dir);
                    AnalysisResult result1 = analyzer.Analyze();
                    lblStatus.Content = "Ready";
                    sb.AppendLine("Analysis of folder \"" + dir.FullName + "\" has returned the following results:");
                    sb.AppendLine("*** Before execution ***");
                    sb.AppendLine("Subfolders found: " + result1.FoldersAffected.ToString());
                    sb.AppendLine("Files found in folder and subfolders: " + result1.FilesAffected.ToString());
                    sb.AppendLine("Duplicated files in folder and subfolders: " + result1.DuplicatedFiles.Length.ToString());

                    AnalysisResult result2 = analyzer.Execute(chkDuplicated.IsChecked, chkSubfolders.IsChecked);
                    sb.AppendLine("*** After execution ***");
                    sb.AppendLine("Subfolders found: " + result2.FoldersAffected.ToString());
                    sb.AppendLine("Files found in folder and subfolders: " + result2.FilesAffected.ToString());
                    sb.AppendLine("Duplicated files in folder and subfolders: " + result2.DuplicatedFiles.Length.ToString());
                }
                else
                    MessageBox.Show("The chosen folder does not exists.", "Pay attention...", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txbResult.Text = sb.ToString();
            this.Cursor = null;
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
