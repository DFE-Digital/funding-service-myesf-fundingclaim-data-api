using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Core.Logging;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Repositories.Enums.Support;
using Pds.FundingClaim.Repositories.Interfaces;
using Pds.FundingClaim.Services.Constants;
using Pds.FundingClaim.Services.Implementations;
using Pds.FundingClaim.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuditModels = Pds.Audit.Api.Client.Models;
using DomainFundingClaim = Pds.FundingClaim.Repositories.DataModels.FundingClaim;
using SchemaFundingClaim = Pds.FundingClaim.CorporateSchema.FundingClaims.FundingClaim;

namespace Pds.FundingClaim.Services.Tests.Unit
{
    [TestClass]
    public class FundingClaimDataServiceTests
    {
        private const string ClaimTypeMidYear = "MIDYEAR";
        private const string ClaimTypeYearEnd = "YEAREND";
        private const string ClaimTypeFinal = "FINAL";
        private const string DataCollectionKey1920Final = "1920-Final";
        private Mock<IAuditService> mockAuditService;

        #region AutoWithdrawFundingClaims

        [TestMethod, TestCategory("Unit")]
        public async Task AutoWithdrawFundingClaims_WhenCalled_AutoWithdrawsEligibleFundingClaims()
        {
            //arrange
            var now = new DateTime(2000, 12, 12, 10, 10, 10);

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();
            var lastFundingClaimWindow = new FundingClaimWindow { Id = 2, SignatureCloseDate = now.AddDays(-5) };
            mockFundingClaimWindowRepository.Setup(repo => repo.GetLastWindow(now))
                                            .Returns(lastFundingClaimWindow);

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();

            MockAuditService();

            var fundingClaimOne = new DomainFundingClaim
            {
                Id = 1,
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToSign
            };

            var fundingClaimTwo = new DomainFundingClaim
            {
                Id = 2,
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToSign
            };

            mockFundingClaimRepository.Setup(repo => repo.GetFundingClaimsToBeAutoWithdrawnForWindow(lastFundingClaimWindow.Id))
                                      .Returns(new List<DomainFundingClaim> { fundingClaimOne, fundingClaimTwo });

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                .Returns(now);

            var mockEmailService = new Mock<IEmailService>();

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            var fundingClaimDataService = new FundingClaimDataService(
                null,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            await fundingClaimDataService.AutoWithdrawFundingClaims();

            //assert
            mockFundingClaimRepository.Verify(
                repo => repo.Update(It.Is<DomainFundingClaim>(
                fc =>
                fc.Id == 1 &&
                fc.Status == FundingClaimState.AutoWithdrawn &&
                fc.LastUpdatedAt == now)), Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.Update(It.Is<DomainFundingClaim>(
                fc =>
                fc.Id == 2 &&
                fc.Status == FundingClaimState.AutoWithdrawn &&
                fc.LastUpdatedAt == now)), Times.Once);

            mockAuditService.Verify(
                repo => repo.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(2));

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimWithdrawnEmail(It.Is<List<int>>(
                list =>
                list[0] == 1 &&
                list[1] == 2 &&
                list.Count == 2)), Times.Once);
        }

        #endregion


        #region CreateFundingClaims

        [TestMethod, TestCategory("Unit")]
        public async Task CreateFundingClaims_WhenCalled_CreatesNonExistingClaimsAndIgnoresDuplicates()
        {
            //arrange
            var resultFromRepository = new Setting { Value = "Test Result" };
            var mockSettingRepository = new Mock<IRepository<Setting>>();
            mockSettingRepository.Setup(method => method.FirstOrDefault(setting => setting.Type == ServiceConstants.FundingClaimLastRetrievedSetting))
                .ReturnsAsync(resultFromRepository);

            var now = new DateTime(2016, 12, 01, 23, 59, 0);
            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var lastFundingClaimWindow = CreateFundingClaimWindow(now, DataCollectionKey1920Final);

            mockFundingClaimWindowRepository.Setup(
                method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                    bool>>>()))
                                            .ReturnsAsync(lastFundingClaimWindow);

