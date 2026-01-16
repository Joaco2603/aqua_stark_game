using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Managers.Adapter.Interfaces;

namespace Managers.Adapter
{
    public class HttpAdapter : IHttpClient, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string>();
        private bool _disposed;

        public HttpAdapter(string baseUrl, Dictionary<string, string> defaultHeaders = null, HttpClient httpClient = null)
        {
            _baseUrl = string.IsNullOrEmpty(baseUrl) ? string.Empty : baseUrl;
            if (defaultHeaders != null)
            {
                foreach (var kv in defaultHeaders) _defaultHeaders[kv.Key] = kv.Value;
            }

            if (httpClient != null)
            {
                _httpClient = httpClient;
            }
            else
            {
                _httpClient = new HttpClient();
            }
        }

        public async Task<object> GetAsync(string url, RequestConfig config = null)
        {
            var requestUri = BuildUri(url, config?.Params);
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            ApplyHeaders(request, config);
            ApplyTimeout(config);

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return await HandleResponse(response).ConfigureAwait(false);
        }

        public async Task<object> PostAsync(string url, object data = null, RequestConfig config = null)
        {
            var requestUri = BuildUri(url, config?.Params);
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            ApplyHeaders(request, config, includeJsonContentType: true);
            ApplyTimeout(config);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return await HandleResponse(response).ConfigureAwait(false);
        }

        public async Task<object> PutAsync(string url, object data = null, RequestConfig config = null)
        {
            var requestUri = BuildUri(url, config?.Params);
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            ApplyHeaders(request, config, includeJsonContentType: true);
            ApplyTimeout(config);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return await HandleResponse(response).ConfigureAwait(false);
        }

        public async Task<object> DeleteAsync(string url, RequestConfig config = null)
        {
            var requestUri = BuildUri(url, config?.Params);
            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            ApplyHeaders(request, config);
            ApplyTimeout(config);

            using var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            return await HandleResponse(response).ConfigureAwait(false);
        }

        private Uri BuildUri(string url, Dictionary<string, object> queryParams)
        {
            var baseUri = string.IsNullOrEmpty(_baseUrl) ? null : new Uri(_baseUrl.EndsWith("/") ? _baseUrl : _baseUrl + "/");
            var relative = string.IsNullOrEmpty(url) ? string.Empty : url;
            var uri = baseUri == null ? new Uri(relative, UriKind.RelativeOrAbsolute) : new Uri(baseUri, relative);

            if (queryParams == null || queryParams.Count == 0) return uri;

            var sb = new StringBuilder();
            foreach (var kv in queryParams)
            {
                if (sb.Length > 0) sb.Append('&');
                var value = kv.Value?.ToString() ?? string.Empty;
                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(value));
            }

            var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
            var full = uri.ToString() + separator + sb.ToString();
            return new Uri(full, UriKind.RelativeOrAbsolute);
        }

        private void ApplyHeaders(HttpRequestMessage request, RequestConfig config, bool includeJsonContentType = false)
        {
            // default headers
            foreach (var kv in _defaultHeaders)
            {
                if (!request.Headers.Contains(kv.Key)) request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            // config headers
            if (config?.Headers != null)
            {
                foreach (var kv in config.Headers)
                {
                    if (!request.Headers.Contains(kv.Key)) request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            if (includeJsonContentType && request.Content == null)
            {
                // ensure content type will be applied when content is set; nothing to do here now
            }
        }

        private void ApplyTimeout(RequestConfig config)
        {
            if (config?.Timeout != null)
            {
                try
                {
                    _httpClient.Timeout = TimeSpan.FromMilliseconds(config.Timeout.Value);
                }
                catch
                {
                    // ignore invalid timeout values
                }
            }
        }

        private static async Task<object> HandleResponse(HttpResponseMessage response)
        {
            var content = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var reason = response.ReasonPhrase ?? "";
                throw new HttpRequestException($"Error {(int)response.StatusCode}: {reason}") { Data = { { "Content", content } } };
            }

            if (string.IsNullOrWhiteSpace(content)) return null;

            try
            {
                var obj = JsonConvert.DeserializeObject<object>(content);
                return obj;
            }
            catch
            {
                return content;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
