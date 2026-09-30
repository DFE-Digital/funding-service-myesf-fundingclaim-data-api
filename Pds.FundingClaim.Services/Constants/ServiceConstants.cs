namespace Pds.FundingClaim.Services.Constants
{
    /// <summary>
    /// Contains the constants used in service layer.
    /// </summary>
    public static class ServiceConstants
    {
        /// <summary>
        /// Setting Representing when the funding claim was last retrieved from DC.
        /// </summary>
        public const int FundingClaimLastRetrievedSetting = 38;

        /// <summary>
        /// Setting Representing if polling for funding claims is allowed or not.
        /// </summary>
        public const int FundingClaimPollingSetting = 9;

        /// <summary>
        /// This represents the contigency time period that is to be added to Submission close date so that we don't miss any funding claims in the period.
        /// </summary>
        public const int ContigencyPeriodInMinutes = 10;

        /// <summary>
        /// Setting Representing the ReconciliationFeedBookmarkId.
        /// </summary>
        public const int ReconciliationFeedBookmarkIdSetting = 7;

        /// <summary>
        /// Setting Representing the FeedReadWarningThreshold.
        /// </summary>
        public const int FeedReadWarningThreshold = 6;

        /// <summary>
        /// Setting Representing whether to use Json format or XML format of funding claims.
        /// </summary>
        public const int UseJsonFormatOfFundingClaims = 39;

        /// <summary>
        /// Setting Representing whether to use new or old reconciliations feed reader.
        /// </summary>
        public const int UseNewReconciliationsFeedReader = 40;

        /// <summary>
        /// Funding Claim Ready To View Email Queue.
        /// </summary>
        public const string FundingClaimReadyToViewEmailQueue = "fundingclaimreadytoviewemail";

        /// <summary>
        /// Reconciliation Ready To Be Viewed Email Queue.
        /// </summary>
        public const string ReconciliationReadyToBeViewedEmailQueue = "reconciliationreadytobeviewedemail";

        /// <summary>
        /// Feed Read Exception Email Queue.
        /// </summary>
        public const string FeedReadExceptionEmailQueue = "feedreadexception";

        /// <summary>
        /// Feed Read Threshold Exceeded Warning Email Queue.
        /// </summary>
        public const string FeedReadThresholdExceededWarningEmailQueue = "feedreadthresholdexceededwarning";
    }
}
