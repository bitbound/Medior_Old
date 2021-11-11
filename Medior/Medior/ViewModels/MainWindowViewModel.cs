using Medior.Core.Shared.BaseTypes;
using Medior.Core.Shared.Models;
using Medior.Core.Shared.Services;
using Medior.Core.Shared.Utilities;
using Medior.Enums;
using Medior.Models;
using Medior.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAppModuleStore _appModuleStore;
        private readonly IAppSettings _appSettings;
        private readonly ILogger<MainWindowViewModel> _logger;
        private AppModule? _selectedModule;

        public MainWindowViewModel(
            IAppSettings appSettings,
            IAppModuleStore appModuleStore,
            ILogger<MainWindowViewModel> logger)
        {
            _appSettings = appSettings;
            _appModuleStore = appModuleStore;
            _logger = logger;
        }

        public ObservableCollectionEx<AppModule> AppModulesFooter { get; } = new();
        public ObservableCollectionEx<AppModule> AppModulesMain { get; } = new();
        public string SearchText { get; set; } = string.Empty;

        public AppModule? SelectedModule
        {
            get => _selectedModule;
            set => SetProperty(ref _selectedModule, value);
        }

        public AppModule SettingsAppModule { get; } = new()
        {
            Label = "Settings",
            PageName = "Medior.Pages.SettingsPage"
        };
        public void FilterModules(string searchText)
        {
            try
            {
                foreach (var module in AppModulesMain)
                {
                    module.IsShown = module.Label.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase);
                    AppModulesMain.InvokeCollectionChanged();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering modules.");
            }
        }

        public void LoadMenuItems()
        {
            try
            {
                AppModulesMain.Clear();
                AppModulesFooter.Clear();

                foreach (var mainModule in _appModuleStore.MainModules)
                {
                    AppModulesMain.Add(mainModule);
                }

                foreach (var mainModule in _appModuleStore.FooterModules)
                {
                    AppModulesFooter.Add(mainModule);
                }

                var favModules = AppModulesMain.Where(x => _appSettings.FavoriteModules.Contains(x.Id));
                foreach (var module in favModules)
                {
                    module.IsFavorited = true;
                }

                SelectedModule = AppModulesMain.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu items.");
            }
        }
    }
}
