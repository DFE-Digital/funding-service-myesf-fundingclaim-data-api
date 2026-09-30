using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.FundingClaim.Api.Models;
using Pds.FundingClaim.CorporateSchema.FundingClaims;
using Pds.FundingClaim.Services.Interfaces;
using Pds.FundingClaim.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.FundingClaim.Api.Controllers
{
    /// <summary>
    /// Api that supports CRUD operations on funding claim data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class FundingClaimController : Controller
    {
        #region Private Fields

        private readonly ISettingDataService _settingDataService;
        private readonly IFundingClaimWindowDataService _fundingClaimWindowDataService;
        private readonly IFundingClaimDataService _fundingClaimDataService;
        private readonly ILoggerAdapter<FundingClaimController> _logger;

        #endregion Private Fields


        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingClaimController"/> class.
        /// </summary>
        /// <param name="settingDataService">Concrete class that implements <see cref=ISettingDataService"/>.</param>
        /// <param name="fundingClaimWindowDataService">Concrete class that implements <see cref=IFundingClaimWindowDataService"/>.</param>
        /// <param name="fundingClaimDataService">Concrete class that implements <see cref=IFundingClaimDataService"/>.</param>
        /// <param name="logger">The logger.</param>
        public FundingClaimController(
            ISettingDataService settingDataService,
            IFundingClaimWindowDataService fundingClaimWindowDataService,
            IFundingClaimDataService fundingClaimDataService,
            ILoggerAdapter<FundingClaimController> logger)
        {
            _settingDataService = settingDataService;
            _fundingClaimWindowDataService = fundingClaimWindowDataService;
            _fundingClaimDataService = fundingClaimDataService;
            _logger = logger;
        }

        #endregion Public Constructors


        #region Api Methods

        /// <summary>
        /// Api method to get funding claim by funding claim Id.
        /// </summary>
        /// <param name="fundingClaimId">Funding claim Id.</param>
        /// <returns>Funding claim with matching Id.</returns>
        [HttpGet("{fundingClaimId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Services.Models.FundingClaim> GetFundingClaimById(int fundingClaimId)
        {
            var fundingClaim = await _fundingClaimDataService.GetFundingClaimById(fundingClaimId);

            _logger.LogInformation($"{(fundingClaim == null ? "Null" : $"Funding claim with Id [{fundingClaim.Id}]")} returned from GetFundingClaimById using Id {fundingClaimId}");

            return fundingClaim;
        }

        /// <summary>
        /// Api method to get previously signed version of funding claim by current funding claim Id.
        /// </summary>
        /// <param name="currentFundingClaimId">Current Funding claim Id.</param>
        /// <returns>Previously signed version of funding claim by current funding claim Id.</returns>
        [HttpGet("{currentFundingClaimId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<Services.Models.FundingClaim> GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(int currentFundingClaimId)
        {
            var fundingClaim = await _fundingClaimDataService.GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId(currentFundingClaimId);

            _logger.LogInformation($"{(fundingClaim == null ? "Null" : $"Funding claim with Id [{fundingClaim.Id}]")} returned from GetPreviouslySignedVersionOfFundingClaimByCurrentFundingClaimId using currentFundingClaimId {currentFundingClaimId}");

            return fundingClaim;
        }

        /// <summary>
        /// Api method to get funding claim last retrieved setting when requested.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<string> GetFundingClaimLastRetrievedSetting()
        {
            var settingResult = await _settingDataService.GetFundingClaimLastRetrievedSetting();
            _logger.LogInformation($"GetFundingClaimLastRetrievedSetting returned setting value: {settingResult}");

            return settingResult;
        }

        /// <summary>
        /// Api method to get funding claim polling setting when requested.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<string> GetFundingClaimPollingSetting()
        {
            var settingResult = await _settingDataService.GetFundingClaimPollingSetting();
            _logger.LogInformation($"GetFundingClaimPollingSetting returned setting value: {settingResult}");

            return settingResult;
        }

        /// <summary>
        /// Api method to get use json format of funding claims setting when requested.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<string> GetUseJsonFormatOfFundingClaimsSetting()
        {
            var settingResult = await _settingDataService.GetUseJsonFormatOfFundingClaimsSetting();
            _logger.LogInformation($"GetUseJsonFormatOfFundingClaimsSetting returned setting value: {settingResult}");

            return settingResult;
        }

        /// <summary>
        /// Api method to get funding claim current window.
        /// </summary>
        /// <returns><see cref="Task{FundingClaimWindow}"/>.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<FundingClaimWindow> GetFundingClaimCurrentWindow()
        {
            var currentWindow = await _fundingClaimWindowDataService.GetFundingClaimCurrentWindow();
            _logger.LogInformation($"GetFundingClaimCurrentWindow returned window with Data Collection key: {currentWindow?.DataCollectionKey}");

            return currentWindow;
        }

        /// <summary>
        /// Api method to create/update funding claim windows.
        /// </summary>
        /// <param name="fundingClaimDetails">List of funding claim windows to be added or updated.</param>
        /// <returns><see cref="Task{IActionResult}"/>.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateFundingClaimWindow(List<FundingClaimDetails> fundingClaimDetails)
        {
            await _fundingClaimWindowDataService.UpdateFundingClaimWindow(fundingClaimDetails);
            _logger.LogInformation("UpdateFundingClaimWindow ran successfully.");

            return Ok();
        }

        /// <summary>
        /// Api method to CreateFundingClaims and send relevant emails.
        /// </summary>
        /// <param name="createFundingClaimsRequest">Request to DCT to CreateFundingClaims.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateFundingClaims(CreateFundingClaimsApiRequest createFundingClaimsRequest)
        {
            _logger.LogInformation(
                "CreateFundingClaims for window Id {FundingClaimWindowId}. Funding claims to process {Count}",
                createFundingClaimsRequest.FundingClaimWindowId,
                createFundingClaimsRequest.FundingClaims.Count);

            await _fundingClaimDataService.CreateFundingClaims(createFundingClaimsRequest.FundingClaims, createFundingClaimsRequest.FundingClaimWindowId);

            _logger.LogInformation(
            "CreateFundingClaims ran successfully for funding claim window {FundingClaimWindowId}.", createFundingClaimsRequest.FundingClaimWindowId);

            return Created("CreateFundingClaims", null);
        }

        #endregion
    }
}