using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeakShock
{
    public class PiShockController : IShockController
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly ShockRequestQueue _queue = new ShockRequestQueue();

        private DateTime _lastShockTime = DateTime.MinValue;
        private TimeSpan ShockCooldown => TimeSpan.FromSeconds(1 + Math.Max(0, Plugin.ShockCooldownSeconds.Value)); // forced 1s minimum, plus user config

        public void TriggerShock(int intensity, int duration = 1, string? shareCode = null)
        {
            var now = DateTime.UtcNow;
            if (now - _lastShockTime < ShockCooldown)
            {
                Plugin.Log.LogInfo($"[PeakShock] Shock skipped due to cooldown.");
                return;
            }
            _lastShockTime = now;
            var code = shareCode ?? Plugin.PiShockShareCode.Value;
            Plugin.Log.LogInfo($"[PeakShock] Enqueue shock: intensity={intensity}, duration={duration}, code={code}");
            _queue.Enqueue(() => TriggerShockInternal(intensity, duration, code));
        }

        public void EnqueueShock(int intensity, int duration, string? code = null)
        {
            TriggerShock(intensity, duration, code);
        }

        private async Task TriggerShockInternal(int intensity, int duration, string code)
        {
            var user = Plugin.PiShockUserName.Value;
            var key = Plugin.PiShockAPIKey.Value;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code))
            {
                Plugin.Log.LogWarning($"[PeakShock] Would send shock (intensity={intensity}, duration={duration}, code={code}), but PiShock credentials are not set.");
                return;
            }
            Plugin.Log.LogInfo($"[PeakShock] Sending shock: intensity={intensity}, duration={duration}, code={code}");

            var json = JsonConvert.SerializeObject(new
            {
                Username = user,
                APIKey = key,
                Code = code,
                Intensity = intensity,
                Duration = duration,
                Op = 0 // 0 = shock
            });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            try
            { 
                Plugin.Log.LogInfo($"[PeakShock] Sending request to PiShock API: {user}, Intensity={intensity}, Duration={duration}");
                var response = await _client.PostAsync("https://do.pishock.com/api/apioperate/", content);
                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.LogError($"PiShock API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"PiShock API exception: {ex}");
            }
        }
    }
}
