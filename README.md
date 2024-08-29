# Boss Mod

<img align="right" width="150" height="150" src="/Data/icon.png">

Boss Mod (vbm) is a Dalamud plugin for FFXIV that provides boss fight radar, auto-rotation, cooldown planning, and AI. All of the its modules can be toggled individually. Support for it can be found in the [Puni.sh Discord server](https://discord.gg/punishxiv).

_Licensed under the terms of the ![BSD 3-Clause License](/LICENSE)_

# Radar

<img align="right" height="200" src="/Data/radar.png">

The radar is the main part of the plugin and the primary reason for its existence. It provides an on-screen window that contains area mini-map that shows player positions, boss position(s), various imminent AoEs, and other mechanics. This is useful because you don't have to remember what ability names mean and you can see exactly whether you're getting clipped by incoming AoE or not.

The radar module also provides the next mechanic(s) in text form, as well as hints for how to resolve the current mechanic from the perspective of the group and the player.
   
# Autorotation

<img align="right" height="300" src="/Data/autorotation_config.png">

For supported classes, the Autorotation module will execute a fully optimal rotation to the best of its ability. Individual job support is listed in the plugin. A small guide on using this can be found [here](https://github.com/awgil/ffxiv_bossmod/wiki/Using-Presets). The features include:

- Movement skills are not executed during parts of fight that require precise positioning
- oGCDs are queued to next free oGCD slot so GCDs are not delayed
- Preventing movement while casting that allows perfect slidecasting
- Ground-targeted abilities, are cast immediately and are queued properly
- Abilities select the "best" target automatically

# Cooldown Planner

<img align="right" height="200" src="/Data/cd_planner.png">

The CD Planner allows you to turn your autorotation configuration into a plan for a specific boss fight. For instance, during a boss fight, you can automatically cast a raid-wide mitigation ability right before the boss casts an AoE. 

All of the options from autorotations are supported in a CD planner, which include "tracks" for using role-based abilities, limit break, cooldowns, and more. These are all configured on a per-boss basis. A small guide on using this can be found [here](https://github.com/awgil/ffxiv_bossmod/wiki/Using-the-CD-Planner).

# AI

<img align="right" height="250" src="/Data/ai.png">

VBM's AI module was created to automate movement during boss fights. With the help of other plugins, entine dungeons can be completely automated, provided a module exist for each boss. 

The AI will move your character based on safe zones determined by a boss's module, which are also displayed on the radar. It also tries to keep you within range of the enemies you are attacking while you're in combat with them.

An example of a plugin that utilizies VBM's AI module is [AutoDuty](https://github.com/ffxivcode/AutoDuty), which is supported in the same [Discord server](https://discord.gg/punishxiv) that VBM is supported in.

<br />
<hr />

# Installation

Add `https://puni.sh/api/repository/veyn` to your plugin repositories and then search for `Boss Mod` in the Plugin Installer to install Boss Mod.

Settings can be accessed via the Plugin Installer or using the chat command `/vbm`.

# Getting help

When you've found a bug or think you have some issue with the plugin, please do the following:

1. Ask in [Discord](https://discord.gg/punishxiv): it might be a known issue or people might be able to help you quickly
2. Gather extra information to aid in investigating the issue:
   1. Set log level to "Debug" (type `/xldev`, select "Dalamud" -> "Set log level" -> "Debug")
   2. Start replay recording (type `/vbm r` and hit "Start Recording")
   3. Reproduce the issue
   4. Stop replay recording (hit "Stop Recording")
   5. Find the logs (typically at `C:\Users\username\AppData\Roaming\XIVLauncher\dalamud.log`)
   6. Find the replay (typically at `C:\Users\username\AppData\Roaming\XIVLauncher\pluginConfigs\BossMod\replays`)
3. Create a [new issue](https://github.com/awgil/ffxiv_bossmod/issues/new/choose) on GitHub, provide a detailed description of the problem (including steps to reproduce the issue), and attach the logs and replay 

**Do not** create GitHub issues to request new modules, ask questions, etc. Discord is much more convenient for these kinds of things.

**Do** create GitHub issues for very concrete bugs (if you have replays, logs, or an easy way to reproduce what is obviously a problem) or very specific feature requests (please discuss in Discord first to understand whether there's any interest in it).

# Contributing

One of the best ways to contribute to Boss Mod is by making modules for boss fights. If you are looking for which bosses don't have modules, you can look in the `/BossMod/Modules` folder of the repository. To make modules, it's suggested to follow the [Making a Module](https://github.com/awgil/ffxiv_bossmod/wiki/Making-a-Module) guide on the repo's wiki, as well as the [Making a Module: What kind of attacks exist?](https://github.com/awgil/ffxiv_bossmod/wiki/Making-a-Module:-What-kind-of-attacks-exist%3F) wiki entry. There are quite a few people in the Discord server who know how to make modules, so feel free to ask for help there.
