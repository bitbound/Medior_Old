using Medior.Core.Shared.Models;
using Medior.Core.Shared.Services;
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
        private readonly ILogger<MainWindowViewModel> _logger;

        public string Text { get; set; } = "Test";

        public MainWindowViewModel(
            IConfigService configService,
            ILogger<MainWindowViewModel> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public Task LoadFavorites()
        {
            return Task.CompletedTask;
            // TODO
            //var config = await _configService.GetConfig();

            //foreach (var moduleId in config.FavoriteModules)
            //{
                
            //}
        }
    }
}
