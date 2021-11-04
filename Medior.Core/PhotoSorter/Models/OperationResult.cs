using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.PhotoSorter.Models
{
    public class OperationResult
    {
        public bool IsSuccess => !WasSkipped && !HadError;
        public bool WasSkipped { get; set; }
        public bool HadError { get; set; }
        public bool FoundExifData { get; set; }
        public string PreOperationPath { get; set; } = string.Empty;
        public string PostOperationPath { get; set; } = string.Empty;

    }
}
