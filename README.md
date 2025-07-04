# PeakShock

PeakShock is a mod for PEAK that connects supported shock devices (like PiShock and OpenShock) to in-game events. When you take damage or die in the game, your device will deliver a shock, adding a fun and challenging twist to your gameplay.

## Features
- Triggers real shocks based on in-game events (player death, injury, and more)
- Supports PiShock and OpenShock platforms
- You can enable or disable specific damage types
- Highly configurable to match your preferences

## Getting Started
1. Install the mod using a mod manager or manually.
2. Run the game once with the mod installed. This will generate the config file: `addzeey.PeakShock.cfg`.
3. Configure your device and preferences:
   - You can edit the config file manually (look for `addzeey.PeakShock.cfg` in your config folder), or use your mod manager's config editor.
   - Make sure to set the correct shock platform (PiShock or OpenShock).
   - Enable or disable specific damage types as you like.
4. Play PEAK and enjoy the extra challenge!

## Configuration File Explained
When you first run the game with PeakShock installed, a config file named `addzeey.PeakShock.cfg` will be generated in your config folder. This file controls how the mod interacts with your shock device and lets you customize your experience.

**All available config options:**

- **Shock Platform**
  - `Provider`: Choose which device to use (`PiShock` or `OpenShock`).

- **PiShock Settings**
  - `UserName`: Your PiShock username.
  - `APIKey`: Your PiShock API Key.
  - `ShareCode`: Your PiShock ShareCode.

- **OpenShock Settings**
  - `ApiUrl`: OpenShock API URL (default: `https://api.openshock.app`).
  - `DeviceId`: Your OpenShock Device ID.
  - `ApiKey`: Your OpenShock API Key.

- **Shock Intensity and Timing**
  - `MinShock`: Minimum shock intensity (1-100).
  - `MaxShock`: Maximum shock intensity (1-100).
  - `DeathShock`: Shock intensity on death (1-100).
  - `DeathDuration`: Shock duration on death (seconds).
  - `ShockCooldownSeconds`: Minimum seconds between shocks (prevents shock spam).

- **Shock Triggers**
  - `EnableInjuryShock`: Enable shock for Injury damage.
  - `EnablePoisonShock`: Enable shock for Poison damage.
  - `EnableColdShock`: Enable shock for Cold damage.
  - `EnableHotShock`: Enable shock for Hot/Fire damage.

You can edit this file manually with a text editor, or use your mod manager's config editor if available. Always save your changes before restarting the game.

If you have questions about a specific setting, check the comments in the config file or ask in the mod's support channels.

## Safety Notice
Use shock devices responsibly. Start with low settings and stop if you feel uncomfortable or take breaks.

## Warning
Do not share config files, as these will have your own private api keys in for the shock device platforms!
