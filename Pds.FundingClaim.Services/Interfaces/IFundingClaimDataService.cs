using System.Collections.Generic;
using System.Threading.Tasks;
using SchemaFundingClaim = Pds.FundingClaim.CorporateSchema.FundingClaims.FundingClaim;

namespace Pds.FundingClaim.Services.Interfaces
{
    /// <summary>
    /// Interface for Funding Claim related data operations.
    /// </summary>
    public interface IFundingClaimDataService
    {
        #region Public Methods

        /// <summary>
        /// Processes Funding Claims if a window exists with the given Id.
        /// </summary>
        /// <param name="fundingClaims">List of funding claims to be added.</param>
        /// <param name="fundingClaimWindowId">The funding claim window ID to be inputted.</param>
        /// <returns>The completed Task.</returns>
        Task CreateFundingClaims(List<SchemaFundingClaim> fundingClaims, int fundingClaimWindowId);

        /// <summary>
        /// Get funding claim for specified Id.
        /// </summary>
        /// <param name="fundingClaimId">Funding claim Id that needs to be retrieved.</param>
        /// <returns>Funding claim by matching Id.</returns>
        Task<Models.FundingClaim> GetFundingClaimById(int fundingClaimId);


        /// <summary>
        /// Get previously signed funding claim by current funding claim Id.
        /// </summary>
        /// <param name="currentFundingClaimId">Current funding claim Id.</param>
        /// <returns>Previously signed version of funding claim by current funding claim Id.</returns>
        Task<Models.FundingClaim> GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(int currentFundingClaimId);

        #endregion
    }
}