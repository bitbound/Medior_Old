using Medior.Core.BaseTypes;
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
        private readonly IConfigService _configService;
        private readonly IAppModuleStore _appModuleStore;
        private readonly ILogger<MainWindowViewModel> _logger;


        public MainWindowViewModel(
            IConfigService configService,
            IAppModuleStore appModuleStore,
            ILogger<MainWindowViewModel> logger)
        {
            _configService = configService;
            _appModuleStore = appModuleStore;
            _logger = logger;
        }

        public ObservableCollectionEx<AppModule> AppModulesMain { get; } = new();
        public ObservableCollectionEx<AppModule> AppModulesFooter { get; } = new();
        public string SearchText { get; set; } = string.Empty;
        public object? SelectedModule { get; set; }


        public void LoadMenuItems()
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

            var favModules = AppModulesMain.Where(x => _configService.Current.FavoriteModules.Contains(x.Id));
            foreach (var module in favModules)
            {
                module.IsFavorited = true;
            }

            SelectedModule = AppModulesMain.FirstOrDefault();
        }

        public void FilterModules(string searchText)
        {
            foreach (var module in AppModulesMain)
            {
                module.IsShown = module.Label.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase);
                AppModulesMain.InvokeCollectionChanged();
            }
        }
    }
}
