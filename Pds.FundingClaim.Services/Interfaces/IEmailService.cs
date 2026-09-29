using Sfa.Sfs.Contracts.Messaging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Services.Interfaces
{
    /// <summary>
    /// Defines the interface for sending emails related to funding claim transactions.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a message that a funding claim is ready to view.
        /// </summary>
        /// <param name="fundingClaimIds">The Ids of funding claims that are ready to view.</param>
        /// <returns>The asynchronous task.</returns>
        Task SendFundingClaimReadyToViewEmail(List<int> fundingClaimIds);

        /// <summary>
        /// Sends a message that a reconciliation is ready to view.
        /// </summary>
        /// <param name="reconciliationId">The Id of the reconciliation that is ready to view.</param>
        /// <returns>The asynchronous task.</returns>
        Task SendReconciliationReadyToBeViewedEmail(int reconciliationId);

        /// <summary>
        /// Sends a message that an exception occurred while reading the reconciliation feed.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <returns>The asynchronous task.</returns>
        Task SendFeedReadExceptionEmail(FeedReadExceptionMessage message);

        /// <summary>
        /// Sends a message when threshold time has passed whilst reading reconciliation feed.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <returns>The asynchronous task.</returns>
        Task SendFeedExceededReadThresholdWarningEmail(FeedReadThresholdExceededWarningMessage message);
    }
}