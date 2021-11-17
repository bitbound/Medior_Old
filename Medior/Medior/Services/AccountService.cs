using Medior.BaseTypes;
using Medior.Enums;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.System;

namespace Medior.Services
{
    public interface IAccountService
    {
        Task<User> GetCurrentUser();
        Task<Result<SubscriptionLevel>> GetSubscriptionLevel();
    }

    public class AccountService : IAccountService
    {
        private readonly string _pro1Sku = "Pro1";
        private readonly ILogger<AccountService> _logger;

        public AccountService(ILogger<AccountService> logger)
        {
            _logger = logger;
        }

        public async Task<User> GetCurrentUser()
        {
            // There would only be more than 1 user on multi-user
            // apps (e.g. an XBox game with local multiplayer).
            var users = await User.FindAllAsync();
            return users[0];
        }

        public async Task<Result<SubscriptionLevel>> GetSubscriptionLevel()
        {
            try
            {
                var context = StoreContext.GetDefault();

                var appLicense = await context.GetAppLicenseAsync();

                foreach (var addon in appLicense.AddOnLicenses)
                {
                    if (!addon.Value.IsActive)
                    {
                        continue;
                    }

                    if (addon.Value.SkuStoreId.StartsWith(_pro1Sku))
                    {
                        return Result.Ok(SubscriptionLevel.Pro1);
                    }

                }

                return Result.Ok(SubscriptionLevel.Free);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking subscription level.");
                return Result.Fail<SubscriptionLevel>("Failed to get subscription information.");
            }
        }
    }
}
