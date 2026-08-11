using Pds.Audit.Api.Client.Interfaces;
using Pds.Core.Logging;
using Pds.FundingClaim.Repositories.DataModels;
using Pds.FundingClaim.Repositories.Enums;
using Pds.FundingClaim.Repositories.Enums.Support;
using Pds.FundingClaim.Repositories.Interfaces;
using Pds.FundingClaim.Services.Constants;
using Pds.FundingClaim.Services.Extensions;
using Pds.FundingClaim.Services.Interfaces;
using Pds.FundingClaim.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainFundingClaim = Pds.FundingClaim.Repositories.DataModels.FundingClaim;
using DomainWindow = Pds.FundingClaim.Repositories.DataModels.FundingClaimWindow;
using SchemaFundingClaim = Pds.FundingClaim.CorporateSchema.FundingClaims.FundingClaim;

namespace Pds.FundingClaim.Services.Implementations
{
    /// <inheritdoc cref="IFundingClaimDataService"/>
    public class FundingClaimDataService : IFundingClaimDataService
    {
        #region Private Members

        private const string FinalFundingClaim = "Final";
        private readonly ILoggerAdapter<FundingClaimDataService> _logger;
        private readonly IAuditService _auditService;
        private IRepository<Setting> _settingsRepository;
        private IFundingClaimWindowRepository _fundingClaimWindowRepository;
        private IFundingClaimRepository _fundingClaimRepository;
        private ISystemProvider _systemProvider;
        private IEmailService _emailService;

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimDataService"/> class.
        /// The parameterised constructor.
        /// </summary>
        /// <param name="settingsRepository">The setting repository service.</param>
        /// <param name="fundingClaimWindowRepository">The funding claim window repository.</param>
        /// <param name="fundingClaimRepository">The funding claim repository.</param>
        /// <param name="systemProvider">The System provider.</param>
        /// <param name="emailService">The email service.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="auditService">The shared audit service.</param>
        public FundingClaimDataService(
            IRepository<Setting> settingsRepository,
            IFundingClaimWindowRepository fundingClaimWindowRepository,
            IFundingClaimRepository fundingClaimRepository,
            ISystemProvider systemProvider,
            IEmailService emailService,
            ILoggerAdapter<FundingClaimDataService> logger,
            IAuditService auditService)
        {
            _settingsRepository = settingsRepository;
            _fundingClaimWindowRepository = fundingClaimWindowRepository;
            _fundingClaimRepository = fundingClaimRepository;
            _systemProvider = systemProvider;
            _emailService = emailService;
            _logger = logger;
            _auditService = auditService;
        }

        #endregion


        #region Implementation

        /// <inheritdoc/>
        public async Task AutoWithdrawFundingClaims()
        {
            var now = _systemProvider.UtcNow();
            var lastWindow = _fundingClaimWindowRepository
                .GetLastWindow(now);

            if (lastWindow != null)
            {
                var fundingClaimsEligibleToBeAutoWithdrawn = _fundingClaimRepository
                    .GetFundingClaimsToBeAutoWithdrawnForWindow(lastWindow.Id);
                var fundingClaimIds = new List<int>();

                foreach (var fundingClaimEligibleToBeAutoWithdrawn in fundingClaimsEligibleToBeAutoWithdrawn)
                {
                    var previousStatus = fundingClaimEligibleToBeAutoWithdrawn.Status;
                    fundingClaimEligibleToBeAutoWithdrawn.Status = FundingClaimState.AutoWithdrawn;
                    fundingClaimEligibleToBeAutoWithdrawn.LastUpdatedAt = now;

                    await _fundingClaimRepository.Update(fundingClaimEligibleToBeAutoWithdrawn);

                    await CreateFundingClaimLogAndAudit(fundingClaimEligibleToBeAutoWithdrawn, previousStatus, Audit.Api.Client.Enumerations.ActionType.FundingClaimWithdrawn);

                    fundingClaimIds.Add(fundingClaimEligibleToBeAutoWithdrawn.Id);
                }

                await _emailService.SendFundingClaimWithdrawnEmail(fundingClaimIds);
            }
        }

