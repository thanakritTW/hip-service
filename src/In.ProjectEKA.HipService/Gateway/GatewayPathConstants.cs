namespace In.ProjectEKA.HipService.Gateway
{
    public static class GatewayPathConstants
    {
        public static readonly string OnDiscoverPath = "/v0.5/care-contexts/on-discover";
        public static readonly string OnLinkInitPath = "/v0.5/links/link/on-init";
        public static readonly string OnLinkConfirmPath = "/v0.5/links/link/on-confirm";
        public static readonly string ConsentOnNotifyPath = "/v0.5/consents/hip/on-notify";
        public static readonly string HealthInformationOnRequestPath = "/v0.5/health-information/hip/on-request";
        public static readonly string HealthInformationNotifyGatewayPath = "/v0.5/health-information/notify";
    }
}