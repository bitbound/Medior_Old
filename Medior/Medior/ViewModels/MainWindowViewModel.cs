using Medior.Core.Shared.Models;
using Medior.Core.Shared.Services;
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
    public class MainWindowViewModel : ObservableObject
    {
        private readonly IConfigService _configService;
        private readonly IAppModuleStore _appModuleStore;
        private readonly ILogger<MainWindowViewModel> _logger;

        public ObservableCollection<AppModule> AppModulesMain { get; } = new();
        public ObservableCollection<AppModule> AppModulesFooter { get; } = new();

        public MainWindowViewModel(
            IConfigService configService,
            IAppModuleStore appModuleStore,
            ILogger<MainWindowViewModel> logger)
        {
            _configService = configService;
            _appModuleStore = appModuleStore;
            _logger = logger;
        }

        public async Task LoadMenuItems()
        {
            AppModulesMain.Clear();
            AppModulesFooter.Clear();

            foreach (var mainModule in _appModuleStore.AllModules.Where(x => x.ModuleType == AppModuleType.Main))
            {
                AppModulesMain.Add(mainModule);
            }

            foreach (var mainModule in _appModuleStore.AllModules.Where(x => x.ModuleType == AppModuleType.Footer))
            {
                AppModulesFooter.Add(mainModule);
            }

            var config = await _configService.GetConfig();
            var favModules = AppModulesMain.Where(x => config.FavoriteModules.Contains(x.Id));
            foreach (var module in favModules)
            {
                module.IsFavorited = true;
            }
        }
    }
}
