using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LDFileOrganizer.Analyzers
{
    public class AnalysisResult
    {
        public int FilesAffected { get; set; }
        public int FoldersAffected { get; set; }
        public FileInfo[] DuplicatedFiles { get; set; }
    }
}
