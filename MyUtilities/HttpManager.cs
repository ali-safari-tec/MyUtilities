using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;


// We shoulde not use any NuGet for this scope.

// ************************************************************* Explain This Class *************************************************************
// **                                                                                                                                          **
// **     This class create for : Http manager with (Get (JSON, string), Post (JSON, string), Put, Delete) and Initialize tools.               **
// **                                                                                                                                          **
// **                                                   * Main explaination is in comment *                                                    **
// **                                                                                                                                          **
// **     Public Methodes :                                                                                                                    **
// **                        => Initialize                                                                                                     **
// **                        => GetAsync                                                                                                       **
// **                        => GetStringAsync                                                                                                 **
// **                        => PostAsync                                                                                                      **
// **                        => PostStringAsync                                                                                                **
// **                        => PutAsync                                                                                                       **
// **                        => DeleteAsync                                                                                                    **
// **                                                                                                                                          **
// **     Private Methodes :                                                                                                                   **
// **                        => CreateJsonContent                                                                                              **
// **                        => SendRequestAsync                                                                                               **
// **                        => DeserializeJsonResponseAsync                                                                                   **
// **                        => EnsureSuccessStatusCodeAsync                                                                                   **
// **                                                                                                                                          **
// ********************************************************************************************************************************************** 

namespace MyUtilities.Net
{
    /// <summary>
    /// Custom exception for HTTP request errors, providing more context.
    /// </summary>
    public class HttpManagerException(string message, int? statusCode = null, string? responseContent = null, Exception? innerException = null)
    : Exception(message, innerException)
    {
        public int? StatusCode { get; } = statusCode;
        public string? ResponseContent { get; } = responseContent;
    }

    /// <summary>
    /// Provides comprehensive manager methods for making HTTP requests, simplifying common tasks like GET, POST, PUT, DELETE, and handling responses with advanced error management.
    /// </summary>
    public static class HttpManager
    {
        // Shared httpClient instance.
        private static readonly HttpClient _httpClient = new();

        /// <summary>
        /// Initializes default settings for the HttpClient, such as User-Agent, Accept header, and potentially timeouts.
        /// </summary>
        /// <param name="defaultUserAgent">The default User-Agent string to send with requests.</param>
        /// <param name="defaultAcceptHeader">The default Accept header value (e.g., "application/json").</param>
        /// <param name="defaultTimeoutSeconds">The default timeout for requests in seconds. Set to null to use the system default.</param>
        public static void Initialize(string? defaultUserAgent = "MyUtilitiesHttpClientHelper/1.0", string defaultAcceptHeader = "application/json", double? defaultTimeoutSeconds = 100.0)
        {
            _httpClient.DefaultRequestHeaders.Clear(); // Ensure we start with a clean slate

            if (!string.IsNullOrEmpty(defaultUserAgent))
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(defaultUserAgent);
            }

            if (!string.IsNullOrEmpty(defaultAcceptHeader))
            {
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(defaultAcceptHeader));
            }

