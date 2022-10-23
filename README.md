This plugin consists of several interrelated parts which can be enabled or disabled independently.

# Boss modules ("radar")

Main part of the plugin and its reason of existence. It's an on-screen window that contains area map (kind of like a standard minimap)
and shows player's position, boss position, various imminent aoes and other mechanics.
This is useful mainly because (a) you don't have to remember what a million of ability names mean (think aureole/lateral aureole on hydaelyn),
(b) you can see exactly whether you're getting clipped by incoming aoe or not.

Above the map, the window contains several lines of text (these can be individually disabled in config if desired):
1. Next mechanic, time until it happens, plus other imminent mechanics that are part of the same "group" (a set of mechanics that resolve one after another and need to be considered as a whole).
All mechanic names are descriptive (e.g. "tankbuster", "raidwide") rather than flavourful (in-game ability names) to reduce cognitive load.
2. Sequence of future mechanics.
3. Global hints - short message indicating how to resolve current mechanic (e.g. "Spread")
4. Player hints - short message indicating what player should do; this is color-coded: yellow if player is at risk of failing, green if everything is ok and player should continue to do what he is doing.

For some mechanics that require precise positioning, plugin can optionally draw an arrow in main game viewport on top of 3D scene - this is enabled by "Show movement hints in world" config option.

# Autorotation (experimental / work-in-progress)

An extension of ideas from XIVCombo and MOAction plugins. For supported classes, execute full optimal rotation by pressing single button, at least when there is only one optimal decision.
Currently supports WAR (well) and WHM (not so well). Some features:
1. "Movement" skills (e.g. Primal Rend & Onslaught for WAR) that are part of rotation are not executed during parts of fight that require precise positioning
(or not executed automatically at all, depending on config).
2. OGCD cooldowns are queued to next free ogcd slot, so that GCDs are not delayed.
3. For casters, there is a mode that prevents movement while casting - this allows performing perfect slide-casting by just holding WSAD and spamming rotation button.
4. Ground-targeted abilities, depending on configuration, are cast immediately (on target or cursor positioning) and are queued properly.
5. Abilities select "best" target automatically (examples: target of nascent flash is selected target if friendly, otherwise mouseover target if friendly, otherwise other tank).

# Cooldown planner

Allows creating a plan (e.g. "Cast Vengeance right before this particular tankbuster") which is then executed automatically while spamming rotation button.

This is still work-in-progress, in future it should allow creating cooldown plans for whole raid and sharing.

# Install instructions:
1. esc -> dalamud settings -> experimental -> custom plugin repositories, add https://raw.githubusercontent.com/awgil/ffxiv_plugin_distribution/master/pluginmaster.json
2. esc -> dalamud plugins -> all plugins -> find "Boss Mod" and install
3. settings are accessible either via esc menu or /vbm console command

# Getting help

When you've found a bug or think you have some issue with the plugin, please do the following:
1. Set log level to 'debug' (type /xldev, select Dalamud -> Set log level -> Debug), reproduce the issue and find logs (typically found at C:\Users\username\AppData\Roaming\XIVLauncher\dalamud.log).
2. Create a new issue in github, provide a detailed description (ideally include steps to reproduce the issue) and attach logs.
