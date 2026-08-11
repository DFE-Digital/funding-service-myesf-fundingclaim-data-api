using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.FundingClaim.CorporateSchema.FundingClaims;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.Interfaces;
using Pds.FundingClaim.Services.Extensions;
using Pds.FundingClaim.Services.Implementations;
using Pds.FundingClaim.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ServicesWindow = Pds.FundingClaim.Services.Models.FundingClaimWindow;

namespace Pds.FundingClaim.Services.Tests.Unit
{
    [TestClass]
    public class FundingClaimWindowDataServiceTests
    {
        #region GetFundingClaimCurrentWindow

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimCurrentWindow_WhenCurrentWindowExists_ReturnsFundingClaimWindow()
        {
            //arrange
            var now = new DateTime(2000, 12, 12, 10, 10, 10);
            var resultFromRepository = new FundingClaimWindow
            {
                DataCollectionKey = "Data Collection key",
                SubmissionOpenDate = new DateTime(2000, 12, 12),
                SubmissionCloseDate = new DateTime(2001, 10, 10),
                SignatureCloseDate = new DateTime(2002, 12, 12),
                RequiresSignature = true
            };

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();
            mockFundingClaimWindowRepository.Setup(method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                    bool>>>()))
                .ReturnsAsync(resultFromRepository);

            var expected = new ServicesWindow
            {
                DataCollectionKey = resultFromRepository.DataCollectionKey,
                SubmissionOpenDate = resultFromRepository.SubmissionOpenDate,
                SubmissionCloseDate = resultFromRepository.SubmissionCloseDate,
                SignatureCloseDate = resultFromRepository.SignatureCloseDate,
                RequiresSignature = resultFromRepository.RequiresSignature
            };

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                              .Returns(now);

            var fundingClaimWindowDataService = new FundingClaimWindowDataService(
                mockFundingClaimWindowRepository.Object,
                mockSystemProvider.Object);

            //act
            var response = await fundingClaimWindowDataService.GetFundingClaimCurrentWindow();

            //assert
            response.Should().BeEquivalentTo(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimCurrentWindow_WhenRepositoryReturnsNoWindow_ReturnsNull()
        {
            //arrange
            var now = new DateTime(2000, 12, 12, 10, 10, 10);

            FundingClaimWindow result = null;
            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();
            mockFundingClaimWindowRepository.Setup(method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                                                bool>>>()))
                                            .ReturnsAsync(result);

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                              .Returns(now);

            var fundingClaimWindowDataService = new FundingClaimWindowDataService(
                mockFundingClaimWindowRepository.Object,
                mockSystemProvider.Object);

            //act
            var response = await fundingClaimWindowDataService.GetFundingClaimCurrentWindow();

            //assert
            response.Should().BeNull();
        }

        #endregion


        #region UpdateFundingClaimWindow

        [TestMethod, TestCategory("Unit")]
        public async Task UpdateFundingClaimWindow_WhenCalled_AddsAndUpdatesFundingClaimWindows()
        {
            //arrange
            var now = new DateTime(2000, 12, 12, 10, 10, 10);
            var dataSetVersionId = Guid.NewGuid();
            var fundingClaimDetails = new List<FundingClaimDetails>()
            {
                new FundingClaimDetails
                {
                    DataCollectionKey = "12-18",
                    SubmissionOpenDateUtc = "2020-01-01 08:16:21.000",
                    SubmissionCloseDateUtc = "2020-01-01 08:16:21.000",
                    SignatureCloseDateUtc = "2020-01-01 08:16:21.000",
                    RequiresSignature = false
                },
                new FundingClaimDetails
                {
                    DataCollectionKey = "19-20",
                    SubmissionOpenDateUtc = "2020-02-02 09:16:21.000",
                    SubmissionCloseDateUtc = "2020-02-02 09:16:21.000",
                    SignatureCloseDateUtc = "2020-02-02 09:16:21.000",
                    RequiresSignature = false
                }
            };

            var existingWindow = new FundingClaimWindow
            {
                DataCollectionKey = fundingClaimDetails[0].DataCollectionKey,
                Id = 1
            };

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();
            mockFundingClaimWindowRepository.Setup(method => method.GetAll())
                                            .ReturnsAsync(new List<FundingClaimWindow> { existingWindow });

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                              .Returns(now);
            mockSystemProvider.Setup(method => method.NewGuid())
                              .Returns(dataSetVersionId);

            var fundingClaimWindowDataService = new FundingClaimWindowDataService(
                mockFundingClaimWindowRepository.Object,
                mockSystemProvider.Object);

            //act
            await fundingClaimWindowDataService.UpdateFundingClaimWindow(fundingClaimDetails);

            //assert
            mockFundingClaimWindowRepository.Verify(
                repo => repo.Create(It.Is<FundingClaimWindow>(
                fc =>
                fc.DataCollectionKey == fundingClaimDetails[1].DataCollectionKey &&
                fc.DataSetVersionId == dataSetVersionId &&
                fc.SubmissionOpenDate == fundingClaimDetails[1].SubmissionOpenDateUtc.ToUtcDateTime() &&
                fc.SubmissionCloseDate == fundingClaimDetails[1].SubmissionCloseDateUtc.ToUtcDateTime() &&
                fc.SignatureCloseDate == fundingClaimDetails[1].SignatureCloseDateUtc.ToUtcDateTime() &&
                fc.RequiresSignature == fundingClaimDetails[1].RequiresSignature &&
                fc.FundingClaimsRetrieved == false &&
                fc.CreatedAt == now &&
                fc.LastUpdatedAt == now)),
                Times.Once);

            mockFundingClaimWindowRepository.Verify(
                repo => repo.Update(It.Is<FundingClaimWindow>(
                fc =>
                fc.DataCollectionKey == fundingClaimDetails[0].DataCollectionKey &&
                fc.SubmissionOpenDate == fundingClaimDetails[0].SubmissionOpenDateUtc.ToUtcDateTime() &&
                fc.SubmissionCloseDate == fundingClaimDetails[0].SubmissionCloseDateUtc.ToUtcDateTime() &&
                fc.SignatureCloseDate == fundingClaimDetails[0].SignatureCloseDateUtc.ToUtcDateTime() &&
                fc.RequiresSignature == fundingClaimDetails[1].RequiresSignature &&
                fc.LastUpdatedAt == now)), Times.Once);
        }

        #endregion
    }
}