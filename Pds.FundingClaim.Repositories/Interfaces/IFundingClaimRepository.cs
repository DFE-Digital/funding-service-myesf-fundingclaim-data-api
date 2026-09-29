using Pds.FundingClaim.Repositories.DataModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Repositories.Interfaces
{
    /// <summary>
    /// Repository to CRUD on FundingClaim table in database.
    /// </summary>
    public interface IFundingClaimRepository : IRepository<DataModels.FundingClaim>
    {
        /// <summary>
        /// Creates a <see cref="FundingClaim"/> associated with this instance.
        /// </summary>
        /// <param name="lastClaimWindow">The last funding claim window.</param>
        /// <param name="corporateFundingClaim">The corporate schema funding claim from DCT.</param>
        /// <returns>The new instance.</returns>
        Task<DataModels.FundingClaim> CreateFundingClaim(
            FundingClaimWindow lastClaimWindow, CorporateSchema.FundingClaims.FundingClaim corporateFundingClaim);

        /// <summary>
        /// Gets the funding claims for the last window.
        /// </summary>
        /// <param name="fundingClaimWindowId">The funding claim window identifier.</param>
        /// <returns>The list of funding claims.</returns>
        IEnumerable<DataModels.FundingClaim> GetFundingClaimsForLastWindow(int fundingClaimWindowId);

        /// <summary>
        /// Get funding claim for specified identifier.
        /// </summary>
        /// <param name="fundingClaimUniqueId">Funding claim identifier that needs to be retrieved.</param>
        /// <returns>A collection of funding claims.</returns>
        IEnumerable<DataModels.FundingClaim> GetFundingClaimForSpecifiedIdentifier(string fundingClaimUniqueId);

        /// <summary>
        /// Get funding claim for specified Id.
        /// </summary>
        /// <param name="fundingClaimId">Funding claim Id that needs to be retrieved.</param>
        /// <returns>Funding claim by matching Id.</returns>
        Task<DataModels.FundingClaim> GetFundingClaimById(int fundingClaimId);

        /// <summary>
        /// Get previously signed funding claim by current funding claim Id.
        /// </summary>
        /// <param name="currentFundingClaimId">Current funding claim Id.</param>
        /// <returns>Previously signed version of funding claim by current funding claim Id.</returns>
        Task<DataModels.FundingClaim> GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(int currentFundingClaimId);
    }
}