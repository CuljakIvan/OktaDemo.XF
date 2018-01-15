using System;
using System.Collections.Generic;
using System.Text;

namespace OktaDemo.XF.Helpers
{
    public class Constants
    {
        public const string ClientId = "xyz";
        public const string RedirectUri = "com.oktapreview.dev-123456:/callback";
        public const string Issuer = "https://dev-123456.oktapreview.com";
        public const string DiscoveryEndpoint = "https://dev-123456.oktapreview.com/.well-known/openid-configuration";
        public const string AuthStateKey = "authState";
        public const string AuthServiceDiscoveryKey = "authServiceDiscovery";
    }
}