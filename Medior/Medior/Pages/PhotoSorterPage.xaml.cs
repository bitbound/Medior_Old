using Medior.AppModules.PhotoSorter.Models;
using Medior.Extensions;
using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoSorterPage : Page
    {
        private AsyncRelayCommand? _deleteJob;
        private AsyncRelayCommand? _renameJob;
        private RelayCommand? _saveJob;
        private AsyncRelayCommand? _newJob;
        private AsyncRelayCommand? _showDestinationTransform;
        private AsyncRelayCommand? _startJob;
        private RelayCommand? _cancelJob;
        private CancellationTokenSource? _jobCancelTokenSource;
        private CancellationToken _jobCancelToken;

        public PhotoSorterPage()
        {
            InitializeComponent();
        }

        public PhotoSorterViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<PhotoSorterViewModel>();

        public RelayCommand CancelJob
        {
            get
            {
                if (_cancelJob is null)
                {
                    _cancelJob = new(
                        () =>
                        {
                            _jobCancelTokenSource?.Cancel();
                        },
                        () => ViewModel.IsJobRunning
                    );
                }
                return _cancelJob;
            }
        }

        public AsyncRelayCommand NewJob
        {
            get
            {
                if (_newJob is null)
                {
                    _newJob = new(
                        async () =>
                        {
                            var (result, sortJobName) = await this.Prompt("New Sort Job",
                            "Enter a name for the new sort job.",
                            "Sort job name",
                            "Save");

                            if (result == ContentDialogResult.Primary)
                            {
                                if (string.IsNullOrWhiteSpace(sortJobName))
                                {
                                    await this.Alert("Name Required", "You must specify a name.");
                                    return;
                                }
                                ViewModel.CreateNewSortJob(sortJobName.Trim());
                            }
                        }
                    );
                }
                return _newJob;
            }
        }

        public AsyncRelayCommand DeleteJob
        {
            get
            {
                if (_deleteJob is null)
                {
                    _deleteJob = new(
                        async () =>
                        {
                            var result = await this.Confirm("Confirm Delete",
                               "Are you sure you want to delete this sort job?");

                            if (result == ContentDialogResult.Primary)
                            {
                                ViewModel.DeleteSortJob();
                            }
                        },
                        () => ViewModel.SelectedJob is not null
                    );
                }
                return _deleteJob;
            }
        }

        public AsyncRelayCommand RenameJob
        {
            get
            {
                if (_renameJob is null)
                {
                    _renameJob = new(
                        async () =>
                        {
                            var (result, newName) = await this.Prompt("New Name",
                               "Enter a new name for this sort job.",
                               "New name for sort job",
                               "Save");

                            if (result == ContentDialogResult.Primary)
                            {
                                if (string.IsNullOrWhiteSpace(newName))
                                {
                                    await this.Alert("Name Required", "You must specify a name.");
                                    return;
                                }
                                ViewModel.RenameSortJob(newName.Trim());
                            }
                        },
                        () => ViewModel.SelectedJob is not null
                    );
                }
                return _renameJob;
            }
        }

        public RelayCommand SaveJob
        {
            get
            {
                if (_saveJob is null)
                {
                    _saveJob = new(
                        () =>
                        {
                            ViewModel.SaveJob();
                            SavedTip.IsOpen = true;
                            UpdateCommandsCanExecute();

                            Debouncer.Debounce(TimeSpan.FromSeconds(2), () =>
                            {
                                DispatcherQueue.TryEnqueue(() =>
                                {
                                    SavedTip.IsOpen = false;
                                });
                            });
                          
                        },
                        () => ViewModel.SelectedJob is not null
                    );
                }
                return _saveJob;
            }
        }

        public AsyncRelayCommand ShowDestinationTransform
        {
            get
            {
                if (_showDestinationTransform is null)
                {
                    _showDestinationTransform = new(
                        async () =>
                        {
                            await this.Alert("Destination Transform", ViewModel.GetDestinationTransform());
                        },
                        () => !string.IsNullOrWhiteSpace(ViewModel.SelectedJob?.DestinationFile)
                    );
                }
                return _showDestinationTransform;
            }
        }

        public AsyncRelayCommand StartJob
        {
            get
            {
                if (_startJob is null)
                {
                    _startJob = new(
                        async () =>
                        {
                            ViewModel.SaveJob();
                            _jobCancelTokenSource = new CancellationTokenSource();
                            _jobCancelToken = _jobCancelTokenSource.Token;
                            ViewModel.IsJobRunning = true;
                            UpdateCommandsCanExecute();
                            JobReport report;
                            try
                            {
                                report = await Task.Run(async () => await ViewModel.StartJob(_jobCancelToken));
                            }
                            finally
                            {
                                ViewModel.IsJobRunning = false;
                                UpdateCommandsCanExecute();
                            }

                            var result = await this.Confirm(
                                "Job Finished",
                                "Job run completed.  Would you like to open the report directory?");
                            if (result == ContentDialogResult.Primary)
                            {
                                Process.Start(new ProcessStartInfo()
                                {
                                    FileName = Path.GetDirectoryName(report.ReportPath) ?? "",
                                    UseShellExecute = true
                                });
                            }
                        },
                        () => ViewModel.SelectedJob is not null && !ViewModel.IsJobRunning
                    );
                }
                return _startJob;
            }
        }

        private void DestinationFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowDestinationTransform.NotifyCanExecuteChanged();
        }

        private void SortJobComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCommandsCanExecute();
        }

        private void UpdateCommandsCanExecute()
        {
            SaveJob.NotifyCanExecuteChanged();
            DeleteJob.NotifyCanExecuteChanged();
            RenameJob.NotifyCanExecuteChanged();
            ShowDestinationTransform.NotifyCanExecuteChanged();
            StartJob.NotifyCanExecuteChanged();
            CancelJob.NotifyCanExecuteChanged();
        }
    }
}
