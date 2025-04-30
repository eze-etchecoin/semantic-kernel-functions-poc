using Microsoft.SemanticKernel;
using SemanticKernelFunctionsPoc.Shared;
using System.Net.Http.Json;
using System.Text.Json;
using System.ComponentModel;

namespace SemanticKernelFunctionsPoc.Plugins;

/// <summary>
/// A Semantic Kernel plugin to interact with the Trips API.
/// </summary>
public class TripsApiPlugin
{
    private const string API_BASE_URL = "http://localhost:5271/";
    private static JsonSerializerOptions JsonResponseSerializerOptions => new() { WriteIndented = true };

    private readonly HttpClient _httpClient;

    private string SessionId { get; set; }

    public TripsApiPlugin()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(API_BASE_URL)
        };
    }

    /// <summary>
    /// Performs a simple login to the API to obtain a session ID.
    /// </summary>
    /// <param name="userName">The username for login.</param>
    /// <returns>The session ID if login is successful, otherwise an error message.</returns>
    [KernelFunction]
    [Description("Performs a simple login with a username to get a session ID.")]
    public async Task<string> SimpleLogin(
        [Description("The username to use for login.")] string userName)
    {
        var loginRequest = new LoginRequest { UserName = userName };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("simple_login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                var respSessionId = loginResponse?.SessionId ?? "Login failed: Could not retrieve session ID.";

                SessionId = respSessionId;

                return respSessionId;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Login failed: API returned status code {response.StatusCode} - {error}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            return $"Login failed: HTTP request error - {httpEx.Message}";
        }
        catch (JsonException jsonEx)
        {
            return $"Login failed: JSON deserialization error - {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            return $"Login failed: An unexpected error occurred - {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the list of trips for the authenticated customer.
    /// </summary>
    /// <param name="sessionId">The session ID obtained from login.</param>
    /// <returns>A JSON string representation of the list of trips, or an error message.</returns>
    [KernelFunction]
    [Description("Gets the list of trips for the current session.")]
    public async Task<string> GetTrips()
    {
        if(!VerifySessionId(out var sessionId)) return "GetTrips failed: Session is unauthorized or invalid.";

        var url = $"trips";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("SessionId", sessionId);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var trips = await response.Content.ReadFromJsonAsync<List<Trip>>();
                // Return as JSON string for easier processing by the LLM/planner
                return JsonSerializer.Serialize(trips, JsonResponseSerializerOptions);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "GetTrips failed: Session is unauthorized or invalid.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"GetTrips failed: API returned status code {response.StatusCode} - {error}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            return $"GetTrips failed: HTTP request error - {httpEx.Message}";
        }
        catch (JsonException jsonEx)
        {
            return $"GetTrips failed: JSON deserialization error - {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            return $"GetTrips failed: An unexpected error occurred - {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the list of vehicles for the authenticated customer.
    /// </summary>
    /// <param name="sessionId">The session ID obtained from login.</param>
    /// <returns>A JSON string representation of the list of vehicles, or an error message.</returns>
    [KernelFunction]
    [Description("Gets the list of vehicles for the current session.")]
    public async Task<string> GetVehicles()
    {
        if (!VerifySessionId(out var sessionId)) return "GetVehicles failed: Session is unauthorized or invalid.";

        var url = $"vehicles";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("SessionId", sessionId);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var vehicles = await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                // Return as JSON string
                return JsonSerializer.Serialize(vehicles, JsonResponseSerializerOptions);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "GetVehicles failed: Session is unauthorized or invalid.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"GetVehicles failed: API returned status code {response.StatusCode} - {error}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            return $"GetVehicles failed: HTTP request error - {httpEx.Message}";
        }
        catch (JsonException jsonEx)
        {
            return $"GetVehicles failed: JSON deserialization error - {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            return $"GetVehicles failed: An unexpected error occurred - {ex.Message}";
        }
    }

    /// <summary>
    /// Gets the list of drivers for the authenticated customer.
    /// </summary>
    /// <param name="sessionId">The session ID obtained from login.</param>
    /// <returns>A JSON string representation of the list of drivers, or an error message.</returns>
    [KernelFunction]
    [Description("Gets the list of drivers for the current session.")]
    public async Task<string> GetDrivers()
    {
        if (!VerifySessionId(out var sessionId)) return "GetDrivers failed: Session is unauthorized or invalid.";

        var url = $"drivers";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("SessionId", sessionId);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var drivers = await response.Content.ReadFromJsonAsync<List<Driver>>();
                // Return as JSON string
                return JsonSerializer.Serialize(drivers, JsonResponseSerializerOptions);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return "GetDrivers failed: Session is unauthorized or invalid.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"GetDrivers failed: API returned status code {response.StatusCode} - {error}";
            }
        }
        catch (HttpRequestException httpEx)
        {
            return $"GetDrivers failed: HTTP request error - {httpEx.Message}";
        }
        catch (JsonException jsonEx)
        {
            return $"GetDrivers failed: JSON deserialization error - {jsonEx.Message}";
        }
        catch (Exception ex)
        {
            return $"GetDrivers failed: An unexpected error occurred - {ex.Message}";
        }
    }

    private bool VerifySessionId(out string sessionId)
    {
        sessionId = SessionId;
        return !string.IsNullOrEmpty(sessionId);
    }
}