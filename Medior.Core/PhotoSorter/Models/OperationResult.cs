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
        public bool WasSkipped { get; init; }
        public bool HadError { get; init; }
        public bool FoundExifData { get; init; }
        public string PreOperationPath { get; init; } = string.Empty;
        public string PostOperationPath { get; init; } = string.Empty;

    }
}
