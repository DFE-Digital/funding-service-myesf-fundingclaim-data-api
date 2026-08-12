using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FundingClaimModel = Pds.FundingClaim.Services.Models.FundingClaim;
using FundingClaimRepoModel = Pds.FundingClaim.Repositories.DataModels.FundingClaim;
using FundingClaimWindowModel = Pds.FundingClaim.Services.Models.FundingClaimWindow;
using FundingClaimWindowRepoModel = Pds.FundingClaim.Repositories.DataModels.FundingClaimWindow;


namespace Pds.FundingClaim.Services.Extensions
{
    public static class FundingClaimMappings
    {
        public static FundingClaimModel ToFundingClaim(this FundingClaimRepoModel fundingclaim)
        {
            return new FundingClaimModel
            {
                Ukprn = fundingclaim.Ukprn,
                Id = fundingclaim.Id,
                Title = fundingclaim.Title,
                Version = fundingclaim.Version,
                Type = fundingclaim.Type,
                Period = fundingclaim.Period,
                SignedBy = fundingclaim.SignedBy,
                SignedByDisplayName = fundingclaim.SignedByDisplayName,
                SignedOn = fundingclaim.SignedOn,
                CreatedAt = fundingclaim.CreatedAt,
                LastUpdatedAt = fundingclaim.LastUpdatedAt,
                FundingClaimUniqueId = fundingclaim.FundingClaimUniqueId,
                FundingClaimWindowId = fundingclaim.FundingClaimWindowId ?? 0,
                FundingClaimWindow = fundingclaim.FundingClaimWindow.ToFundingClaimWindow(),
                DateSubmitted = fundingclaim.DateSubmitted,
                Status = fundingclaim.Status
            };
        }

        public static FundingClaimWindowModel ToFundingClaimWindow(this FundingClaimWindowRepoModel fundingclaimwindow)
        {
            return new FundingClaimWindowModel
            {
                Id = fundingclaimwindow.Id,
                DataCollectionKey = fundingclaimwindow.DataCollectionKey,
                SubmissionOpenDate = fundingclaimwindow.SubmissionOpenDate,
                SubmissionCloseDate = fundingclaimwindow.SubmissionCloseDate,
                SignatureCloseDate = fundingclaimwindow.SignatureCloseDate,
                RequiresSignature = fundingclaimwindow.RequiresSignature,
                LastUpdatedAt = fundingclaimwindow.LastUpdatedAt
            };
        }
    }
}
