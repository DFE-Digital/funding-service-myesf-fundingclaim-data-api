using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.FundingClaim.Repositories.Data;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Repositories.Enums.Support;
using Pds.FundingClaim.Repositories.Exceptions;
using Pds.FundingClaim.Repositories.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchemaFundingClaim = Pds.FundingClaim.CorporateSchema.FundingClaims.FundingClaim;

namespace Pds.FundingClaim.Repositories.Tests.Integration
{
    [TestClass]
    public class FundingClaimRepositoryTests
    {
        private readonly PdsContext _context;

        public FundingClaimRepositoryTests()
        {
            _context = new InMemoryDbContextFactory().GetPdsDbContext();
        }

        #region CreateFundingClaim

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenClaimTypeIsNotFinalAndNotInRepository_CreatesReadyToViewFundingClaimAndLog()
        {
            //arrange
            var window = new FundingClaimWindow();
            window.DataCollectionKey = "2000-Final";

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-MidYear_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "MIDYEAR",
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var expectedClaim = new DataModels.FundingClaim()
            {
                Id = 1,
                FundingClaimUniqueId = schemaFundingClaim.FundingClaimId,
                Type = schemaFundingClaim.ClaimTypeName.ToFundingClaimType()
            };

            var mockLogger = new Mock<ILoggerAdapter<Repository<DataModels.FundingClaim>>>(MockBehavior.Strict);
            mockLogger.Setup(
                mL => mL.LogInformation(
                    "User just created a {entityType} with id {createdEntityId}.", expectedClaim.GetType(), expectedClaim.Id)).Verifiable();

            var repository = new FundingClaimRepository(_context, mockLogger.Object);

            //act
            var fundingClaim = await repository.CreateFundingClaim(window, schemaFundingClaim);

            //assert
            _context.FundingClaims.Should().HaveCount(1);
            _context.FundingClaims.Should().ContainEquivalentOf(fundingClaim);
            _context.FundingClaims.First().Status.Should().Be(FundingClaimState.ReadyToReview);

            mockLogger.Verify(l => l.LogInformation(
                "User just created a {entityType} with id {createdEntityId}.",
                fundingClaim.GetType(),
                fundingClaim.GetType().GetProperty("Id").GetValue(fundingClaim)));
        }

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenClaimTypeIsFinalAndPeriodEarlierThan2526AndNotInRepository_CreatesReadyToSignFundingClaimAndLog()
        {
            //arrange
            var window = new FundingClaimWindow();
            window.DataCollectionKey = "2000-Final";

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-Final_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "FINAL",
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var expectedClaim = new DataModels.FundingClaim()
            {
                Id = 1,
                FundingClaimUniqueId = schemaFundingClaim.FundingClaimId,
                Type = schemaFundingClaim.ClaimTypeName.ToFundingClaimType()
            };

            var mockLogger = new Mock<ILoggerAdapter<Repository<DataModels.FundingClaim>>>(MockBehavior.Strict);
            mockLogger.Setup(
                mL => mL.LogInformation(
                    "User just created a {entityType} with id {createdEntityId}.", expectedClaim.GetType(), expectedClaim.Id)).Verifiable();

            var repository = new FundingClaimRepository(_context, mockLogger.Object);

            //act
            var fundingClaim = await repository.CreateFundingClaim(window, schemaFundingClaim);

            //assert
            _context.FundingClaims.Should().HaveCount(1);
            _context.FundingClaims.Should().Equal(fundingClaim);
            _context.FundingClaims.First().Status.Should().Be(FundingClaimState.ReadyToReview);

            mockLogger.Verify(l => l.LogInformation(
                "User just created a {entityType} with id {createdEntityId}.",
                fundingClaim.GetType(),
                fundingClaim.GetType().GetProperty("Id").GetValue(fundingClaim)));
        }

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenClaimTypeIsFinalAndPeriod2526OrLaterAndNotInRepository_CreatesReadyToSignFundingClaimAndLog()
        {
            //arrange
            var window = new FundingClaimWindow();
            window.DataCollectionKey = "2526-Final";

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "2526-Final_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "FINAL",
                Period = "2526",
                SubmissionDateTime = DateTime.Parse("2026-09-01T00:00:00")
            };