            var existingFundingClaim1 = new DomainFundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "1617-MidYear_12345678_1",
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToReview
            };

            var existingFundingClaim2 = new DomainFundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "1617-MidYear_12333333_2",
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToReview
            };

            var existingFundingClaim3 = new DomainFundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "1617_MidYear_12344444_3",
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToReview
            };

            var existingFundingClaim4 = new DomainFundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "1617_MidYear_12344444_4",
                FundingClaimWindow = lastFundingClaimWindow,
                Status = FundingClaimState.ReadyToReview
            };

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>(MockBehavior.Strict);

            mockFundingClaimRepository.Setup(
                repo => repo.GetFundingClaimsForLastWindow(lastFundingClaimWindow.Id))
                                      .Returns(new List<DomainFundingClaim>
                                      {
                                          existingFundingClaim1,
                                          existingFundingClaim2
                                      });

            var schemaFundingClaim1 = new SchemaFundingClaim
            {
                FundingClaimId = "1617-MidYear_12345678_1",
                ClaimTypeName = ClaimTypeMidYear,
                Ukprn = "12345678"
            };

            var schemaFundingClaim2 = new SchemaFundingClaim
            {
                FundingClaimId = existingFundingClaim2.FundingClaimUniqueId,
                ClaimTypeName = ClaimTypeMidYear,
                Ukprn = "12345679"
            };

            var schemaFundingClaim3 = new SchemaFundingClaim
            {
                FundingClaimId = existingFundingClaim3.FundingClaimUniqueId,
                ClaimTypeName = ClaimTypeMidYear,
                Ukprn = "12345670",
            };

            var schemaFundingClaim4 = new SchemaFundingClaim
            {
                FundingClaimId = existingFundingClaim4.FundingClaimUniqueId,
                ClaimTypeName = ClaimTypeFinal,
                Ukprn = "12345671"
            };

            var schemaFundingClaimList = new List<SchemaFundingClaim>
            {
                schemaFundingClaim1,
                schemaFundingClaim2,
                schemaFundingClaim3,
                schemaFundingClaim4
            };

            var createdFundingClaim1 = new DomainFundingClaim()
            {
                Id = existingFundingClaim2.Id + 1,
                FundingClaimUniqueId = schemaFundingClaim1.FundingClaimId,
                FundingClaimWindow = lastFundingClaimWindow,
                Type = schemaFundingClaim3.ClaimTypeName.ToFundingClaimType(),
                Ukprn = int.Parse(schemaFundingClaim3.Ukprn)
            };

            var createdFundingClaim2 = new DomainFundingClaim()
            {
                Id = existingFundingClaim2.Id + 2,
                FundingClaimUniqueId = schemaFundingClaim2.FundingClaimId,
                FundingClaimWindow = lastFundingClaimWindow,
                Type = schemaFundingClaim4.ClaimTypeName.ToFundingClaimType(),
                Ukprn = int.Parse(schemaFundingClaim4.Ukprn)
            };

            var createdFundingClaim3 = new DomainFundingClaim()
            {
                Id = existingFundingClaim2.Id + 2,
                FundingClaimUniqueId = schemaFundingClaim3.FundingClaimId,
                FundingClaimWindow = lastFundingClaimWindow,
                Type = schemaFundingClaim4.ClaimTypeName.ToFundingClaimType(),
                Ukprn = int.Parse(schemaFundingClaim4.Ukprn)
            };

            var createdFundingClaim4 = new DomainFundingClaim()
            {
                Id = existingFundingClaim2.Id + 2,
                FundingClaimUniqueId = schemaFundingClaim4.FundingClaimId,
                FundingClaimWindow = lastFundingClaimWindow,
                Type = schemaFundingClaim4.ClaimTypeName.ToFundingClaimType(),
                Ukprn = int.Parse(schemaFundingClaim4.Ukprn)
            };

            mockFundingClaimRepository.Setup(
                repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim3))
                                      .ReturnsAsync(createdFundingClaim1);

            mockFundingClaimRepository.Setup(
                repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim4))
                                      .ReturnsAsync(createdFundingClaim2);

            var result = new List<DomainFundingClaim>();

            mockFundingClaimRepository.Setup(
                    method => method.Where(It.IsAny<Expression<Func<DomainFundingClaim,
                        bool>>>()))
                .Returns(result);

            MockAuditService();

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(now);

            var mockEmailService = new Mock<IEmailService>();

            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            SetExpectationsForGetFundingClaimForSpecifiedIdentifier(mockFundingClaimRepository, createdFundingClaim1);

            SetExpectationsForGetFundingClaimForSpecifiedIdentifier(mockFundingClaimRepository, createdFundingClaim2);

            SetExpectationsForGetFundingClaimForSpecifiedIdentifier(mockFundingClaimRepository, createdFundingClaim3);

            SetExpectationsForGetFundingClaimForSpecifiedIdentifier(mockFundingClaimRepository, createdFundingClaim4);

            mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimForSpecifiedIdentifier(createdFundingClaim4.FundingClaimUniqueId))
                .Returns(new List<DomainFundingClaim>()
                {
                });

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            await fundingClaimDataService.CreateFundingClaims(schemaFundingClaimList, lastFundingClaimWindow.Id);

            //assert
            mockFundingClaimWindowRepository.Verify(
                repo => repo.Update(
                    It.Is<FundingClaimWindow>(fw => fw.LastUpdatedAt == now && fw.FundingClaimsRetrieved)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fcw => fcw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(sfc => sfc == schemaFundingClaim1)),
                Times.Never);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fcw => fcw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(sfc => sfc == schemaFundingClaim2)),
                Times.Never);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fcw => fcw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(sfc => sfc == schemaFundingClaim3)),
                Times.Never);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fcw => fcw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(sfc => sfc == schemaFundingClaim4)),
                Times.Once);

            mockAuditService.Verify(
               repo => repo.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(1));

            mockSettingRepository.Verify(
                repo => repo.Update(
                    It.Is<Setting>(fw => fw.Value == now.ToString())),
                Times.Once);

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToViewEmail(It.Is<List<int>>(
                list =>
                list[0] == 3 &&
                list.Count == 1)), Times.Never);

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToSignEmail(It.Is<List<int>>(
                list =>
                list[0] == 4 &&
                list.Count == 1)), Times.Once);

            VerifyWindowDetails(mockLogger, lastFundingClaimWindow);

            mockLogger.Verify(
                l => l.LogInformation(
                    $"Funding Claim for Ukprn {createdFundingClaim1.Ukprn}, Title: {createdFundingClaim1.Title} has not changed. FundingClaimId: {createdFundingClaim1.FundingClaimUniqueId}"));

            mockLogger.Verify(
                l => l.LogInformation(
                    $"Funding Claim for Ukprn {createdFundingClaim2.Ukprn}, Title: {createdFundingClaim2.Title} has not changed. FundingClaimId: {createdFundingClaim2.FundingClaimUniqueId}"));

            mockLogger.Verify(
                l => l.LogInformation(
                    $"Funding Claim for Ukprn {createdFundingClaim3.Ukprn}, Title: {createdFundingClaim3.Title} has not changed. FundingClaimId: {createdFundingClaim3.FundingClaimUniqueId}"));
        }

        [TestMethod, TestCategory("Unit")]
        public async Task CreateFundingClaims_WhenCalledOnFinalFundingClaimsOnly_SendsReadyToSignEmailsOnly()
        {
            //arrange
            var resultFromRepository = new Setting { Value = "Test Result" };
            var mockSettingRepository = new Mock<IRepository<Setting>>();

            mockSettingRepository.Setup(method => method.FirstOrDefault(setting => setting.Type == ServiceConstants.FundingClaimLastRetrievedSetting))
                .ReturnsAsync(resultFromRepository);

            var now = new DateTime(2016, 12, 01, 23, 59, 0);
            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var lastFundingClaimWindow = CreateFundingClaimWindow(now, DataCollectionKey1920Final);

            mockFundingClaimWindowRepository.Setup(method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                                                bool>>>()))
                                            .ReturnsAsync(lastFundingClaimWindow);

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>(MockBehavior.Strict);

            mockFundingClaimRepository.Setup(repo => repo.GetFundingClaimsForLastWindow(lastFundingClaimWindow.Id))
                                      .Returns(new List<DomainFundingClaim>());

            var mockFundingClaim = mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimForSpecifiedIdentifier(It.IsAny<string>()))
                .Returns(new List<DomainFundingClaim>());

            var schemaFundingClaim1 = new SchemaFundingClaim
            {
                FundingClaimId = "1617_Final_12344444_1",
                ClaimTypeName = ClaimTypeFinal,
                Ukprn = "12345678"
            };

            var schemaFundingClaim2 = new SchemaFundingClaim
            {
                FundingClaimId = "1617_Final_12345555_1",
                ClaimTypeName = ClaimTypeFinal,
                Ukprn = "12345679"
            };

            var schemaFundingClaimList = new List<SchemaFundingClaim>
            {
                schemaFundingClaim1,
                schemaFundingClaim2
            };

            mockFundingClaimRepository.Setup(repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim1))
                .ReturnsAsync(new DomainFundingClaim()
                {
                    Id = 1,
                    FundingClaimUniqueId = schemaFundingClaim1.FundingClaimId,
                    Type = schemaFundingClaim1.ClaimTypeName.ToFundingClaimType(),
                    Ukprn = int.Parse(schemaFundingClaim1.Ukprn)
                });

            mockFundingClaimRepository.Setup(repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim2))
                .ReturnsAsync(new DomainFundingClaim()
                {
                    Id = 2,
                    FundingClaimUniqueId = schemaFundingClaim2.FundingClaimId,
                    Type = schemaFundingClaim2.ClaimTypeName.ToFundingClaimType(),
                    Ukprn = int.Parse(schemaFundingClaim2.Ukprn)
                });

            var result = new List<DomainFundingClaim>();

            mockFundingClaimRepository.Setup(
                    method => method.Where(It.IsAny<Expression<Func<DomainFundingClaim,
                        bool>>>()))
                .Returns(result);

            MockAuditService();

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                              .Returns(now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            await fundingClaimDataService.CreateFundingClaims(schemaFundingClaimList, lastFundingClaimWindow.Id);

            //assert
            mockFundingClaimWindowRepository.Verify(
                repo => repo.Update(
                    It.Is<FundingClaimWindow>(fw => fw.LastUpdatedAt == now && fw.FundingClaimsRetrieved)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fw => fw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(fd => fd == schemaFundingClaim1)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fw => fw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(fd => fd == schemaFundingClaim2)),
                Times.Once);

            mockAuditService.Verify(
               repo => repo.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(2));

            mockSettingRepository.Verify(
                repo => repo.Update(
                    It.Is<Setting>(fw => fw.Value == now.ToString())),
                Times.Once);

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToViewEmail(It.Is<List<int>>(
                    list =>
                    list.Count == 0)),
                Times.Never);

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToSignEmail(It.Is<List<int>>(
                    list =>
                    list[0] == 1 &&
                    list.Count == 2)),
                Times.Once);

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToSignEmail(It.Is<List<int>>(
                    list =>
                    list[1] == 2 &&
                    list.Count == 2)),
                Times.Once);

            VerifyWindowDetails(mockLogger, lastFundingClaimWindow);
        }

        [DataRow("1920-MidYear", ClaimTypeMidYear)]
        [DataRow("1920-YearEnd", ClaimTypeYearEnd)]
        [DataRow("1920-Final", ClaimTypeFinal)]
        [TestMethod, TestCategory("Unit")]
        public async Task WhenCreateingFundingClaims_EnsureCorrectEmailTypeSent(string dataCollectionKey, string claimType)
        {
            //arrange
            var resultFromRepository = new Setting { Value = "Test Result" };
            var mockSettingRepository = new Mock<IRepository<Setting>>();
            mockSettingRepository.Setup(method => method.FirstOrDefault(setting => setting.Type == ServiceConstants.FundingClaimLastRetrievedSetting))
                .ReturnsAsync(resultFromRepository);

            var now = new DateTime(2016, 12, 01, 23, 59, 0);

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var lastFundingClaimWindow = CreateFundingClaimWindow(now, dataCollectionKey);

            mockFundingClaimWindowRepository.Setup(method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                                                bool>>>()))
                                            .ReturnsAsync(lastFundingClaimWindow);

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>(MockBehavior.Strict);
            mockFundingClaimRepository.Setup(repo => repo.GetFundingClaimsForLastWindow(lastFundingClaimWindow.Id))
                                      .Returns(new List<DomainFundingClaim>());

            var mockFundingClaim = mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimForSpecifiedIdentifier(It.IsAny<string>()))
                .Returns(new List<DomainFundingClaim>());

            var schemaFundingClaim1 = new SchemaFundingClaim
            {
                FundingClaimId = "1617_Final_12344444_1",
                ClaimTypeName = claimType,
                Ukprn = "12345678"
            };

            var schemaFundingClaim2 = new SchemaFundingClaim
            {
                FundingClaimId = "1617_Final_12345555_1",
                ClaimTypeName = claimType,
                Ukprn = "12345679"
            };

            var schemaFundingClaimList = new List<SchemaFundingClaim>
            {
                schemaFundingClaim1,
                schemaFundingClaim2
            };

            mockFundingClaimRepository.Setup(repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim1))
                .ReturnsAsync(new DomainFundingClaim()
                {
                    Id = 1,
                    FundingClaimUniqueId = schemaFundingClaim1.FundingClaimId,
                    Type = schemaFundingClaim1.ClaimTypeName.ToFundingClaimType(),
                    Ukprn = int.Parse(schemaFundingClaim1.Ukprn)
                });

            mockFundingClaimRepository.Setup(repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim2))
                .ReturnsAsync(new DomainFundingClaim()
                {
                    Id = 2,
                    FundingClaimUniqueId = schemaFundingClaim2.FundingClaimId,
                    Type = schemaFundingClaim2.ClaimTypeName.ToFundingClaimType(),
                    Ukprn = int.Parse(schemaFundingClaim2.Ukprn)
                });

            var result = new List<DomainFundingClaim>();

            mockFundingClaimRepository.Setup(
                    method => method.Where(It.IsAny<Expression<Func<DomainFundingClaim,
                        bool>>>()))
                .Returns(result);

            MockAuditService();

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(method => method.UtcNow())
                              .Returns(now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            await fundingClaimDataService.CreateFundingClaims(schemaFundingClaimList, lastFundingClaimWindow.Id);

            //assert
            mockFundingClaimWindowRepository.Verify(
                repo => repo.Update(
                    It.Is<FundingClaimWindow>(fw => fw.LastUpdatedAt == now && fw.FundingClaimsRetrieved)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fw => fw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(fd => fd == schemaFundingClaim1)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fw => fw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(fd => fd == schemaFundingClaim2)),
                Times.Once);

            mockAuditService.Verify(
               repo => repo.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(2));

            mockSettingRepository.Verify(
                repo => repo.Update(
                    It.Is<Setting>(fw => fw.Value == now.ToString())),
                Times.Once);

            if (dataCollectionKey.Contains("Final"))
            {
                mockEmailService.Verify(
                    emailService => emailService.SendFundingClaimReadyToSignEmail(It.Is<List<int>>(
                        list =>
                            list.Count == 2)),
                    Times.Once);
            }
            else
            {
                mockEmailService.Verify(
                    emailService => emailService.SendFundingClaimReadyToViewEmail(It.Is<List<int>>(
                        list =>
                            list[0] == 1 &&
                            list.Count == 2)),
                    Times.Never);
            }

            VerifyWindowDetails(mockLogger, lastFundingClaimWindow);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task CreateFundingClaims_WhenCalledOnFinalClaimVersion2_MarksVersion1ReplacedInRepositoryAndCreatesLog()
        {
            //arrange
            var resultFromRepository = new Setting { Value = "Test Result" };
            var mockSettingRepository = new Mock<IRepository<Setting>>();
            mockSettingRepository.Setup(method => method.FirstOrDefault(setting => setting.Type == ServiceConstants.FundingClaimLastRetrievedSetting))
                .ReturnsAsync(resultFromRepository);

            var now = new DateTime(2016, 12, 01, 23, 59, 0);

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var lastFundingClaimWindow = CreateFundingClaimWindow(now, DataCollectionKey1920Final);

            mockFundingClaimWindowRepository.Setup(
                method => method.FirstOrDefault(It.IsAny<Expression<Func<FundingClaimWindow,
                    bool>>>()))
                                            .ReturnsAsync(lastFundingClaimWindow);

            var existingFundingClaim = new DomainFundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "1617_Final_12345678_1",
                FundingClaimWindow = lastFundingClaimWindow,
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToSign,
                Version = 1
            };

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();

            mockFundingClaimRepository.Setup(
                repo => repo.GetFundingClaimsForLastWindow(lastFundingClaimWindow.Id))
                                      .Returns(new List<DomainFundingClaim>
                                      {
                                          existingFundingClaim
                                      });

            var mockFundingClaim = mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimForSpecifiedIdentifier(It.IsAny<string>()))
                .Returns(new List<DomainFundingClaim>());

            var schemaFundingClaim = new SchemaFundingClaim
            {
                FundingClaimId = "1617_Final_12345678_2",
                Ukprn = existingFundingClaim.Ukprn.ToString(),
                ClaimTypeName = ClaimTypeFinal,
                VersionNumber = existingFundingClaim.Version + 1
            };

            var schemaFundingClaimList = new List<SchemaFundingClaim>
            {
                schemaFundingClaim
            };

            var createdFundingClaim = new DomainFundingClaim()
            {
                Id = existingFundingClaim.Id + 1,
                FundingClaimUniqueId = schemaFundingClaim.FundingClaimId,
                FundingClaimWindow = lastFundingClaimWindow,
                Ukprn = int.Parse(schemaFundingClaim.Ukprn),
                Status = FundingClaimState.ReadyToSign,
                Type = schemaFundingClaim.ClaimTypeName.ToFundingClaimType(),
                Version = schemaFundingClaim.VersionNumber
            };

            mockFundingClaimRepository.Setup(
                repo => repo.CreateFundingClaim(lastFundingClaimWindow, schemaFundingClaim))
                                      .ReturnsAsync(createdFundingClaim);

            var result = new List<DomainFundingClaim>()
            {
                existingFundingClaim
            };

            mockFundingClaimRepository.Setup(
                    method => method.Where(It.IsAny<Expression<Func<DomainFundingClaim,
                        bool>>>()))
                .Returns(result);

            MockAuditService();

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            await fundingClaimDataService.CreateFundingClaims(schemaFundingClaimList, lastFundingClaimWindow.Id);

            //assert
            mockFundingClaimWindowRepository.Verify(
                repo => repo.Update(
                    It.Is<FundingClaimWindow>(fw => fw.LastUpdatedAt == now && fw.FundingClaimsRetrieved)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.CreateFundingClaim(
                    It.Is<FundingClaimWindow>(fcw => fcw.Id == lastFundingClaimWindow.Id),
                    It.Is<SchemaFundingClaim>(sfc => sfc == schemaFundingClaim)),
                Times.Once);

            mockFundingClaimRepository.Verify(
                repo => repo.Update(
                    It.Is<DomainFundingClaim>(fc => fc.Id == 1 &&
                                                        fc.LastUpdatedAt == now &&
                                                        fc.Status == FundingClaimState.Replaced)),
                Times.Once);

            mockAuditService.Verify(
              repo => repo.AuditAsync(It.IsAny<AuditModels.Audit>()), Times.Exactly(2));

            mockEmailService.Verify(
                emailService => emailService.SendFundingClaimReadyToSignEmail(It.Is<List<int>>(
                list =>
                list[0] == 2 &&
                list.Count == 1)), Times.Once);

            VerifyWindowDetails(mockLogger, lastFundingClaimWindow);

            mockLogger.Verify(
                l => l.LogInformation(
                    "Funding Claim {existingFundingClaim.Title} has not changed.", existingFundingClaim.Title),
                Times.Never);
        }

        #endregion


        #region GetFundingClaim

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimById_WhenCalledWithValidFundingClaimId_ReturnsFundingClaimByRequestedId()
        {
            //arrange
            var expectedFundingClaim = new DomainFundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "1617_Final_12345678_1",
                FundingClaimWindow =
                new FundingClaimWindow
                {
                    Id = 1,
                    DataCollectionKey = "1617-Final",
                    SignatureCloseDate = DateTime.Now
                },
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToSign,
                Version = 1
            };

            var mockSettingRepository = new Mock<IRepository<Setting>>();

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();
            mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimById(expectedFundingClaim.Id))
                .Returns(Task.FromResult(expectedFundingClaim));

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(DateTime.Now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            MockAuditService();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            var result = await fundingClaimDataService.GetFundingClaimById(expectedFundingClaim.Id);

            //assert
            Assert.AreEqual(expectedFundingClaim.Id, result.Id);
            Assert.AreEqual(expectedFundingClaim.Ukprn, result.Ukprn);
            Assert.AreEqual(expectedFundingClaim.Status, result.Status);
            Assert.AreEqual(expectedFundingClaim.Version, result.Version);
            Assert.AreEqual(expectedFundingClaim.FundingClaimUniqueId, result.FundingClaimUniqueId);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.Id, result.FundingClaimWindow.Id);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.DataCollectionKey, result.FundingClaimWindow.DataCollectionKey);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.SignatureCloseDate, result.FundingClaimWindow.SignatureCloseDate);

            mockFundingClaimRepository.Verify(
                repo => repo.GetFundingClaimById(expectedFundingClaim.Id),
                Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFundingClaimById_WhenCalledWithInValidFundingClaimId_ReturnsNull()
        {
            //arrange
            var mockSettingRepository = new Mock<IRepository<Setting>>();

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();
            mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimById(123))
                .Returns(Task.FromResult<DomainFundingClaim>(null));

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(DateTime.Now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            MockAuditService();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            var result = await fundingClaimDataService.GetFundingClaimById(123);

            //assert
            Assert.AreEqual(null, result);

            mockFundingClaimRepository.Verify(
                repo => repo.GetFundingClaimById(123),
                Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_WhenCalledWithValidFundingClaimId_ReturnsPreviouslySignedFundingClaim()
        {
            //arrange
            var expectedFundingClaim = new DomainFundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "1617_Final_12345678_1",
                FundingClaimWindow =
                new FundingClaimWindow
                {
                    Id = 1,
                    DataCollectionKey = "1617-Final",
                    SignatureCloseDate = DateTime.Now
                },
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToSign,
                Version = 1
            };

            var mockSettingRepository = new Mock<IRepository<Setting>>();

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();
            mockFundingClaimRepository
                .Setup(repo => repo.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(expectedFundingClaim.Id))
                .Returns(Task.FromResult(expectedFundingClaim));

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(DateTime.Now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            MockAuditService();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            var result = await fundingClaimDataService.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(expectedFundingClaim.Id);

            //assert
            Assert.AreEqual(expectedFundingClaim.Id, result.Id);
            Assert.AreEqual(expectedFundingClaim.Ukprn, result.Ukprn);
            Assert.AreEqual(expectedFundingClaim.Status, result.Status);
            Assert.AreEqual(expectedFundingClaim.Version, result.Version);
            Assert.AreEqual(expectedFundingClaim.FundingClaimUniqueId, result.FundingClaimUniqueId);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.Id, result.FundingClaimWindow.Id);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.DataCollectionKey, result.FundingClaimWindow.DataCollectionKey);
            Assert.AreEqual(expectedFundingClaim.FundingClaimWindow.SignatureCloseDate, result.FundingClaimWindow.SignatureCloseDate);

            mockFundingClaimRepository.Verify(
                repo => repo.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(expectedFundingClaim.Id),
                Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_WhenCalledWithInValidFundingClaimId_ReturnsNull()
        {
            //arrange
            var mockSettingRepository = new Mock<IRepository<Setting>>();

            var mockFundingClaimWindowRepository = new Mock<IFundingClaimWindowRepository>();

            var mockFundingClaimRepository = new Mock<IFundingClaimRepository>();
            mockFundingClaimRepository
                .Setup(repo => repo.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(123))
                .Returns(Task.FromResult<DomainFundingClaim>(null));

            var mockSystemProvider = new Mock<ISystemProvider>();
            mockSystemProvider.Setup(
                method => method.UtcNow())
                              .Returns(DateTime.Now);

            var mockEmailService = new Mock<IEmailService>();
            var mockLogger = new Mock<ILoggerAdapter<FundingClaimDataService>>();

            MockAuditService();

            var fundingClaimDataService = new FundingClaimDataService(
                mockSettingRepository.Object,
                mockFundingClaimWindowRepository.Object,
                mockFundingClaimRepository.Object,
                mockSystemProvider.Object,
                mockEmailService.Object,
                mockLogger.Object,
                mockAuditService.Object);

            //act
            var result = await fundingClaimDataService.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(123);

            //assert
            Assert.AreEqual(null, result);

            mockFundingClaimRepository.Verify(
                repo => repo.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(123),
                Times.Once);
        }

        #endregion GetFundingClaim

        private void MockAuditService()
        {
            mockAuditService = new Mock<IAuditService>();

            mockAuditService
               .Setup(e => e.AuditAsync(It.IsAny<AuditModels.Audit>()))
               .Returns(Task.CompletedTask);
        }

        private void VerifyWindowDetails(Mock<ILoggerAdapter<FundingClaimDataService>> logger, FundingClaimWindow fundingClaimWindow)
        {
            logger.Verify(
                l => l.LogInformation(
                    "In a Funding Claim window {DataCollectionKey} {SubmissionOpenDate} - {SubmissionCloseDate}",
                    fundingClaimWindow.DataCollectionKey,
                    fundingClaimWindow.SubmissionOpenDate,
                    fundingClaimWindow.SubmissionCloseDate),
                Times.Once);
        }

        private void SetExpectationsForGetFundingClaimForSpecifiedIdentifier(Mock<IFundingClaimRepository> mockFundingClaimRepository, DomainFundingClaim fundingClaim)
        {
            mockFundingClaimRepository
                .Setup(repo => repo.GetFundingClaimForSpecifiedIdentifier(fundingClaim.FundingClaimUniqueId))
                .Returns(new List<DomainFundingClaim>()
                {
                    fundingClaim
                });
        }

        private FundingClaimWindow CreateFundingClaimWindow(DateTime curreDateTime, string dataCollectionKey)
        {
            return new FundingClaimWindow
            {
                Id = 1,
                SubmissionOpenDate = curreDateTime.AddDays(-10),
                SignatureCloseDate = curreDateTime.AddDays(-5),
                DataCollectionKey = DataCollectionKey1920Final
            };
        }
    }
}