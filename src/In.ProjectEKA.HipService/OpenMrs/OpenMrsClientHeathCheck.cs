
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace In.ProjectEKA.HipService.OpenMrs
{
    public class OpenMrsClientHealthCheck : IHealthCheck{

    public IOpenMrsClient openMrsClient { get; set; }

    public OpenMrsClientHealthCheck(IOpenMrsClient client)
    {
        openMrsClient = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var response = await openMrsClient.GetAsync("ws/fhir2/Patient");
        var healthCheckResultHealthy = false;
        if (response.StatusCode == HttpStatusCode.OK) {
            healthCheckResultHealthy = true;
        }

        if (healthCheckResultHealthy)
        {
            return await Task.FromResult(
                HealthCheckResult.Healthy("A healthy result."));
        }

        return await Task.FromResult(
            HealthCheckResult.Unhealthy("An unhealthy result."));
    }
}
}