            var expectedClaim = new DataModels.FundingClaim()
            {
                Id = 1,
                FundingClaimUniqueId = schemaFundingClaim.FundingClaimId,
                Type = schemaFundingClaim.ClaimTypeName.ToFundingClaimType()
            };

            var mockLogger = new Mock<ILoggerAdapter<Repository<DataModels.FundingClaim>>>(MockBehavior.Strict);
            mockLogger.Setup(
                mL => mL.LogInformation(
                    "User just created a {entityType} with id {createdEntityId}.", expectedClaim.GetType(), expectedClaim.Id)).Verifiable();

            var repository = new FundingClaimRepository(_context, mockLogger.Object);

            //act
            var fundingClaim = await repository.CreateFundingClaim(window, schemaFundingClaim);

            //assert
            _context.FundingClaims.Should().HaveCount(1);
            _context.FundingClaims.Should().Equal(fundingClaim);
            _context.FundingClaims.First().Status.Should().Be(FundingClaimState.ReadyToReview);

            mockLogger.Verify(l => l.LogInformation(
                "User just created a {entityType} with id {createdEntityId}.",
                fundingClaim.GetType(),
                fundingClaim.GetType().GetProperty("Id").GetValue(fundingClaim)));
        }

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenCalledWithInvalidUKPRN_ThrowsException()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-Final_12345678_1",
                Ukprn = "abcdefgh12",
                VersionNumber = 1,
                ClaimTypeName = "FINAL",
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var repository = new FundingClaimRepository(_context, null);