        /// <inheritdoc/>
        public async Task CreateFundingClaims(
            List<SchemaFundingClaim> fundingClaims, int fundingClaimWindowId)
        {
            var fundingClaimWindow = await _fundingClaimWindowRepository.FirstOrDefault(window =>
                window.Id == fundingClaimWindowId);

            _logger.LogInformation(
                "In a Funding Claim window {DataCollectionKey} {SubmissionOpenDate} - {SubmissionCloseDate}",
                fundingClaimWindow.DataCollectionKey,
                fundingClaimWindow.SubmissionOpenDate,
                fundingClaimWindow.SubmissionCloseDate);

            var newFundingClaimIds = await ProcessFundingClaims(fundingClaims, fundingClaimWindow);

            await UpdateLastRetrievedSetting(fundingClaimWindow);

            await SendReadyToSignAndReadyToViewEmails(newFundingClaimIds, IsFinalFundingClaim(fundingClaimWindow.DataCollectionKey));
        }

        /// <inheritdoc/>
        public async Task<Models.FundingClaim> GetFundingClaimById(int fundingClaimId)
        {
            var fundingClaim = await _fundingClaimRepository.GetFundingClaimById(fundingClaimId);

            return fundingClaim == null ? null : FundingClaimMappings.ToFundingClaim(fundingClaim);
        }

        /// <inheritdoc/>
        public async Task<Models.FundingClaim> GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(int currentFundingClaimId)
        {
            var fundingClaim = await _fundingClaimRepository.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(currentFundingClaimId);

            return fundingClaim == null ? null : FundingClaimMappings.ToFundingClaim(fundingClaim);
        }

        #endregion


        #region Private Methods

        private async Task<List<int>> ProcessFundingClaims(
            List<SchemaFundingClaim> schemaFundingClaims,
            DomainWindow fundingClaimWindow)
        {
            return await CreateNewFundingClaimsInRepository(schemaFundingClaims, fundingClaimWindow);
        }

        private async Task UpdateLastRetrievedSetting(DomainWindow fundingClaimWindow)
        {
            var fundingClaimLastRetrievedSetting = await _settingsRepository.FirstOrDefault(setting => setting.Type == ServiceConstants.FundingClaimLastRetrievedSetting);

            fundingClaimLastRetrievedSetting.Value = _systemProvider.UtcNow().ToString();
            await _settingsRepository.Update(fundingClaimLastRetrievedSetting);

            fundingClaimWindow.LastUpdatedAt = _systemProvider.UtcNow();
            fundingClaimWindow.FundingClaimsRetrieved = true;
            await _fundingClaimWindowRepository.Update(fundingClaimWindow);
        }

        private async Task<List<int>> CreateNewFundingClaimsInRepository(
            List<SchemaFundingClaim> schemaFundingClaims,
            DomainWindow fundingClaimWindow)
        {
            var newFundingClaimIds = new List<int>();
            var alreadyProcessedCount = 0;
            foreach (var schemaFundingClaim in schemaFundingClaims)
            {
                var existingFundingClaim = _fundingClaimRepository.GetFundingClaimForSpecifiedIdentifier(schemaFundingClaim.FundingClaimId).FirstOrDefault();

                if (existingFundingClaim == null)
                {
                    var fundingClaim = await _fundingClaimRepository.CreateFundingClaim(fundingClaimWindow, schemaFundingClaim);

                    await CreateFundingClaimLogAndAudit(fundingClaim, null, Audit.Api.Client.Enumerations.ActionType.FundingClaimCreated);

                    if (IsFinalFundingClaim(fundingClaimWindow.DataCollectionKey))
                    {
                        await ReplacePreviousVersions(fundingClaim);
                    }

                    newFundingClaimIds.Add(fundingClaim.Id);
                }
                else
                {
                    _logger.LogInformation(
                    $"Funding Claim for Ukprn {existingFundingClaim.Ukprn}, Title: {existingFundingClaim.Title} has not changed. FundingClaimId: {schemaFundingClaim.FundingClaimId}");
                    alreadyProcessedCount += 1;
                }
            }

            _logger.LogInformation(
                $"Funding Claim Summary: Total Count: {schemaFundingClaims.Count}, New: {newFundingClaimIds.Count} Already Processed: {alreadyProcessedCount}");

            return newFundingClaimIds;
        }

