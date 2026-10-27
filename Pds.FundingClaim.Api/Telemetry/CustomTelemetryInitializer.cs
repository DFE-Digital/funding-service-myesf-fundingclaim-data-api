using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;

namespace Pds.FundingClaim.Api.Telemetry
{
    /// <summary>
    /// The custom telemetry initializer for adding properties to all telemetry.
    /// </summary>
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _environment, _component;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomTelemetryInitializer"/> class.
        /// </summary>
        /// <param name="environment">The execution environment.</param>
        /// <param name="component">The name of the component.</param>
        public CustomTelemetryInitializer(string environment, string component)
        {
            _environment = environment;
            _component = component;
        }

        /// <inheritdoc/>
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.GlobalProperties.TryAdd("environment", _environment);
            telemetry.Context.GlobalProperties.TryAdd("component", _component);
        }
    }
}