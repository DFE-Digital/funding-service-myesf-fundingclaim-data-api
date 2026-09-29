using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.FundingClaim.Api.Controllers;
using Pds.FundingClaim.Api.Models;
using Pds.FundingClaim.CorporateSchema.FundingClaims;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServicesWindow = Pds.FundingClaim.Services.Models.FundingClaimWindow;

namespace Pds.FundingClaim.Api.Tests.Unit
{
    [TestClass]
    public class FundingClaimControllerTests
    {
        #region Public Methods

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimLastRetrievedSetting_WhenRequested_CreatesLogAndReturnsFundingClaimLastRetrievedSetting()
        {
            //arrange
            var resultFromService = "Test Result";
            var mockSettingDataService = new Mock<ISettingDataService>();
            mockSettingDataService.Setup(service => service.GetFundingClaimLastRetrievedSetting())
                             .ReturnsAsync(resultFromService);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(mockSettingDataService.Object, null, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.GetFundingClaimLastRetrievedSetting();

            //assert
            response.Should().BeEquivalentTo(resultFromService);
            mockLogger.Verify(l => l.LogInformation(
                $"GetFundingClaimLastRetrievedSetting returned setting value: {response}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimPollingSetting_WhenRequested_CreatesLogAndReturnsFundingClaimPollingSetting()
        {
            //arrange
            var resultFromService = "Test Result";

            var mockSettingDataService = new Mock<ISettingDataService>();
            mockSettingDataService.Setup(service => service.GetFundingClaimPollingSetting())
                             .ReturnsAsync(resultFromService);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(mockSettingDataService.Object, null, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.GetFundingClaimPollingSetting();

            //assert
            response.Should().BeEquivalentTo(resultFromService);
            mockLogger.Verify(l => l.LogInformation(
                $"GetFundingClaimPollingSetting returned setting value: {response}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetUseJsonFormatOfFundingClaimsSetting_WhenRequested_CreatesLogAndReturnsSetting()
        {
            //arrange
            var resultFromService = "Test Result";

            var mockSettingDataService = new Mock<ISettingDataService>();
            mockSettingDataService.Setup(service => service.GetUseJsonFormatOfFundingClaimsSetting())
                             .ReturnsAsync(resultFromService);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(mockSettingDataService.Object, null, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.GetUseJsonFormatOfFundingClaimsSetting();

            //assert
            response.Should().BeEquivalentTo(resultFromService);
            mockLogger.Verify(l => l.LogInformation(
                $"GetUseJsonFormatOfFundingClaimsSetting returned setting value: {response}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimCurrentWindow_WhenServiceReturnsWindow_CreatesLogAndReturnsCurrentWindow()
        {
            //arrange
            var resultFromService = new ServicesWindow
            {
                DataCollectionKey = "Data Collection key",
                SubmissionOpenDate = new DateTime(2000, 12, 12),
                SubmissionCloseDate = new DateTime(2001, 10, 10),
                SignatureCloseDate = new DateTime(2002, 12, 12),
                RequiresSignature = true
            };

            var mockFundingClaimWindowDataService = new Mock<IFundingClaimWindowDataService>();
            mockFundingClaimWindowDataService.Setup(service => service.GetFundingClaimCurrentWindow())
                             .ReturnsAsync(resultFromService);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(null, mockFundingClaimWindowDataService.Object, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.GetFundingClaimCurrentWindow();

            //assert
            response.Should().BeEquivalentTo(resultFromService);
            mockLogger.Verify(l => l.LogInformation(
                $"GetFundingClaimCurrentWindow returned window with Data Collection key: {resultFromService.DataCollectionKey}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimCurrentWindow_WhenServiceReturnsNoWindow_CreatesLogAndReturnsNull()
        {
            //arrange
            ServicesWindow resultFromService = null;


            var mockFundingClaimWindowDataService = new Mock<IFundingClaimWindowDataService>();
            mockFundingClaimWindowDataService.Setup(service => service.GetFundingClaimCurrentWindow())
                             .ReturnsAsync(resultFromService);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(null, mockFundingClaimWindowDataService.Object, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.GetFundingClaimCurrentWindow();

            //assert
            response.Should().BeNull();
            mockLogger.Verify(l => l.LogInformation(
                "GetFundingClaimCurrentWindow returned window with Data Collection key: "));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task UpdateFundingClaimWindow_WhenSaveIsSuccessful_CreatesLogAndReturnsSuccessMessage()
        {
            //arrange
            var fundingClaimDetails = new List<FundingClaimDetails>();


            var mockFundingClaimWindowDataService = new Mock<IFundingClaimWindowDataService>();
            mockFundingClaimWindowDataService.Setup(service => service.UpdateFundingClaimWindow(fundingClaimDetails))
                             .Returns(Task.CompletedTask);
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();

            var fundingClaimController = new FundingClaimController(null, mockFundingClaimWindowDataService.Object, null, mockLogger.Object);

            //act
            var response = await fundingClaimController.UpdateFundingClaimWindow(fundingClaimDetails);

            //assert
            response.Should().BeOfType(typeof(OkResult));
            mockLogger.Verify(l => l.LogInformation("UpdateFundingClaimWindow ran successfully."));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task CreateFundingClaims_CreatesLogAndReturnsSuccessMessage()
        {
            //arrange
            var fundingClaimsList = new List<CorporateSchema.FundingClaims.FundingClaim>();
            var fundingClaimWindowId = 1;

            var mockFundingClaimDataService = new Mock<IFundingClaimDataService>();
            mockFundingClaimDataService.Setup(service => service.CreateFundingClaims(fundingClaimsList, fundingClaimWindowId))
                 .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();
            var fundingClaimController = new FundingClaimController(null, null, mockFundingClaimDataService.Object, mockLogger.Object);

            var request = new CreateFundingClaimsApiRequest { FundingClaims = fundingClaimsList, FundingClaimWindowId = fundingClaimWindowId };

            //act
            var response = await fundingClaimController.CreateFundingClaims(request);

            //assert
            response.Should().BeOfType(typeof(CreatedResult));
            mockLogger.Verify(l => l.LogInformation(
                "CreateFundingClaims ran successfully for funding claim window {FundingClaimWindowId}.", fundingClaimWindowId));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimById_CreatesLogAndReturnsFundingClaim()
        {
            //arrange
            var fundingClaim = new Services.Models.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new ServicesWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 1,
                Title = "Test",
            };

            var mockFundingClaimDataService = new Mock<IFundingClaimDataService>();
            mockFundingClaimDataService.Setup(service => service.GetFundingClaimById(fundingClaim.Id))
                 .Returns(Task.FromResult(fundingClaim));

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();
            var fundingClaimController = new FundingClaimController(null, null, mockFundingClaimDataService.Object, mockLogger.Object);


            //act
            var response = await fundingClaimController.GetFundingClaimById(fundingClaim.Id);

            //assert
            response.Should().BeEquivalentTo(fundingClaim);
            mockLogger.Verify(l => l.LogInformation($"Funding claim with Id [{response.Id}] returned from GetFundingClaimById using Id {fundingClaim.Id}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimById_CreatesLogAndReturnsNullResponse()
        {
            //arrange
            var mockFundingClaimDataService = new Mock<IFundingClaimDataService>();
            mockFundingClaimDataService.Setup(service => service.GetFundingClaimById(1))
                 .Returns(Task.FromResult((Services.Models.FundingClaim)null));

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();
            var fundingClaimController = new FundingClaimController(null, null, mockFundingClaimDataService.Object, mockLogger.Object);


            //act
            var response = await fundingClaimController.GetFundingClaimById(1);

            //assert
            response.Should().BeEquivalentTo((Services.Models.FundingClaim)null);
            mockLogger.Verify(l => l.LogInformation($"Null returned from GetFundingClaimById using Id 1"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_CreatesLogAndReturnsPreviouslySignedFundingClaim()
        {
            //arrange
            var fundingClaim = new Services.Models.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new ServicesWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 1,
                Title = "Test",
            };

            var mockFundingClaimDataService = new Mock<IFundingClaimDataService>();
            mockFundingClaimDataService.Setup(service => service.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(fundingClaim.Id))
                 .Returns(Task.FromResult(fundingClaim));

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();
            var fundingClaimController = new FundingClaimController(null, null, mockFundingClaimDataService.Object, mockLogger.Object);


            //act
            var response = await fundingClaimController.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(fundingClaim.Id);

            //assert
            response.Should().BeEquivalentTo(fundingClaim);
            mockLogger.Verify(l => l.LogInformation($"Funding claim with Id [{response.Id}] returned from GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId using currentFundingClaimId {response.Id}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_CreatesLogAndReturnsNullResponse()
        {
            //arrange
            var mockFundingClaimDataService = new Mock<IFundingClaimDataService>();
            mockFundingClaimDataService.Setup(service => service.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(1))
                 .Returns(Task.FromResult((Services.Models.FundingClaim)null));

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimController>>();
            var fundingClaimController = new FundingClaimController(null, null, mockFundingClaimDataService.Object, mockLogger.Object);


            //act
            var response = await fundingClaimController.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(1);

            //assert
            response.Should().BeEquivalentTo((Services.Models.FundingClaim)null);
            mockLogger.Verify(l => l.LogInformation($"Null returned from GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId using currentFundingClaimId 1"));
        }
        #endregion
    }
}