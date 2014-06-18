using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LDFileOrganizer.Analyzers
{
    public class DefaultAnalyzer : IAnalyzer
    {
        private double folderCount;
        private DirectoryInfo _dir;
        private DirectoryInfo[] _folders;
        private AnalysisResult _result;

        public AnalysisResult Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public event EventHandler<Tuple<string, double>> FileAnalyzed;
        public event EventHandler<Tuple<string, double>> DuplicatedFileRemoved;
        public event EventHandler<Tuple<string, double>> FileRenamed;
        public event EventHandler<Tuple<string, double>> FolderAnalyzed;
        public event EventHandler DeletingFolderStarted;
        public event EventHandler DeletingDuplicatedFilesStarted;
        public event EventHandler RenamingFilesStarted;

        public DefaultAnalyzer(DirectoryInfo dir)
        {
            _dir = dir;
        }

        public AnalysisResult Analyze()
        {
            _result = new AnalysisResult();
            _result.FilesAffected = GetAllFiles().Length;
            _result.FoldersAffected = GetAllFolders().Length;

            _result.DuplicatedFiles = GetDuplicatedFiles();

            return _result;
        }

        public AnalysisResult Execute(bool? removeDuplicated, bool? removeFolders, bool? removeNumerals)
        {
            if (removeFolders.HasValue && removeFolders.Value)
            {
                if (DeletingFolderStarted != null)
                    DeletingFolderStarted(this, EventArgs.Empty);

                folderCount = 0;
                _folders = _dir.GetDirectories();
                foreach (var folder in _folders)
                {
                    DeleteChildren(folder);
                }
            }

            if (removeDuplicated.HasValue && removeDuplicated.Value)
            {
                if (_result == null)
                    throw new ApplicationException("You have to make a first analysis fo your folder in order to remove duplicated files. Please do it by clicking the \"Analyze it!\" button before go.");

                if (DeletingDuplicatedFilesStarted != null)
                    DeletingDuplicatedFilesStarted(this, EventArgs.Empty);

                for (int i = 0; i < _result.DuplicatedFiles.Length; i++)
                {
                    FileInfo file = _result.DuplicatedFiles[i];
                    file.Attributes = FileAttributes.Normal;
                    file.Delete();

                    if (DuplicatedFileRemoved != null)
                        DuplicatedFileRemoved(this, new Tuple<string, double>(file.FullName, ((double)i / (double)_result.DuplicatedFiles.Length) * 100.0d));
                }
            }

            if (removeNumerals.HasValue && removeNumerals.Value)
            {
                if (RenamingFilesStarted != null)
                    RenamingFilesStarted(this, EventArgs.Empty);

                Regex regex = new Regex(@"(\(?\d+[)\s-_]+)?(.+)[\s-_]+(\(\d+\))?(\..{2,4})");
                FileInfo[] files = GetAllFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    if (regex.IsMatch(file.Name))
                    {
                        try 
                        { 
                            file.MoveTo(Path.Combine(file.DirectoryName, regex.Replace(file.Name, "$2$4")));
                            if (FileRenamed != null)
                                FileRenamed(this, new Tuple<string, double>(file.FullName, ((double)i / (double)files.Length) * 100.0d));
                        }
                        catch { continue; }
                    }
                }
            }

            _result = Analyze();
            return _result;
        }

        private void DeleteChildren(DirectoryInfo folder)
        {
            foreach (var f in folder.GetDirectories())
            {
                DeleteChildren(f);
            }

            foreach (var file in folder.GetFiles())
            {
                string fileName = file.Name.Replace(file.Extension, "");
                if (_dir.GetFiles(fileName + file.Extension).Length == 0)
                    file.MoveTo(Path.Combine(_dir.FullName, fileName) + file.Extension);
                else
                {
                    int i = 1;
                    while (_dir.GetFiles(fileName + " (" + i.ToString() + ")" + file.Extension).Length > 0)
                    { i++; }
                    file.MoveTo(Path.Combine(_dir.FullName, fileName + " (" + i.ToString() + ")" + file.Extension));
                }
            }

            if (folder.GetFiles().Length == 0)
            {
                folder.Attributes = FileAttributes.Normal;
                folder.Delete();
            }

            if (FolderAnalyzed != null)
                FolderAnalyzed(this, new Tuple<string, double>(folder.FullName, (folderCount / (double)_folders.Length) * 100.0d));

            folderCount++;
        }

        private FileInfo[] GetAllFiles()
        {
            return _dir.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(x => x.Name).ToArray();
        }

        private DirectoryInfo[] GetAllFolders()
        {
            return _dir.GetDirectories("*", SearchOption.AllDirectories);
        }

        private FileInfo[] GetDuplicatedFiles()
        {
            FileInfo[] files = GetAllFiles();
            List<FileInfo> list = new List<FileInfo>();
            List<FileInfo> duplicated = new List<FileInfo>();
            DefaultFileComparer comparer = new DefaultFileComparer();

            for(int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];
                if (list.Contains(file, comparer))
                    duplicated.Add(file);
                else
                    list.Add(file);

                if (FileAnalyzed != null)
                    FileAnalyzed(this, new Tuple<string, double>(file.FullName, ((double)i / (double)files.Length) * 100.0d));
            }

            return duplicated.ToArray();
        }
    }
}