            //act & assert
            await repository.Invoking(r =>
                r.CreateFundingClaim(window, schemaFundingClaim))
                    .Should().ThrowExactlyAsync<UkprnWrongFormatException>()
                    .WithMessage("The UKPRN abcdefgh12 contains non-numerical characters or is blank.");
        }

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenCalledWithInvalidClaimType_ThrowsException()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-Final_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "year",
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var repository = new FundingClaimRepository(_context, null);

            //act & assert
            await repository.Invoking(r =>
                r.CreateFundingClaim(window, schemaFundingClaim))
                    .Should().ThrowExactlyAsync<ClaimTypeNameInvalidException>()
                    .WithMessage("The ClaimTypeName year is invalid. Please use a ClaimTypeName that can be parsed to one of the FundingClaimTypes: MIDYEAR, YEAREND, FINAL");
        }

        [TestMethod, TestCategory("Integration")]
        public async Task CreateFundingClaim_WhenNoFundingClaimData_ThrowsException()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim();

            var repository = new FundingClaimRepository(_context, null);

            //act & assert
            await repository.Invoking(r =>
                r.CreateFundingClaim(window, schemaFundingClaim))
                    .Should().ThrowExactlyAsync<UkprnWrongFormatException>()
                    .WithMessage("The UKPRN  contains non-numerical characters or is blank.");
        }
        #endregion


        [TestMethod, TestCategory("Integration")]
        public void GetFundingClaimsForLastWindow_WhenCalled_GetsFundingClaimsForLastWindow()
        {
            //arrange
            var earlierFundingClaimWindow = new FundingClaimWindow { Id = 1, DataCollectionKey = "2425-Final" };
            var lastFundingClaimWindow = new FundingClaimWindow { Id = 2, DataCollectionKey = "2425-Final" };

            var fundingClaimInEarlierWindow1 = new DataModels.FundingClaim { FundingClaimWindow = earlierFundingClaimWindow, FundingClaimUniqueId = "1", Period = "2425", Title = "fundingClaimInEarlierWindow1" };
            var fundingClaimInEarlierWindow2 = new DataModels.FundingClaim { FundingClaimWindow = earlierFundingClaimWindow, FundingClaimUniqueId = "2", Period = "2425", Title = "fundingClaimInEarlierWindow2" };
            var fundingClaimInLastWindow1 = new DataModels.FundingClaim { FundingClaimWindow = lastFundingClaimWindow, Version = 1, FundingClaimUniqueId = "3", Period = "2425", Title = "fundingClaimInLastWindow1" };
            var fundingClaimInLastWindow2 = new DataModels.FundingClaim { FundingClaimWindow = lastFundingClaimWindow, Version = 2, FundingClaimUniqueId = "4", Period = "2425", Title = "fundingClaimInLastWindow2" };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaimInEarlierWindow1,
                fundingClaimInEarlierWindow2,
                fundingClaimInLastWindow1,
                fundingClaimInLastWindow2
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = repository.GetFundingClaimsForLastWindow(lastFundingClaimWindow.Id).ToList();

            //assert
            response.Should().HaveCount(2);
            response.Should().BeEquivalentTo(
                new List<DataModels.FundingClaim>
                {
                    fundingClaimInLastWindow1,
                    fundingClaimInLastWindow2
                },
                options => options.WithStrictOrdering());
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetFundingClaimById_WhenCalledWithMatchingId_GetsFundingClaimsByRequestedId()
        {
            //arrange
            var fundingClaim1 = new DataModels.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 1,
                Title = "test title"
            };

            var fundingClaim2 = new DataModels.FundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "2425_Final_12345678_2",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 2,
                        DataCollectionKey = "2425_Final_12345678_2-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 2,
                Title = "test title"
            };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaim1,
                fundingClaim2,
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = await repository.GetFundingClaimById(2);

            //assert
            response.Should().BeEquivalentTo(fundingClaim2);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetFundingClaimById_WhenCalledWithoutMatchingId_ReturnsNull()
        {
            //arrange
            var fundingClaim1 = new DataModels.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 1,
                Title = "test title"
            };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaim1
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = await repository.GetFundingClaimById(2);

            //assert
            response.Should().BeNull();
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_WhenCalledWithMatchingId_GetsPreviouslySignedFundingClaim()
        {
            //arrange
            var fundingClaim1 = new DataModels.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 1,
                Title = "Title 1"
            };

            var fundingClaim2 = new DataModels.FundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "2425_Final_12345678_2",
                FundingClaimWindow =
                new FundingClaimWindow
                {
                    Id = 2,
                    DataCollectionKey = "2425_Final_12345678_2-Final",
                    SignatureCloseDate = DateTime.Now
                },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.Signed,
                Version = 2,
                Title = "Title 2"
            };

            var fundingClaim3 = new DataModels.FundingClaim
            {
                Id = 3,
                FundingClaimUniqueId = "2425_Final_12345678_3",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 3,
                        DataCollectionKey = "2425_Final_12345678_3-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToReview,
                Version = 3,
                Title = "Title 3"
            };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaim1,
                fundingClaim2,
                fundingClaim3
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = await repository.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(3);

            //assert
            response.Should().BeEquivalentTo(fundingClaim2);
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_WhenCalledWithoutValidFundingClaim_ReturnsNull()
        {
            //arrange
            var fundingClaim1 = new DataModels.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToReview,
                Version = 1,
                Title = "Title 1"
            };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaim1
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = await repository.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(2);

            //assert
            response.Should().BeNull();
        }

        [TestMethod, TestCategory("Integration")]
        public async Task GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId_WhenCalledWithoutValidScenario_ReturnsNull()
        {
            //arrange
            var fundingClaim1 = new DataModels.FundingClaim
            {
                Id = 1,
                FundingClaimUniqueId = "2425_Final_12345678_1",
                FundingClaimWindow =
                    new FundingClaimWindow
                    {
                        Id = 1,
                        DataCollectionKey = "2425_Final_12345678_1-Final",
                        SignatureCloseDate = DateTime.Now
                    },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToReview,
                Version = 1,
                Title = "Title 1"
            };

            var fundingClaim2 = new DataModels.FundingClaim
            {
                Id = 2,
                FundingClaimUniqueId = "2425_Final_12345678_2",
                FundingClaimWindow =
                new FundingClaimWindow
                {
                    Id = 2,
                    DataCollectionKey = "2425_Final_12345678_2-Final",
                    SignatureCloseDate = DateTime.Now
                },
                Period = "2425",
                Type = FundingClaimType.FINAL,
                Ukprn = 12345678,
                Status = FundingClaimState.ReadyToReview,
                Version = 2,
                Title = "Title 2"
            };

            _context.FundingClaims.AddRange(new List<DataModels.FundingClaim>
            {
                fundingClaim1,
                fundingClaim2
            });
            _context.SaveChanges();

            var repository = new FundingClaimRepository(_context, null);

            //act
            var response = await repository.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(2);

            //assert
            response.Should().BeNull();
        }
    }
}