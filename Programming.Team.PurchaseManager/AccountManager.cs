
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.PurchaseManager
{
    public class AccountManager : IAccountManager
    {
        protected ILogger Logger { get; }
        protected IUserRepository UserRepository { get; }
        protected AccountService AccountService { get; }
        protected AccountLinkService AccountLinkService { get; }
        protected string PlatformName { get; }
        protected string RefreshAccountLinkUrl { get; }
        protected string ReturnAccountLinkUrl { get; }
        public AccountManager(ILogger<AccountManager> logger, IUserRepository userRepository, AccountService accountService, AccountLinkService accountLinkService, IConfiguration config)
        {
            Logger = logger;
            UserRepository = userRepository;
            AccountService = accountService;
            PlatformName = config["Stripe:PlatformName"] ?? throw new InvalidDataException();
            RefreshAccountLinkUrl = config["Stripe:RefreshAccountLinkUrl"] ?? throw new InvalidDataException();
            ReturnAccountLinkUrl = config["Stripe:ReturnAccountLinkUrl"] ?? throw new InvalidDataException();
            AccountLinkService = accountLinkService;
        }
        public async Task<string?> CreateAccountId(User user, string? stripeAccountId = null, CancellationToken token = default)
        {
            try
            {
                Account? acct = null;
                if (string.IsNullOrWhiteSpace(user.StripeAccountId) && string.IsNullOrWhiteSpace(stripeAccountId))
                {
                    var accountOptions = new AccountCreateOptions
                    {
                        Type = "express",
                        Country = "US",
                        Email = user.EmailAddress ?? throw new InvalidDataException("Email required"),
                        Capabilities = new AccountCapabilitiesOptions
                        {
                            Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
                        },
                        BusinessType = "individual", // Optional: "company" for orgs
                        Metadata = new Dictionary<string, string>
                        {
                            { "created_by", PlatformName}
                        }
                    };
                    acct = await AccountService.CreateAsync(accountOptions, cancellationToken: token);
                }
                else if (!string.IsNullOrWhiteSpace(user.StripeAccountId) && user.StripeStatus != "approved")
                {
                    acct = await AccountService.GetAsync(user.StripeAccountId, cancellationToken: token);
                    if (user.EmailAddress?.ToLower() != acct.Email.ToLower())
                        throw new InvalidDataException("Email Address Doesn't match");
                }
                else if(!string.IsNullOrWhiteSpace(stripeAccountId))
                {
                    acct = await AccountService.GetAsync(stripeAccountId, cancellationToken: token);
                    if (user.EmailAddress?.ToLower() != acct.Email.ToLower())
                        throw new InvalidDataException("Email Address Doesn't match");
                }
                if (acct == null)
                    return null;
                user.StripeAccountId = acct.Id;
                user.StripeUpdateDate = DateTime.UtcNow;
                user.StripeStatus = "pending";
                await UserRepository.Update(user, token: token);
                var link = await AccountLinkService.CreateAsync(new AccountLinkCreateOptions()
                {
                    Account = acct.Id,
                    RefreshUrl = RefreshAccountLinkUrl,
                    ReturnUrl = ReturnAccountLinkUrl,
                    Type = "account_onboarding"

                }, cancellationToken: token);
                return link.Url;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
            return null;
        }

        public async Task FinalizeAccount(User user, CancellationToken token = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.StripeAccountId))
                    throw new InvalidDataException($"{user.EmailAddress} does not have a stripe account assigned");
                var acct = await AccountService.GetAsync(user.StripeAccountId, cancellationToken: token);
                if (acct == null) throw new InvalidDataException($"{user.EmailAddress} {user.StripeAccountId} is invalid");
                user.StripeStatus = acct.PayoutsEnabled ? "approved" : "denied";
                user.StripeUpdateDate = DateTime.UtcNow;
                await UserRepository.Update(user, token:token);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public Task<string?> GetAccountId(User user, CancellationToken token = default)
        {
            if (user.StripeStatus != "approved")
                return Task.FromResult<string?>(null);
            return Task.FromResult(user.StripeAccountId);
        }
    }
}
