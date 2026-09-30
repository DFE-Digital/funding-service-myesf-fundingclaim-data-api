using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Azure.Interfaces.Messaging;
using Pds.Core.Logging;
using Pds.FundingClaim.Services.Constants;
using Pds.FundingClaim.Services.Implementations;
using Sfa.Sfs.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Services.Tests.Unit
{
    [TestClass]
    public class EmailServiceTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task SendFundingClaimReadyToViewEmail_WhenCalled_SendsReadyToSignViewMessages()
        {
            //arrange
            var fundingClaimIds = new List<int> { 1, 2 };
            var azureServiceBusMessagingService = new Mock<IAzureMessagingServiceBusService>();

            var mockLogger = new Mock<ILoggerAdapter<EmailService>>();
            var emailService = new EmailService(azureServiceBusMessagingService.Object, mockLogger.Object);

            //act
            await emailService.SendFundingClaimReadyToViewEmail(fundingClaimIds);

            //assert
            azureServiceBusMessagingService.Verify(
               repo => repo.SendMessageAsync(It.Is<FundingClaimReadyToViewMessage>(fc => fc.FundingClaimId == fundingClaimIds[0]), ServiceConstants.FundingClaimReadyToViewEmailQueue, It.IsAny<string>()), Times.Once);
            azureServiceBusMessagingService.Verify(
               repo => repo.SendMessageAsync(It.Is<FundingClaimReadyToViewMessage>(fc => fc.FundingClaimId == fundingClaimIds[1]), ServiceConstants.FundingClaimReadyToViewEmailQueue, It.IsAny<string>()), Times.Once);
            mockLogger.Verify(
                l => l.LogInformation(
                "Creating azure service bus message for sending email for ready to view funding claim {fundingClaimId}",
                fundingClaimIds[0]), Times.Once);
            mockLogger.Verify(
                l => l.LogInformation(
                "Creating azure service bus message for sending email for ready to view funding claim {fundingClaimId}",
                fundingClaimIds[1]), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendReconciliationReadyToBeViewedEmail_WhenCalled_SendsReadyToViewMessage()
        {
            //arrange
            var reconciliationId = 1;
            var azureServiceBusMessagingService = new Mock<IAzureMessagingServiceBusService>();

            var mockLogger = new Mock<ILoggerAdapter<EmailService>>();
            var emailService = new EmailService(azureServiceBusMessagingService.Object, mockLogger.Object);

            //act
            await emailService.SendReconciliationReadyToBeViewedEmail(reconciliationId);

            //assert
            azureServiceBusMessagingService.Verify(
                repo => repo.SendMessageAsync(It.Is<ReconciliationReadyToBeViewedMessage>(r => r.ReconciliationId == reconciliationId), ServiceConstants.ReconciliationReadyToBeViewedEmailQueue, It.IsAny<string>()), Times.Once);
            mockLogger.Verify(
                l => l.LogInformation(
                    $"Creating azure service bus message for sending an email. Reconciliation funding claim that is ready to be viewed {reconciliationId}."), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendFeedReadExceptionEmail_WhenCalled_SendsExceptionMessage()
        {
            //arrange
            var message = new FeedReadExceptionMessage { Url = "Url", Type = FeedReadExceptionMessage.ExceptionType.EmptyPageOnFeed, Bookmark = Guid.NewGuid() };
            var log = $"Creating azure service bus message for sending an email. Reconciliation feed read exception email of type {message.Type} and Url {message.Url} and bookmark {message.Bookmark}.";

            var azureServiceBusMessagingService = new Mock<IAzureMessagingServiceBusService>(MockBehavior.Strict);
            azureServiceBusMessagingService.Setup(service => service.SendMessageAsync(message, ServiceConstants.FeedReadExceptionEmailQueue, It.IsAny<string>())).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILoggerAdapter<EmailService>>(MockBehavior.Strict);
            mockLogger.Setup(l => l.LogInformation(log)).Verifiable();

            var emailService = new EmailService(azureServiceBusMessagingService.Object, mockLogger.Object);

            //act
            await emailService.SendFeedReadExceptionEmail(message);

            //assert
            azureServiceBusMessagingService.Verify(
                repo => repo.SendMessageAsync(It.Is<FeedReadExceptionMessage>(m => m.Url == message.Url && m.Bookmark == message.Bookmark && m.Type == message.Type), ServiceConstants.FeedReadExceptionEmailQueue, It.IsAny<string>()), Times.Once);
            mockLogger.Verify(l => l.LogInformation(log), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task SendFeedExceededReadThresholdWarningEmail_WhenCalled_SendsWarningMessage()
        {
            //arrange
            var message = new FeedReadThresholdExceededWarningMessage
            {
                Start = DateTime.Now.AddMinutes(-10),
                Now = DateTime.Now,
                BookmarkId = Guid.NewGuid(),
                LastPageUrl = "url"
            };
            var log = $"Creating azure service bus message for sending an email. Reconciliation feed read threshold exceeded warning mail for url {message.LastPageUrl} and bookmark {message.BookmarkId}.";

            var azureServiceBusMessagingService = new Mock<IAzureMessagingServiceBusService>(MockBehavior.Strict);
            azureServiceBusMessagingService.Setup(service => service.SendMessageAsync(message, ServiceConstants.FeedReadThresholdExceededWarningEmailQueue, It.IsAny<string>())).Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILoggerAdapter<EmailService>>(MockBehavior.Strict);
            mockLogger.Setup(l => l.LogInformation(log)).Verifiable();

            var emailService = new EmailService(azureServiceBusMessagingService.Object, mockLogger.Object);

            //act
            await emailService.SendFeedExceededReadThresholdWarningEmail(message);

            //assert
            azureServiceBusMessagingService.Verify(
                repo => repo.SendMessageAsync(It.Is<FeedReadThresholdExceededWarningMessage>(m => m.Start == message.Start && m.Now == message.Now && m.BookmarkId == message.BookmarkId && m.LastPageUrl == message.LastPageUrl), ServiceConstants.FeedReadThresholdExceededWarningEmailQueue, It.IsAny<string>()), Times.Once);

            mockLogger.Verify(l => l.LogInformation(log), Times.Once);
        }
    }
}