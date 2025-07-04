using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PeakShock
{
    // Here are some basic resources on code style and naming conventions to help
    // you in your first CSharp plugin!
    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
    // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
    // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

    // This BepInAutoPlugin attribute comes from the Hamunii.BepInEx.AutoPlugin
    // NuGet package, and it will generate the BepInPlugin attribute for you!
    // For more info, see https://github.com/Hamunii/BepInEx.AutoPlugin
    [BepInAutoPlugin]
    public partial class Plugin : BaseUnityPlugin
    {
        public const string PluginVersion = "0.1.4";
        internal static ManualLogSource Log { get; private set; } = null!;
        internal static ConfigFile CFG { get; private set; } = null!;
        internal static PiShockController PiShockController { get; private set; } = null!;
        internal static ConfigEntry<string> PiShockUserName { get; private set; } = null!;
        internal static ConfigEntry<string> PiShockAPIKey { get; private set; } = null!;
        internal static ConfigEntry<string> PiShockShareCode { get; private set; } = null!;
        internal static ConfigEntry<int> MinShock { get; private set; } = null!;
        internal static ConfigEntry<int> MaxShock { get; private set; } = null!;
        internal static ConfigEntry<int> DeathShock { get; private set; } = null!;
        internal static ConfigEntry<int> DeathDuration { get; private set; } = null!;
        internal static ConfigEntry<bool> EnableInjuryShock { get; private set; } = null!;
        internal static ConfigEntry<bool> EnablePoisonShock { get; private set; } = null!;
        internal static ConfigEntry<bool> EnableColdShock { get; private set; } = null!;
        internal static ConfigEntry<bool> EnableHotShock { get; private set; } = null!;
        internal static ConfigEntry<float> ShockCooldownSeconds { get; private set; } = null!;
        public enum ShockProvider { PiShock, OpenShock }
        internal static ConfigEntry<ShockProvider> ShockProviderType { get; private set; } = null!;
        internal static ConfigEntry<string> OpenShockApiUrl { get; private set; } = null!;
        internal static ConfigEntry<string> OpenShockDeviceId { get; private set; } = null!;
        internal static ConfigEntry<string> OpenShockApiKey { get; private set; } = null!;
        internal static IShockController ShockController { get; private set; } = null!;
        private Harmony? _harmony;

        private void Awake()
        {
            // BepInEx gives us a logger which we can use to log information.
            // See https://lethal.wiki/dev/fundamentals/logging
            Log = Logger;

            // BepInEx also gives us a config file for easy configuration.
            // See https://lethal.wiki/dev/intermediate/custom-configs

            // We can apply our hooks here.
            // See https://lethal.wiki/dev/fundamentals/patching-code

            // Config
            CFG = Config;
            PiShockUserName = CFG.Bind("PiShock", "UserName", "", "Your PiShock username");
            PiShockAPIKey = CFG.Bind("PiShock", "APIKey", "", "Your PiShock API Key");
            PiShockShareCode = CFG.Bind("PiShock", "ShareCode", "", "Your PiShock ShareCode");
            MinShock = CFG.Bind("Shock", "MinShock", 10, "Minimum shock intensity (1-100)");
            MaxShock = CFG.Bind("Shock", "MaxShock", 100, "Maximum shock intensity (1-100)");
            DeathShock = CFG.Bind("Shock", "DeathShock", 80, "Shock intensity on death (1-100)");
            DeathDuration = CFG.Bind("Shock", "DeathDuration", 2, "Shock duration on death (seconds)");
            EnableInjuryShock = CFG.Bind("ShockTypes", "EnableInjuryShock", true, "Enable shock for Injury damage");
            EnablePoisonShock = CFG.Bind("ShockTypes", "EnablePoisonShock", false, "Enable shock for Poison damage");
            EnableColdShock = CFG.Bind("ShockTypes", "EnableColdShock", false, "Enable shock for Cold damage");
            EnableHotShock = CFG.Bind("ShockTypes", "EnableHotShock", false, "Enable shock for Hot/Fire damage");
            ShockCooldownSeconds = CFG.Bind("Shock", "ShockCooldownSeconds", 2f, "Minimum seconds between shocks (prevents shock spam)");
            ShockProviderType = CFG.Bind("Shock", "Provider", ShockProvider.PiShock, "Choose PiShock or OpenShock");
            OpenShockApiUrl = CFG.Bind("OpenShock", "ApiUrl", "https://api.openshock.app", "OpenShock API URL");
            OpenShockDeviceId = CFG.Bind("OpenShock", "DeviceId", "", "OpenShock Device ID");
            OpenShockApiKey = CFG.Bind("OpenShock", "ApiKey", "", "OpenShock API Key");

            Log.LogInfo($"[PeakShock] Config:\nMinShock={MinShock.Value}\nMaxShock={MaxShock.Value}\nDeathShock={DeathShock.Value}\nDeathDuration={DeathDuration.Value}\nEnableInjuryShock={EnableInjuryShock.Value}\nEnablePoisonShock={EnablePoisonShock.Value}\nEnableColdShock={EnableColdShock.Value}\nEnableHotShock={EnableHotShock.Value}\nShockCooldownSeconds={ShockCooldownSeconds.Value}");

            if (ShockProviderType.Value == ShockProvider.PiShock)
            {
                ShockController = new PiShockController();
                Log.LogInfo("[PeakShock] Initialized PiShockController");
            }
            else
            {
                ShockController = new OpenShockController();
                Log.LogInfo("[PeakShock] Initialized OpenShockController");
            }
            _harmony = new Harmony("com.yourname.PeakShock");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Log our awake here so we can see it in LogOutput.log file
            Log.LogInfo($"Plugin {Name} is loaded!");
        }
    }

    namespace PeakShock.Patches
    {
        [HarmonyPatch(typeof(Character), "RPCA_Die")]
        public class CharacterDeathPatch
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance)
            {
                if (__instance.IsLocal)
                {
                    int intensity = Plugin.DeathShock.Value;
                    int duration = Plugin.DeathDuration.Value;
                    Plugin.Log.LogInfo($"[PeakShock] Player died. Triggering death shock: {intensity}% for {duration}s");
                    Task.Run(() => Plugin.ShockController.EnqueueShock(intensity, duration));
                }
            }
        }

        [HarmonyPatch(typeof(CharacterAfflictions), nameof(CharacterAfflictions.AddStatus))]
        public class CharacterAfflictions_AddStatus_Patch
        {

            // Affliction Type Enabled Dicitonary
            private static readonly Dictionary<CharacterAfflictions.STATUSTYPE, bool> ShockTypeEnabled = new Dictionary<CharacterAfflictions.STATUSTYPE, bool>
            {
                { CharacterAfflictions.STATUSTYPE.Hunger, false },
                { CharacterAfflictions.STATUSTYPE.Injury, Plugin.EnableInjuryShock.Value },
                { CharacterAfflictions.STATUSTYPE.Poison, Plugin.EnablePoisonShock.Value },
                { CharacterAfflictions.STATUSTYPE.Cold, Plugin.EnableColdShock.Value },
                { CharacterAfflictions.STATUSTYPE.Hot, Plugin.EnableHotShock.Value }
            };

            [HarmonyPostfix]
            public static void Postfix(CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, float amount, bool fromRPC)
            {
                if (!__instance.character.IsLocal) return;
                if (amount <= 0f) return;
                // Ignore all status effect shocks if the player is dead or passed out
                if (__instance.character.data.dead || __instance.character.data.fullyPassedOut || __instance.character.data.passedOut)
                {
                    Plugin.Log.LogInfo($"[PeakShock] Ignored status effect shock ({statusType}) because player is dead or passed out.");
                    return;
                }
                int minShock = Plugin.MinShock.Value;
                int maxShock = Plugin.MaxShock.Value;
                const float damageThreshold = 0.01f;
                float damageReceivedBelowThreshold = 0f;
                if (amount < damageThreshold)
                {
                    // Accumulate small amounts of damage to trigger shock later
                    damageReceivedBelowThreshold += amount;
                    if (damageReceivedBelowThreshold > damageThreshold)
                    {
                        Plugin.Log.LogInfo($"[PeakShock] Accumulated damage {damageReceivedBelowThreshold} exceeds threshold, triggering shock.");
                        int intensity = Mathf.Clamp(Mathf.RoundToInt(damageReceivedBelowThreshold * maxShock), minShock, maxShock);
                        Task.Run(() => Plugin.ShockController.EnqueueShock(intensity, 1));
                        damageReceivedBelowThreshold = 0f; // Reset after triggering shock
                    }
                    return; // Ignore small amounts of damage
                }
                else
                {

                    if (CharacterAfflictions.STATUSTYPE.Hunger == statusType) return; // ignore hunger status effects
                    if (!ShockTypeEnabled.TryGetValue(statusType, out bool isEnabled) || !isEnabled)
                    {
                        Plugin.Log.LogInfo($"[PeakShock] Ignored status effect shock for {statusType} because it is disabled.");
                        return; // Ignore status effects that are not enabled
                    }
                    else
                    {
                        int intensity = Mathf.Clamp(Mathf.RoundToInt(amount * maxShock), minShock, maxShock);
                        Plugin.Log.LogInfo($"[PeakShock] Status effect {statusType} damage: {amount}, shock: {intensity}%");
                        Task.Run(() => Plugin.ShockController.EnqueueShock(intensity, 1));
                    }
                }
            }
        }
    }
}