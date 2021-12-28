using System.Net.Http;
using System.Security;
using Microsoft.Graph;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;


namespace LibraryCloud
{
    class MsalAuthenticationProvider : IAuthenticationProvider
    {
        private string _userId;
        private string[] _scopes;
        private string _username;
        private SecureString _password;
        private IPublicClientApplication _clientApplication;
        private static MsalAuthenticationProvider _singleton;

        private MsalAuthenticationProvider(IPublicClientApplication clientApplication, string[] scopes, string username, SecureString password)
        {
            _userId = null;
            _scopes = scopes;
            _username = username;
            _password = password;
            _clientApplication = clientApplication;
        }

        public static MsalAuthenticationProvider GetInstance(IPublicClientApplication clientApplication, string[] scopes, string username, SecureString password)
        {
            if (_singleton == null)
            {
                _singleton = new MsalAuthenticationProvider(clientApplication, scopes, username, password);
            }

            return _singleton;
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_userId))
            {
                try
                {
                    var account = await _clientApplication.GetAccountAsync(_userId);

                    if (account != null)
                    {
                        var silentResult = await _clientApplication.AcquireTokenSilent(_scopes, account).ExecuteAsync();
                        return silentResult.AccessToken;
                    }
                }
                catch (MsalUiRequiredException) { }
            }

            var result = await _clientApplication.AcquireTokenByUsernamePassword(_scopes, _username, _password).ExecuteAsync();
            _userId = result.Account.HomeAccountId.Identifier;
            return result.AccessToken;
        }
    }
}
