using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EZBarEscritorio.Infrastructure.Network
{
    public class AuthInterceptor : DelegatingHandler
    {
        private readonly string _jwtToken;

        public AuthInterceptor(string token)
        {
            _jwtToken = token;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_jwtToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
            }
            // Necesario para saltar la página de advertencia de ngrok
            request.Headers.Add("ngrok-skip-browser-warning", "true");
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
