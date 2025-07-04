using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace PeakShock;

public class OpenShockController : IShockController
{
    private readonly ShockRequestQueue _queue = new ShockRequestQueue();
    private readonly string _apiUrl;
    private readonly string _deviceId;
    private readonly string _apiKey;
    private readonly HttpClient _client = new HttpClient();
    private DateTime _lastShockTime = DateTime.MinValue;

    private TimeSpan ShockCooldown => TimeSpan.FromSeconds(1.0 + Math.Max(0.0f, Plugin.ShockCooldownSeconds.Value));

    public OpenShockController()
    {
        _apiUrl = Plugin.OpenShockApiUrl.Value;
        _deviceId = Plugin.OpenShockDeviceId.Value;
        _apiKey = Plugin.OpenShockApiKey.Value;
    }

    public void EnqueueShock(int intensity, int duration, string? code = null)
    {
        var utcNow = DateTime.UtcNow;
        if (utcNow - _lastShockTime < ShockCooldown)
        {
            Plugin.Log.LogInfo("[PeakShock] OpenShock shock skipped due to cooldown.");
            return;
        }
        _lastShockTime = utcNow;
        _queue.Enqueue(() => TriggerShockInternal(intensity, duration, code));
    }

    private async Task TriggerShockInternal(int intensity, int duration, string? code)
    {
        // Convert duration from seconds to milliseconds and clamp to API limits
        int durationMs = Math.Clamp(duration * 1000, 300, 65535);
        if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiKey))
        {
            Plugin.Log.LogWarning($"[PeakShock] Would send OpenShock (intensity={intensity}, duration={durationMs}), but OpenShock credentials are not set.");
            return;
        }
        var id = !string.IsNullOrEmpty(code) ? code : _deviceId;
        if (string.IsNullOrEmpty(id))
        {
            Plugin.Log.LogWarning("[PeakShock] No deviceId or share code provided for OpenShock.");
            return;
        }
        Plugin.Log.LogInfo($"[PeakShock] Sending OpenShock: id={id}, intensity={intensity}, duration={durationMs}");
        var data = new
        {
            Shocks = new[]
            {
                new {
                    Id = id,
                    Type = 1, // 1 = Shock
                    Intensity = intensity,
                    Duration = durationMs
                }
            },
            CustomName = "Integrations.PeakShock"
        };
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "/2/shockers/control")
        {
            Content = content
        };
        request.Headers.Add("OpenShockToken", _apiKey);
        request.Headers.Add("User-Agent", $"PeakShock/{Plugin.Version}");
        try
        {
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Plugin.Log.LogError($"OpenShock API error: {response.StatusCode} {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"OpenShock API exception: {ex}");
        }
    }
}
