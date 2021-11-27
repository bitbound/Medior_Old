using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Service
{
    public class Main : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Create and handle SignalR connection.
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
