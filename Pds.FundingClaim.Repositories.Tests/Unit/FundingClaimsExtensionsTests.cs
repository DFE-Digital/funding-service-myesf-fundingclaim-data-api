using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.DataModels.Support;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Repositories.Enums.Support;
using Pds.FundingClaim.Repositories.Exceptions;
using System;
using SchemaFundingClaim = Pds.FundingClaim.CorporateSchema.FundingClaims.FundingClaim;

namespace Pds.FundingClaim.Repositories.Tests.Unit
{
    [TestClass]
    public class FundingClaimsExtensionsTests
    {
        [TestMethod, TestCategory("Unit")]
        public void SetTitle_WhenFundingClaimTypeIsNotYearEnd_FundingClaimHasExpectedTitle()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-MidYear_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "MIDYEAR",
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var fundingClaimData = new FundingClaimData(schemaFundingClaim);

            var fundingClaim = new DataModels.FundingClaim(
                window,
                fundingClaimData,
                schemaFundingClaim.FundingClaimId,
                int.Parse(schemaFundingClaim.Ukprn),
                schemaFundingClaim.VersionNumber,
                schemaFundingClaim.ClaimTypeName.ToFundingClaimType(),
                schemaFundingClaim.Period,
                schemaFundingClaim.SubmissionDateTime,
                FundingClaimState.ReadyToReview);

            //act
            fundingClaim.SetTitle();

            //assert
            fundingClaim.Title.Should().Be("Mid year (R06) funding claim for 2016 to 2017 version 1");
        }

        [TestMethod, TestCategory("Unit")]
        public void SetTitle_WhenFundingClaimTypeIsYearEndAndPeriodIs1819OrLater_FundingClaimHasExpectedTitle()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1819-YearEnd_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "YEAREND",
                Period = "1819",
                SubmissionDateTime = DateTime.Parse("2019-01-01T00:00:00")
            };

            var fundingClaimData = new FundingClaimData(schemaFundingClaim);

            var fundingClaim = new DataModels.FundingClaim(
                window,
                fundingClaimData,
                schemaFundingClaim.FundingClaimId,
                int.Parse(schemaFundingClaim.Ukprn),
                schemaFundingClaim.VersionNumber,
                schemaFundingClaim.ClaimTypeName.ToFundingClaimType(),
                schemaFundingClaim.Period,
                schemaFundingClaim.SubmissionDateTime,
                FundingClaimState.ReadyToReview);

            //act
            fundingClaim.SetTitle();

            //assert
            fundingClaim.Title.Should().Be("Year end (R10) funding claim for 2018 to 2019 version 1");
        }

        //This test will not pass except with ExpectedException.
        [TestMethod, TestCategory("Unit")]
        public void SetTitle_WhenFundingClaimPeriodWrongCharacterLength_ThrowsPeriodWrongLengthException()
        {
            var exception = Assert.ThrowsExactly<PeriodWrongLengthException>(() =>
            {
                //arrange
                var window = new FundingClaimWindow();

                var schemaFundingClaim = new SchemaFundingClaim()
                {
                    FundingClaimId = "1617-MidYear_12345678_1",
                    Ukprn = "12345678",
                    VersionNumber = 1,
                    ClaimTypeName = "MIDYEAR",
                    Period = "16178",
                    SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
                };

                var fundingClaimData = new FundingClaimData(schemaFundingClaim);

                var fundingClaim = new DataModels.FundingClaim(
                    window,
                    fundingClaimData,
                    schemaFundingClaim.FundingClaimId,
                    int.Parse(schemaFundingClaim.Ukprn),
                    schemaFundingClaim.VersionNumber,
                    schemaFundingClaim.ClaimTypeName.ToFundingClaimType(),
                    schemaFundingClaim.Period,
                    schemaFundingClaim.SubmissionDateTime,
                    FundingClaimState.ReadyToReview);

                //act
                fundingClaim.SetTitle();
            });

            Assert.AreEqual("The period 16178's length is not 4 characters.", exception.Message);
        }

        [TestMethod, TestCategory("Unit")]
        public void SetTitle_WhenFundingClaimPeriodInvalid_ThrowsPeriodWrongFormatException()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "20-1-MidYear_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                ClaimTypeName = "MIDYEAR",
                Period = "20-1",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var fundingClaimData = new FundingClaimData(schemaFundingClaim);

            var fundingClaim = new DataModels.FundingClaim(
                window,
                fundingClaimData,
                schemaFundingClaim.FundingClaimId,
                int.Parse(schemaFundingClaim.Ukprn),
                schemaFundingClaim.VersionNumber,
                schemaFundingClaim.ClaimTypeName.ToFundingClaimType(),
                "2021",
                schemaFundingClaim.SubmissionDateTime,
                FundingClaimState.ReadyToReview)
            {
                Period = "20-1"
            };

            //act & assert
            fundingClaim.Invoking(f => f.SetTitle())
                .Should().ThrowExactly<PeriodWrongFormatException>()
                .WithMessage("The period 20-1 is in the wrong format. It should have the last two numbers of the starting year followed by the last two numbers of the ending year. Example: '1920'.");
        }

        [TestMethod, TestCategory("Unit")]
        public void SetTitle_WhenFundingClaimTypeInvalid_ThrowsArgumentException()
        {
            //arrange
            var window = new FundingClaimWindow();

            var schemaFundingClaim = new SchemaFundingClaim()
            {
                FundingClaimId = "1617-MidYear_12345678_1",
                Ukprn = "12345678",
                VersionNumber = 1,
                Period = "1617",
                SubmissionDateTime = DateTime.Parse("2016-09-01T00:00:00")
            };

            var fundingClaimData = new FundingClaimData(schemaFundingClaim);

            var fundingClaim = new DataModels.FundingClaim(
                window,
                fundingClaimData,
                schemaFundingClaim.FundingClaimId,
                int.Parse(schemaFundingClaim.Ukprn),
                schemaFundingClaim.VersionNumber,
                FundingClaimType.MIDYEAR,
                schemaFundingClaim.Period,
                schemaFundingClaim.SubmissionDateTime,
                FundingClaimState.ReadyToReview)
            {
                Type = (FundingClaimType)3
            };

            //act & assert
            fundingClaim.Invoking(f => f.SetTitle())
                .Should().ThrowExactly<ArgumentException>()
                .WithMessage("error '3' not found in type 'FundingClaimType'");
        }
    }
}