using Microsoft.EntityFrameworkCore;
using Pds.Core.Logging;
using Pds.FundingClaim.Repositories.Data;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Repositories.Enums.Support;
using Pds.FundingClaim.Repositories.Exceptions;
using Pds.FundingClaim.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Repositories.Implementation
{
    /// <inheritdoc cref="IFundingClaimRepository"/>
    public class FundingClaimRepository : Repository<DataModels.FundingClaim>, IFundingClaimRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimRepository"/> class.
        /// The parametrised constructor.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logging service.</param>
        public FundingClaimRepository(
            PdsContext context,
            ILoggerAdapter<Repository<DataModels.FundingClaim>> logger)
            : base(context, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<DataModels.FundingClaim> CreateFundingClaim(
            FundingClaimWindow lastClaimWindow,
            CorporateSchema.FundingClaims.FundingClaim corporateFundingClaim)
        {
            int ukprn;
            FundingClaimType type;

            try
            {
                ukprn = int.Parse(corporateFundingClaim.Ukprn);
            }
            catch
            {
                throw new UkprnWrongFormatException(corporateFundingClaim.Ukprn);
            }

            try
            {
                type = corporateFundingClaim.ClaimTypeName.ToFundingClaimType();
            }
            catch
            {
                throw new ClaimTypeNameInvalidException(corporateFundingClaim.ClaimTypeName);
            }

            var version = corporateFundingClaim.VersionNumber;
            var period = corporateFundingClaim.Period;
            var submissionDate = corporateFundingClaim.SubmissionDateTime;
            var fundingClaimState = type == FundingClaimType.FINAL && int.TryParse(period, out int val) && val < 2526 ?
                FundingClaimState.ReadyToSign : FundingClaimState.ReadyToReview;

            var fundingClaimData = new FundingClaimData(corporateFundingClaim);
            var fundingClaim = new DataModels.FundingClaim(
                lastClaimWindow,
                fundingClaimData,
                corporateFundingClaim.FundingClaimId,
                ukprn,
                version,
                type,
                period,
                submissionDate,
                fundingClaimState);

            fundingClaim.CreatedAt = fundingClaim.LastUpdatedAt = DateTime.UtcNow;

            await Create(fundingClaim);

            return fundingClaim;
        }

        /// <inheritdoc/>
        public IEnumerable<DataModels.FundingClaim> GetFundingClaimsToBeAutoWithdrawnForWindow(int fundingClaimWindowId)
        {
            return Where(fundingClaim =>
                                fundingClaim.FundingClaimWindow.Id == fundingClaimWindowId
                                && fundingClaim.Status == FundingClaimState.ReadyToSign)
                        .OrderBy(fundingClaim => fundingClaim.Version);
        }

        /// <inheritdoc/>
        public IEnumerable<DataModels.FundingClaim> GetFundingClaimsForLastWindow(int fundingClaimWindowId)
        {
            return Where(fundingClaim =>
                                fundingClaim.FundingClaimWindow.Id == fundingClaimWindowId)
                        .OrderBy(fundingClaim => fundingClaim.Version);
        }

        /// <inheritdoc/>
        public IEnumerable<DataModels.FundingClaim> GetFundingClaimForSpecifiedIdentifier(string fundingClaimUniqueId)
        {
            return Where(fundingClaim =>
                fundingClaim.FundingClaimUniqueId == fundingClaimUniqueId);
        }

        /// <inheritdoc/>
        public async Task<DataModels.FundingClaim> GetFundingClaimById(int fundingClaimId)
        {
            return await Get(x => x.Id == fundingClaimId, p => p.Include(c => c.FundingClaimWindow));
        }

        /// <inheritdoc/>
        public async Task<DataModels.FundingClaim> GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(int currentFundingClaimId)
        {
            var currentFundingClaim = await GetFundingClaimById(currentFundingClaimId);

            if (currentFundingClaim == null)
            {
                return null;
            }

            var previousSignedVersion = Where(fundingClaim =>
                    fundingClaim.Status == FundingClaimState.Signed
                                 && fundingClaim.Period == currentFundingClaim.Period
                                 && fundingClaim.Type == currentFundingClaim.Type
                                 && fundingClaim.Ukprn == currentFundingClaim.Ukprn
                                 && fundingClaim.Version < currentFundingClaim.Version);

            return previousSignedVersion.Any() ? previousSignedVersion.OrderByDescending(fundingClaim => fundingClaim.Version).FirstOrDefault() : null;
        }
    }
}