using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System;

namespace EZBarEscritorio.Infrastructure.Network
{
    public interface IApiService
    {
        Task<IEnumerable<T>> GetAsync<T>(string endpoint);
        Task<bool> PatchAsync<T>(string endpoint, T payload);
        Task<bool> PutAsync<T>(string endpoint, T payload);
        Task<bool> PostAsync<T>(string endpoint, T payload);
        void SetBaseUrl(string url);
    }

    public class NgrokApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private string? _dynamicBaseUrl;

        public NgrokApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string BuildUrl(string endpoint)
        {
            if (string.IsNullOrEmpty(_dynamicBaseUrl)) return endpoint;
            
            var baseUri = _dynamicBaseUrl.TrimEnd('/');
            var cleanEndpoint = endpoint.TrimStart('/');
            return $"{baseUri}/{cleanEndpoint}";
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(BuildUrl(endpoint)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<T>>().ConfigureAwait(false) ?? Array.Empty<T>();
        }

        public async Task<bool> PatchAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PatchAsJsonAsync(BuildUrl(endpoint), payload).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PutAsJsonAsync(BuildUrl(endpoint), payload).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PostAsJsonAsync(BuildUrl(endpoint), payload).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }

        public void SetBaseUrl(string url)
        {
            _dynamicBaseUrl = url;
        }
    }
}
