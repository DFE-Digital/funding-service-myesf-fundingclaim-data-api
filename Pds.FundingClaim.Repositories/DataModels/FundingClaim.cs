using Pds.FundingClaim.Repositories.DataModels.Support;
using Pds.FundingClaim.Repositories.Enums;
using System;

namespace Pds.FundingClaim.Repositories.DataModels
{
    /// <summary>
    /// Normalised version of a DC funding claim.
    /// </summary>
    public partial class FundingClaim
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaim"/> class.
        /// Private Default Constructor.
        /// </summary>
        public FundingClaim()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaim"/> class.
        /// Full Constructor.
        /// </summary>
        /// <param name="fundingClaimWindow">The window to which the funding claim belongs.</param>
        /// <param name="data">The original data from DEDS.</param>
        /// <param name="fundingClaimUniqueId">The unique id which DEDS supplies that identifies the funding claim.</param>
        /// <param name="ukprn">The UKPRN for whom the funding claim belongs.</param>
        /// <param name="version">The version of this instance.</param>
        /// <param name="type">The type of this instance.</param>
        /// <param name="period">The period to which this instance belongs.</param>
        /// <param name="dateSubmitted">The time at which a funding claim has been submitted.</param>
        /// <param name="status">The status of the funding claim.</param>
        public FundingClaim(
            FundingClaimWindow fundingClaimWindow,
            FundingClaimData data,
            string fundingClaimUniqueId,
            int ukprn,
            int version,
            FundingClaimType type,
            string period,
            DateTime? dateSubmitted,
            FundingClaimState status)
        {
            FundingClaimWindow = fundingClaimWindow;
            FundingClaimData = data;
            FundingClaimUniqueId = fundingClaimUniqueId;
            FundingClaimWindowId = fundingClaimWindow.Id;
            Ukprn = ukprn;
            Version = version;
            Type = type;
            Period = period;
            DateSubmitted = dateSubmitted;
            Status = status;
            this.SetTitle();
            data?.SetFundingClaim(this);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets identifier for the funding claim.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets who the funding claim is for.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of a funding claim.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the version number of the funding claim.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the type of funding claim that this instance represents.
        /// </summary>
        public FundingClaimType Type { get; set; }

        /// <summary>
        /// Gets or sets the period for which this funding claim belongs.
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Gets or sets if a funding claim has been signed, then this will be populated by the name of the provider agent who signed it.
        /// </summary>
        public string SignedBy { get; set; }

        /// <summary>
        /// Gets or sets if a funding claim has been signed, then this will be populated by the display name of the provider agent who signed it.
        /// </summary>
        public string SignedByDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the contract was signed.
        /// </summary>
        public DateTime? SignedOn { get; set; }

        /// <summary>
        /// Gets or sets when this instance was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when this instance was last updated.
        /// </summary>
        public DateTime LastUpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets unique identifier of funding claim.
        /// </summary>
        public string FundingClaimUniqueId { get; set; }

        /// <summary>
        /// Gets or sets related funding claim window.
        /// </summary>
        public int? FundingClaimWindowId { get; set; }

        /// <summary>
        /// Gets or sets the Status of the funding claim.
        /// </summary>
        public FundingClaimState Status { get; set; }

        /// <summary>
        /// Gets or sets the time at which a funding claim has been submitted.
        /// </summary>
        public DateTime? DateSubmitted { get; set; }

        /// <summary>
        /// Gets or sets related funding claim window. In spite of the plural-sounding autogenerated
        /// model name, FundingClaimWindows refers to a single funding claim window object.
        /// </summary>
        public virtual FundingClaimWindow FundingClaimWindow { get; set; }

        /// <summary>
        /// Gets or sets related funding claim data. In spite of the plural-sounding autogenerated
        /// model name, FundingClaimDatas refers to a single funding claim object.
        /// </summary>
        public virtual FundingClaimData FundingClaimData { get; set; }

        #endregion
    }
}