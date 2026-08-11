using Pds.FundingClaim.CorporateSchema.FundingClaims;
using Pds.FundingClaim.Repositories.Interfaces;
using Pds.FundingClaim.Services.Constants;
using Pds.FundingClaim.Services.Extensions;
using Pds.FundingClaim.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainWindow = Pds.FundingClaim.Repositories.DataModels.FundingClaimWindow;
using ServicesWindow = Pds.FundingClaim.Services.Models.FundingClaimWindow;

namespace Pds.FundingClaim.Services.Implementations
{
    /// <inheritdoc cref="IFundingClaimWindowDataService"/>
    public class FundingClaimWindowDataService : IFundingClaimWindowDataService
    {
        #region Private Members

        private IFundingClaimWindowRepository _fundingClaimWindowRepository;
        private ISystemProvider _systemProvider;

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimWindowDataService"/> class.
        /// The parameterised constructor.
        /// </summary>
        /// <param name="fundingClaimWindowRepository">The funding claim window repository.</param>
        /// <param name="systemProvider">The System provider.</param>
        public FundingClaimWindowDataService(
            IFundingClaimWindowRepository fundingClaimWindowRepository,
            ISystemProvider systemProvider)
        {
            _fundingClaimWindowRepository = fundingClaimWindowRepository;
            _systemProvider = systemProvider;
        }

        #endregion


        #region Implementation

        /// <inheritdoc/>
        public async Task<ServicesWindow> GetFundingClaimCurrentWindow()
        {
            var now = _systemProvider.UtcNow();

            var result = await _fundingClaimWindowRepository.FirstOrDefault(window =>
                window.SubmissionOpenDate <= now
                && now <= window.SubmissionCloseDate.AddMinutes(ServiceConstants.ContigencyPeriodInMinutes));

            return result == null ? null : FundingClaimMappings.ToFundingClaimWindow(result);
        }

        /// <inheritdoc/>
        public async Task UpdateFundingClaimWindow(IEnumerable<FundingClaimDetails> fundingClaimDetails)
        {
            var now = _systemProvider.UtcNow();
            var allWindows = await _fundingClaimWindowRepository.GetAll();

            foreach (var fundingClaimDetail in fundingClaimDetails)
            {
                var matchedWindow = allWindows.SingleOrDefault(window =>
                    window.DataCollectionKey == fundingClaimDetail.DataCollectionKey);

                if (matchedWindow == null)
                {
                    var window = new DomainWindow
                    {
                        DataSetVersionId = _systemProvider.NewGuid(),
                        DataCollectionKey = fundingClaimDetail.DataCollectionKey,
                        SubmissionOpenDate = fundingClaimDetail.SubmissionOpenDateUtc.ToUtcDateTime(),
                        SubmissionCloseDate = fundingClaimDetail.SubmissionCloseDateUtc.ToUtcDateTime(),
                        SignatureCloseDate = fundingClaimDetail.SignatureCloseDateUtc?.ToUtcDateTime(),
                        RequiresSignature = fundingClaimDetail.RequiresSignature,
                        FundingClaimsRetrieved = false,
                        CreatedAt = now,
                        LastUpdatedAt = now
                    };
                    await _fundingClaimWindowRepository.Create(window);
                }
                else
                {
                    matchedWindow.SubmissionOpenDate = fundingClaimDetail.SubmissionOpenDateUtc.ToUtcDateTime();
                    matchedWindow.SubmissionCloseDate = fundingClaimDetail.SubmissionCloseDateUtc.ToUtcDateTime();
                    matchedWindow.SignatureCloseDate = fundingClaimDetail.SignatureCloseDateUtc?.ToUtcDateTime();
                    matchedWindow.RequiresSignature = fundingClaimDetail.RequiresSignature;
                    matchedWindow.LastUpdatedAt = now;
                    await _fundingClaimWindowRepository.Update(matchedWindow);
                }
            }
        }

        #endregion
    }
}