            if (defaultTimeoutSeconds.HasValue)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(defaultTimeoutSeconds.Value);
            }
            else
            {
                // Reset to default if null is provided
                _httpClient.Timeout = TimeSpan.FromSeconds(100); // Example default, adjust as needed
            }
        }

        /// <summary>
        /// Sends an HTTP GET request.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize the JSON response into.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The deserialized response object.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors including network issues, non-success status codes, cancellation, or timeouts.</exception>
        public static async Task<TResponse> GetAsync<TResponse>(string url, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            var response = await SendRequestAsync(HttpMethod.Get, url, null, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken); // Check status code and throw if not success

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return await DeserializeJsonResponseAsync<TResponse>(responseBody) ?? throw new HttpManagerException("Received empty or invalid response body.", (int)response.StatusCode, responseBody);
        }

        /// <summary>
        /// Sends an HTTP GET request and returns the response as a string.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors.</exception>
        public static async Task<string> GetStringAsync(string url, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var response = await SendRequestAsync(HttpMethod.Get, url, null, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            return await response.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
        }

        /// <summary>
        /// Sends an HTTP POST request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the data to send in the request body.</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the JSON response into.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The data object to serialize and send as JSON in the request body.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The deserialized response object.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors.</exception>
        public static async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest data, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            var response = await SendRequestAsync(HttpMethod.Post, url, data, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return await DeserializeJsonResponseAsync<TResponse>(responseBody) ?? throw new HttpManagerException("Received empty or invalid response body.", (int)response.StatusCode, responseBody);
        }

        /// <summary>
        /// Sends an HTTP POST request with data and returns the response as a string.
        /// </summary>
        /// <typeparam name="TRequest">The type of the data to send in the request body.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The data object to serialize and send as JSON in the request body.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The response content as a string.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors.</exception>
        public static async Task<string> PostStringAsync<TRequest>(string url, TRequest data, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var response = await SendRequestAsync(HttpMethod.Post, url, data, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            return await response.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
        }

        /// <summary>
        /// Sends an HTTP PUT request.
        /// </summary>
        /// <typeparam name="TRequest">The type of the data to send in the request body.</typeparam>
        /// <typeparam name="TResponse">The type to deserialize the JSON response into.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="data">The data object to serialize and send as JSON in the request body.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The deserialized response object.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors.</exception>
        public static async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest data, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            var response = await SendRequestAsync(HttpMethod.Put, url, data, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return await DeserializeJsonResponseAsync<TResponse>(responseBody) ?? throw new HttpManagerException("Received empty or invalid response body.", (int)response.StatusCode, responseBody);
        }

        /// <summary>
        /// Sends an HTTP DELETE request.
        /// </summary>
        /// <typeparam name="TResponse">The type to deserialize the JSON response into.</typeparam>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">Optional dictionary of custom headers.</param>
        /// <param name="timeout">Optional timeout for this specific request.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The deserialized response object.</returns>
        /// <exception cref="HttpManagerException">Thrown for various errors.</exception>
        public static async Task<TResponse> DeleteAsync<TResponse>(string url, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default) where TResponse : class
        {
            var response = await SendRequestAsync(HttpMethod.Delete, url, null, headers, timeout, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return await DeserializeJsonResponseAsync<TResponse>(responseBody) ?? throw new HttpManagerException("Received empty or invalid response body.", (int)response.StatusCode, responseBody);
        }


        //-------------------------------------------------------------------------- Private Methodes --------------------------------------------------------------------------//

        private readonly static JsonSerializerOptions _defaultJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true, // Allows deserializing JSON where property names might differ in casing
            // Add other default options if needed, e.g., converters, handlers
        };

        // Helper for JSON Serialization/Deserialization
        private static StringContent CreateJsonContent<T>(T data)
        {
            string json = JsonSerializer.Serialize(data, _defaultJsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // Core request methods.
        private static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object? content = null, IDictionary<string, string>? headers = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var requestMessage = new HttpRequestMessage(method, url);

            if (content != null)
            {
                requestMessage.Content = CreateJsonContent(content);
            }

            // Add custom headers for this specific request
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    // Avoid adding default headers again if they are passed here
                    if (!_httpClient.DefaultRequestHeaders.Any(h => h.Key.Equals(header.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            // Use a specific timeout for this request if provided, otherwise use the default HttpClient timeout
            var client = _httpClient;
            var originalTimeout = client.Timeout;
            if (timeout.HasValue)
            {
                client.Timeout = timeout.Value;
            }

            try
            {
                var response = await client.SendAsync(requestMessage, cancellationToken);
                // Reset timeout if it was changed
                if (timeout.HasValue)
                {
                    client.Timeout = originalTimeout;
                }
                return response;
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                // Reset timeout if it was changed
                if (timeout.HasValue)
                {
                    client.Timeout = originalTimeout;
                }
                throw new HttpManagerException("Request was cancelled.", null, null, ex);
            }
            catch (TaskCanceledException ex) // Likely a timeout
            {
                // Reset timeout if it was changed
                if (timeout.HasValue)
                {
                    client.Timeout = originalTimeout;
                }
                throw new HttpManagerException($"Request timed out after {client.Timeout.TotalSeconds} seconds.", null, null, ex);
            }
            catch (HttpRequestException ex)
            {
                // Reset timeout if it was changed
                if (timeout.HasValue)
                {
                    client.Timeout = originalTimeout;
                }
                // HttpRequestException might not have status code or response content readily available here
                throw new HttpManagerException("An HTTP request error occurred.", null, null, ex);
            }
        }

        private static async Task<T?> DeserializeJsonResponseAsync<T>(string jsonContent) where T : class
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }
            try
            {
                return await Task.Run(() => JsonSerializer.Deserialize<T>(jsonContent, _defaultJsonOptions));
            }
            catch (JsonException ex)
            {
                // Log or handle JSON parsing errors
                throw new HttpManagerException("Failed to deserialize JSON response.", null, jsonContent, ex);
            }
        }

        /// <summary>
        /// Checks if the HTTP response status code indicates success (2xx).
        /// If not, it reads the response content and throws a detailed HttpManagerException.
        /// </summary>
        /// <param name="response">The HttpResponseMessage to check.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="HttpManagerException">Thrown if the status code is not successful.</exception>
        private static async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string? responseContent = null;
            try
            {
                // Try reading content even for error responses, as it might contain useful details
                responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch { /* Ignore errors during error content reading */ }

            // Throw a custom exception with details
            throw new HttpManagerException(
                $"HTTP request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).",
                (int)response.StatusCode,
                responseContent);
        }
    }
}

