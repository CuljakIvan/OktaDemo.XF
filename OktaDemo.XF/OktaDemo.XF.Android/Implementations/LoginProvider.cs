using System;
using System.Threading.Tasks;
using Android.Content;
using Nito.AsyncEx;
using OktaDemo.XF.Droid.Implementations;
using OktaDemo.XF.Helpers;
using OktaDemo.XF.Interfaces;
using OktaDemo.XF.Models;
using OpenId.AppAuth;
using Org.Json;

[assembly: Xamarin.Forms.Dependency(typeof(LoginProvider))]

namespace OktaDemo.XF.Droid.Implementations
{
    public class LoginProvider : ILoginProvider
    {
        private readonly AuthorizationService _authService;
        private AuthState _authState;
        internal static LoginProvider Current;
        private readonly AsyncAutoResetEvent _loginResultWaitHandle = new AsyncAutoResetEvent(false);

        public LoginProvider()
        {
            Current = this;
            _authService = new AuthorizationService(MainActivity.Instance);
        }


        public async Task<AuthInfo> LoginAsync()
        {
            try
            {
                var serviceConfiguration = await AuthorizationServiceConfiguration.FetchFromUrlAsync(
                    Android.Net.Uri.Parse(Constants.DiscoveryEndpoint));

                Console.WriteLine("configuration retrieved, proceeding");

                MakeAuthRequest(serviceConfiguration, new AuthState());

                await _loginResultWaitHandle.WaitAsync();

            }
            catch (AuthorizationException ex)
            {
                Console.WriteLine("Failed to retrieve configuration:" + ex);
            }

            return new AuthInfo()
            {
                IsAuthorized = _authState?.IsAuthorized ?? false,
                AccessToken = _authState?.AccessToken,
                IdToken = _authState?.IdToken,
                RefreshToken = _authState?.RefreshToken,
                Scope = _authState?.Scope
            };
        }

        private void MakeAuthRequest(AuthorizationServiceConfiguration serviceConfig, AuthState authState)
        {
            var authRequest = new AuthorizationRequest.Builder(serviceConfig, Constants.ClientId,
                    ResponseTypeValues.Code, Android.Net.Uri.Parse(Constants.RedirectUri))
                .SetScope("openid profile email offline_access")
                .Build();

            Console.WriteLine("Making auth request to " + serviceConfig.AuthorizationEndpoint);

            var postAuthorizationIntent = MainActivity.CreatePostAuthorizationIntent(MainActivity.Instance, authRequest,
                serviceConfig.DiscoveryDoc, authState);

            var customTabsIntentBuilder = _authService.CreateCustomTabsIntentBuilder();
            var customTabsIntent = customTabsIntentBuilder.Build();

            _authService.PerformAuthorizationRequest(authRequest, postAuthorizationIntent, customTabsIntent);
        }

        internal void NotifyOfCallback(Intent intent)
        {
            try
            {
                _authState = GetAuthStateFromIntent(intent);
                if (_authState != null)
                {
                    AuthorizationResponse response = AuthorizationResponse.FromIntent(intent);
                    AuthorizationException ex = AuthorizationException.FromIntent(intent);
                    _authState.Update(response, ex);

                    if (response != null)
                    {
                        Console.WriteLine("Received AuthorizationResponse.");
                        PerformTokenRequest(response.CreateTokenExchangeRequest());
                    }
                    else
                    {
                        Console.WriteLine("Authorization failed: " + ex);
                    }
                }
                else
                {
                    //We need this line to tell the Login method to return the result
                    _loginResultWaitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                //We need this line to tell the Login method to return the result
                _loginResultWaitHandle.Set();
            }
        }

        private AuthState GetAuthStateFromIntent(Intent intent)
        {
            if (!intent.HasExtra(Constants.AuthStateKey))
            {
                return null;
            }
            try
            {
                return AuthState.JsonDeserialize(intent.GetStringExtra(Constants.AuthStateKey));
            }
            catch (JSONException ex)
            {
                Console.WriteLine("Malformed AuthState JSON saved: " + ex);
                return null;
            }
        }

        private void PerformTokenRequest(TokenRequest request)
        {
            try
            {
                var clientAuthentication = _authState.ClientAuthentication;
            }
            catch (ClientAuthenticationUnsupportedAuthenticationMethod ex)
            {
                //We need this line to tell the Login method to return the result
                _loginResultWaitHandle.Set();

                Console.WriteLine(
                    "Token request cannot be made, client authentication for the token endpoint could not be constructed: " +
                    ex);

                return;
            }

            _authService.PerformTokenRequest(request, ReceivedTokenResponse);
        }

        private void ReceivedTokenResponse(TokenResponse tokenResponse, AuthorizationException authException)
        {
            try
            {
                Console.WriteLine("Token request complete");
                _authState.Update(tokenResponse, authException);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                //We need this line to tell the Login method to return the result
                _loginResultWaitHandle.Set();
            }
        }
    }
}