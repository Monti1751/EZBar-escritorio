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
    }

    public class NgrokApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        // DI: HttpClient ya viene configurado con el BaseAddress y el Interceptor
        public NgrokApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<T>>() ?? Array.Empty<T>();
        }

        public async Task<bool> PatchAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PatchAsJsonAsync(endpoint, payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, payload);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostAsync<T>(string endpoint, T payload)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, payload);
            return response.IsSuccessStatusCode;
        }
    }
}
