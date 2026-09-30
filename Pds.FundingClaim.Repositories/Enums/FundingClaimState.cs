using System;
using System.ComponentModel.DataAnnotations;

namespace Pds.FundingClaim.Repositories.Enums
{
    /// <summary>
    /// Defines the state that a funding claim can be in.
    /// </summary>
    public enum FundingClaimState
    {
        /// <summary>
        /// The funding claim is ready to be signed by the provider.
        /// </summary>
        [Obsolete("From 25-26 onwards, Final Funding Claims will not require signing.")]
        [Display(Name = "Ready to sign")]
        ReadyToSign = 0,

        /// <summary>
        /// The funding claim has been replaced by a newer version.
        /// </summary>
        [Display(Name = "Replaced")]
        Replaced = 1,

        /// <summary>
        /// The funding claim has been signed.
        /// </summary>
        [Display(Name = "Signed")]
        Signed = 2,

        /// <summary>
        /// The funding claim wasn't signed in the submission/signing window and has now been withdrawn.
        /// </summary>
        [Obsolete("From 25-26 onwards, funding claims will not be withdrawn.")]
        [Display(Name = "Withdrawn")]
        AutoWithdrawn = 3,

        /// <summary>
        /// The funding claim is a Ready to review (does not require signature)
        /// </summary>
        [Display(Name = "Ready to review")]
        ReadyToReview = 4
    }
}