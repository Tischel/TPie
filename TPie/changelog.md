# 1.10.0.0
- Added support for Patch 7.1 and Dalamud API 11.

# 1.9.1.0
- Added setting to Nested Ring Elements so they can be activated by clicking instead of hovering.

# 1.9.0.0
- Added support for Dawntrail and Dalamud API 10.

# 1.8.2.0
- The Escape key can now be used to close a ring with a toggable keybind.
- Added a setting for rings with toggable keybinds to not execute the hovered action when closed.

# 1.8.1.0
- Added support for the "new" Dalamund Fonts API.

# 1.8.0.1
- Fixed various issues related to Gear Set elements.

# 1.8.0.0
- Added support for patch 6.5 and Dalamud API 9.

# 1.7.1.1
- Fixed crash if Wotsit is not installed.

# 1.7.1.0
- Added support for the Wotsit plugin:
    + You can search for rings with by name to go directly to its settings.
    + Only rings with a name will work.

- Added a Quick Settings shortcut:
    + Double right-clicking while a ring is opened will open the settings windows for that ring:
    + This can be disabled in the general settings.

- Improved interactions when editing ring items:
    + Selecting a different ring item when the edit window is opened will refresh the window with the newly selected item.

# 1.7.0.0
- Added support for Patch 6.3 and Dalamud Api8.

# 1.6.3.0
- Added a filter to only show acquired emotes when searching.
- Fixed some emotes not working.

# 1.6.2.0
- Fixed high quality icons for items.

# 1.6.1.0
- Fixed TPie not working properly with Penumbra v0.5.8.0 and their new Interface Collection.

# 1.6.0.0
- Added Emote as a ring item:
    + This is just a convenience feature to be able to add emotes without having to manually search for their icons.
    + It will simply use the command for the selected emote.
    + The plugin doesn't and won't know which emotes you have unlocked. Trying to use an unlocked emote won't work.
    
- Added a "Draw Text" setting to Game Macro and Command items.
- Added a "Draw Text Only When Selected" setting to Game Macro, Command and Gear Set items.
- The Keybind Edit Window will now focus the input field automatically when opened.
- Fixed ring preview overlapping with the settings window on high Dalamud Font Scales.

# 1.5.0.0
- Added support for Patch 6.2 and Dalamud Api7.

# 1.4.0.0
- Added setting to automatically move the cursor to the center of the rings when they have a fixed position.
- Keybinds can now be customized per job:
    + This means you can use the same keybind for different rings as long as they are tied to different jobs.
    + Added a new "Edit Keybind" window to support this new functionallity.

# 1.3.1.0
- Added buttons to sort rings in the list.
- Fixed quick action setting being transferred to another item when deleting or moving items.
- Fixed quick actions showing a selection background when the setting is disabled.
- Fixed crash when attempting to move an item without having a selected item.

# 1.3.0.0
- Updated for patch 6.1 support.

# 1.2.1.1
- Fixed "Keep Previous Ring Center" not working properly in some multi-monitor situations.

# 1.2.1.0
- Added setting for nested rings to keep it centered with the previous ring. 
    + This will move your cursor to the center automatically after activating a nested ring.
    + Enabled by default.

# 1.2.0.3
- Fixed quick actions executing when they shouldn't.
- Fixed ring settings window freezing when for rings with toggle mode activated.

# 1.2.0.2
- Fixed black square when opening a ring with Dalamud's multi-monitor setting enabled.

# 1.2.0.1
- Making it clearer that nested rings can't be used as quick actions (due to how nested rings are activated, it doesn't work well with quick actions).
- Fixed ring elements' labels cutting off.

# 1.2.0.0
- Added quick actions for rings:
    + Quick actions are placed on the center of a ring and can be executed even if the ring is not fully opened.
    + If the keybind is held, any of the ring elements can be chosen, including the quick action in the center.
    + On Toggle Mode, there's no way to activate a quick action without double tapping or tap & click.

# 1.1.1.0
- Added support for mouse wheel buttons to use as keybinds.

# 1.1.0.2
- Fixed save errors / crashes with nested rings.

# 1.1.0.1
- Nested Rings elements text can now be hidden.
- Fixed ring elements not being clickable on toggle mode.

# 1.1.0.0
- Added support for nested rings.
    + Nested Rings are activated by hovering on them for a set amount of time.
    + On Toggle Mode, Nested Rings can also be activated by clicking on them.
    + Nested Rings are linked by name. If there are multiple rings with the same name, one will be picked "randomly".
- Toggle Mode is now a keybind setting instead of a global one.
- Actions now show the remaining charges when applicable.
- Fixed actions cooldowns not calculating properly for actions with multiple charges.
- Fixed crash that could happen when creating a new ring.

# 1.0.0.0
- Added a rotation setting for rings.
- Added a setting to show tooltips when hovering over ring elements.
- Added global border settings for ring elements.

# 0.3.0.0
- Added a new type of element to execute the user-defined game macros.
- Old macro elements renamed to "Commmand". These will remain as single-lined text commands.
- Added more icons to the browse tab.
- Increased leniency so it's easier to select elements without needing much precision.
- Fixed rings showing the window border (usually caused by Dalamud themes).

# 0.2.0.1
- Fixed texture loading related crashes (textures will still not load when corrupted/invalid, but TPie shouldn't crash anymore).

# 0.2.0.0
- Added an icon browser.
- Added settings to hide the ring's selection line and background.
- Fixed high quality items showing when searching for normal items.

# 0.1.1.0
- Added keybind passthrough setting.
- Added keybind toggle mode setting.
- Fixed rings preview sometimes not working.

# 0.1.0.0
- Endwalker support (there might still be some issues with Reaper / Sage actions).

# 0.0.2.6
- Fixed gear sets names not saving properly.

# 0.0.2.5
- Fixed inventory items not working properly when having multiple stacks of the same item.

# 0.0.2.4
- Fixed some "weird" crashes.
- Fixed rings sometimes appearing on wrong positions when multiple keybinds are active at the same time.

# 0.0.2.3
- Fixed keybinds being active when the game window is not focused.

# 0.0.2.2
- Fixed being able to show 2 rings at the same time, which can lead to a crash.

# 0.0.2.1
- Fixed edit buttons showing on a ring with no elements which lead to a crash.

# 0.0.2.0
- Added border settings for ring elements.
- Added more settings for gear sets.
- Enabled "Caps Lock" as a valid keybind.
- TPie settings windows now support Dalamud's Global UI Scale.
- Fixed items not being automatically selected for edit when adding a new one while another one was already selected.

# 0.0.1.0
- First release