        private async Task ReplacePreviousVersions(DomainFundingClaim fundingClaimToMatch)
        {
            var versionsOfSameClaim = _fundingClaimRepository
                .Where(fc => fc.FundingClaimWindow.Id == fundingClaimToMatch.FundingClaimWindow.Id
                             && fc.Ukprn == fundingClaimToMatch.Ukprn
                             && fc.Status == FundingClaimState.ReadyToSign
                             && fc.Version < fundingClaimToMatch.Version)
                .OrderBy(fc => fc.Version)
                .ToList();

            foreach (var versionOfSameClaim in versionsOfSameClaim)
            {
                if (versionOfSameClaim.Version < fundingClaimToMatch.Version)
                {
                    await MarkFundingClaimReplaced(versionOfSameClaim);
                }
            }
        }

        private async Task MarkFundingClaimReplaced(DomainFundingClaim fundingClaim)
        {
            var previousStatus = fundingClaim.Status;
            fundingClaim.Status = FundingClaimState.Replaced;
            fundingClaim.LastUpdatedAt = _systemProvider.UtcNow();
            await _fundingClaimRepository.Update(fundingClaim);

            await CreateFundingClaimLogAndAudit(fundingClaim, previousStatus, Audit.Api.Client.Enumerations.ActionType.FundingClaimReplaced);
        }

        private async Task SendReadyToSignAndReadyToViewEmails(List<int> newFundingClaimIds, bool isFinalFundingClaim)
        {
            if (isFinalFundingClaim && newFundingClaimIds.Any())
            {
                await _emailService.SendFundingClaimReadyToSignEmail(newFundingClaimIds);
            }

            if (!isFinalFundingClaim && newFundingClaimIds.Any())
            {
                await _emailService.SendFundingClaimReadyToViewEmail(newFundingClaimIds);
            }
        }

        private async Task CreateFundingClaimLogAndAudit(DomainFundingClaim fundingClaim, FundingClaimState? previousState, Audit.Api.Client.Enumerations.ActionType action, string additionalInformation = "")
        {
            var message = $"Funding Claim [{fundingClaim.FundingClaimUniqueId}] with Id [{fundingClaim.Id}] has ";

            message += action switch
            {
                Audit.Api.Client.Enumerations.ActionType.FundingClaimCreated => "been created. ",
                Audit.Api.Client.Enumerations.ActionType.FundingClaimReplaced => "been replaced. ",
                Audit.Api.Client.Enumerations.ActionType.FundingClaimWithdrawn => "been withdrawn. ",
                _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
            };

            message += additionalInformation;

            if (previousState != null)
            {
                message += $" The funding claim state before was {previousState.Value.GetDisplayName()}. ";
            }

            message += $" The funding claim state after is {fundingClaim.Status.GetDisplayName()}.";

            await _auditService.AuditAsync(
                    new Audit.Api.Client.Models.Audit
                    {
                        Severity = Audit.Api.Client.Enumerations.SeverityLevel.Information,
                        Action = action,
                        Ukprn = fundingClaim.Ukprn,
                        //// Note that we don't have the user data.
                        User = "System",
                        Message = message.TrimEnd()
                    });

            _logger.LogInformation(message);
        }

        private bool IsFinalFundingClaim(string dataCollectionKey)
        {
            return dataCollectionKey.Contains(FinalFundingClaim);
        }

        #endregion
    }
}