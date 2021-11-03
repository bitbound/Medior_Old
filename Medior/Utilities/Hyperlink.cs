using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    public static class Hyperlink
    {
        public static RelayCommand<string> OpenInBrowser => new(
            param =>
            {
                if (param is not null)
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = param,
                        UseShellExecute = true
                    });
                }
            },
            param =>
            {
                return param is not null;
            }
        );
    }
}
