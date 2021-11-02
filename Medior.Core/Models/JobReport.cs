using Medior.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.Models
{
    public class JobReport
    {
        public string JobName { get; init; } = string.Empty;
        public SortOperation Operation { get; init; }
        public List<OperationResult> Results { get; init; } = new();
        public bool DryRun { get; internal set; }
    }
}
