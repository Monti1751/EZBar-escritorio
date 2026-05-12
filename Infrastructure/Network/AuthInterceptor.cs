using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EZBarEscritorio.Infrastructure.Network
{
    public class AuthInterceptor : DelegatingHandler
    {
        private string? _username;
        private string? _password;

        public void SetCredentials(string username, string password)
        {
            _username = username;
            _password = password;
        }

        [System.Diagnostics.DebuggerStepThrough]
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try 
            {
                if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
                {
                    var authString = $"{_username}:{_password}";
                    var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
                }
                
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
