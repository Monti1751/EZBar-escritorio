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
        private readonly string _username;
        private readonly string _password;

        public AuthInterceptor(string username, string password)
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
                // Necesario para saltar la página de advertencia de ngrok
                request.Headers.Add("ngrok-skip-browser-warning", "true");
                
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception)
            {
                // Dejamos que la excepción suba para que el ViewModel la capture,
                // pero tener el bloque try-catch aquí ayuda al depurador de VS a entender que está controlada.
                throw;
            }
        }
    }
}
