using Medior.BaseTypes;
using Medior.Enums;
using Medior.Models.Messages;
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
        Task<Result> PurchaseProSubscription();
    }

    public class AccountService : IAccountService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<AccountService> _logger;
        private readonly string _pro1ProductId = "9NGBJCSW13PW";
        private readonly string _unlockProductId = "9NSBFJT6CB9P";
        private readonly string _pro1Sku = "Pro1";
        public AccountService(
            IMessagePublisher messagePublisher,
            ILogger<AccountService> logger)
        {
            _messagePublisher = messagePublisher;
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

                var result = await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

                if (result.ExtendedError is not null)
                {
                    _logger.LogError(result.ExtendedError, "Error getting store products.");
                    return Result.Fail<SubscriptionLevel>("Unable to communicate with Windows Store.");
                }

                var product = result.Products?.Values?.FirstOrDefault(x => x.StoreId == _unlockProductId);

                if (product?.IsInUserCollection == true)
                {
                    return Result.Ok(SubscriptionLevel.DevUnlock);
                }

                return Result.Ok(SubscriptionLevel.Free);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking subscription level.");
                return Result.Fail<SubscriptionLevel>("Failed to get subscription information.");
            }
        }

        public async Task<Result> PurchaseProSubscription()
        {
            try
            {
                var context = StoreContext.GetDefault();
                var result = await context.GetAssociatedStoreProductsAsync(new string[] { "Durable" });

                if (result.ExtendedError is not null)
                {
                    _logger.LogError(result.ExtendedError, "Error getting store products.");
                    return Result.Fail("Unable to communicate with Windows Store.");
                }

                var product = result.Products?.Values?.FirstOrDefault(x => x.StoreId == _pro1ProductId);

                if (product is null)
                {
                    _logger.LogError("Could not find product in Windows Store.");
                    return Result.Fail("Unable to find item in the Windows Store.");
                }

                if (product.IsInUserCollection)
                {
                    return Result.Fail("You already have a Pro subscription.");
                }

                var purchaseResult = await product.RequestPurchaseAsync();
                if (purchaseResult.Status != StorePurchaseStatus.Succeeded)
                {
                    _logger.LogError(result.ExtendedError, "Error getting store products.");
                    return Result.Fail("Unable to communicate with Windows Store.");
                }

                _messagePublisher.Messenger.Send(new SubscriptionMessage(SubscriptionLevel.Pro1));
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while purchasing subscription.");
                return Result.Fail("Unexpected error occurred while completing purchase.");
            }
     
        }
    }
}
