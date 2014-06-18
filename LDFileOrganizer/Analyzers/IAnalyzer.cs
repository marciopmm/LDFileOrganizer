using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDFileOrganizer.Analyzers
{
    public interface IAnalyzer
    {
        AnalysisResult Result { get; set; }
        event EventHandler<Tuple<string, double>> FileAnalyzed;
        event EventHandler<Tuple<string, double>> DuplicatedFileRemoved;
        event EventHandler<Tuple<string, double>> FileRenamed;
        event EventHandler<Tuple<string, double>> FolderAnalyzed;
        event EventHandler DeletingFolderStarted;
        event EventHandler DeletingDuplicatedFilesStarted;
        event EventHandler RenamingFilesStarted;
        AnalysisResult Analyze();
        AnalysisResult Execute(bool? removeDuplicated, bool? removeFolders, bool? removeNumerals);
    }
}
