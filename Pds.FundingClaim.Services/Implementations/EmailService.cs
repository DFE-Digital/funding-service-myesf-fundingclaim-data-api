using Pds.Core.Azure.Interfaces.Messaging;
using Pds.Core.Logging;
using Pds.FundingClaim.Services.Constants;
using Pds.FundingClaim.Services.Interfaces;
using Sfa.Sfs.Contracts.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Services.Implementations
{
    /// <inheritdoc cref="IEmailService"/>
    public class EmailService : IEmailService
    {
        private readonly ILoggerAdapter<EmailService> _logger;
        private readonly IAzureMessagingServiceBusService _azureMessagingServiceBusService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="azureServiceBusMessagingService">The azure service bus messaging service.</param>
        /// <param name="logger">The logging service.</param>
        public EmailService(IAzureMessagingServiceBusService azureServiceBusMessagingService, ILoggerAdapter<EmailService> logger)
        {
            _azureMessagingServiceBusService = azureServiceBusMessagingService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task SendFundingClaimReadyToViewEmail(List<int> fundingClaimIds)
        {
            foreach (var fundingClaimId in fundingClaimIds)
            {
                _logger.LogInformation(
                    "Creating azure service bus message for sending email for ready to view funding claim {fundingClaimId}",
                    fundingClaimId);

                var message = new FundingClaimReadyToViewMessage
                {
                    FundingClaimId = fundingClaimId
                };

                await _azureMessagingServiceBusService.SendMessageAsync(message, ServiceConstants.FundingClaimReadyToViewEmailQueue);
            }
        }

        /// <inheritdoc/>
        public async Task SendReconciliationReadyToBeViewedEmail(int reconciliationId)
        {
            _logger.LogInformation(
                $"Creating azure service bus message for sending an email. Reconciliation funding claim that is ready to be viewed {reconciliationId}.");

            var message = new ReconciliationReadyToBeViewedMessage
            {
                ReconciliationId = reconciliationId
            };
            await _azureMessagingServiceBusService.SendMessageAsync(message, ServiceConstants.ReconciliationReadyToBeViewedEmailQueue);
        }

        /// <inheritdoc/>
        public async Task SendFeedReadExceptionEmail(FeedReadExceptionMessage message)
        {
            _logger.LogInformation(
                $"Creating azure service bus message for sending an email. Reconciliation feed read exception email of type {message.Type} and Url {message.Url} and bookmark {message.Bookmark}.");

            await _azureMessagingServiceBusService.SendMessageAsync(message, ServiceConstants.FeedReadExceptionEmailQueue);
        }

        /// <inheritdoc/>
        public async Task SendFeedExceededReadThresholdWarningEmail(FeedReadThresholdExceededWarningMessage message)
        {
            _logger.LogInformation(
                $"Creating azure service bus message for sending an email. Reconciliation feed read threshold exceeded warning mail for url {message.LastPageUrl} and bookmark {message.BookmarkId}.");

            await _azureMessagingServiceBusService.SendMessageAsync(message, ServiceConstants.FeedReadThresholdExceededWarningEmailQueue);
        }
    }
}