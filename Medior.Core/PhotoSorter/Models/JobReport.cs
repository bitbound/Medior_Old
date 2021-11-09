using Medior.Core.PhotoSorter.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.PhotoSorter.Models
{
    public class JobReport
    {
        public string JobName { get; set; } = string.Empty;
        public SortOperation Operation { get; set; }
        public List<OperationResult> Results { get; set; } = new();
        public bool DryRun { get; set; }
        public string ReportPath { get; set; } = String.Empty;
    }
}
