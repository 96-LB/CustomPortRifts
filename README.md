> ## ⚠️ As of 23 April 2025, this mod is no longer compatible with the game. Uninstall the mod if you're running into an infinite loading screen!
> The base game now supports custom portraits, so this repository exists only as a historical record, unless this mod gets a new update in the future. The base game implementation is backwards compatible with the mod, but there are a few differences. It's recommended that you use [this guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3487821958) for instructions, as some inaccuracies exist below. Enjoy!

# Custom PortRifts

This project is a mod for Rift of the NecroDancer which adds support for customizing character portraits. Players can choose to replace the characters which appear in a specific tracks, or alternatively change the sprites for a specific character across all tracks (including a quick toggle to use Cadence's outfit from Crypt of the NecroDancer). The mod follows the same specification that the base game uses for custom portraits in custom levels.

> ⚠️ BepInEx mods are <ins>**not officially supported**</ins> by Rift of the NecroDancer. If you encounter any issues with this mod, please open an issue on this GitHub repository, and do not submit reports to Brace Yourself Games! In order to prevent serious bugs, this mod will automatically disable itself when you update your game, and you will have to return here to download a new, compatible version.

The current version is <ins>**v0.2.2**</ins>. Downloads for the latest version can be found [here](https://github.com/96-LB/CustomPortRifts/releases/latest). The changelog can be found [here](Changelog.md).

## Installation

Custom PortRifts runs on BepInEx 5. In order to use this mod, you must first install BepInEx into your Rift of the NecroDancer. A more detailed guide can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html), but a summary is provided below. If BepInEx is already installed, you can skip the next subsection.

### Installing BepInEx
1. Navigate to the latest release of BepInEx 5 [here](https://github.com/BepInEx/BepInEx/releases).
    > ⚠️ This mod is only tested for compatibility with BepInEx 5. If the above link takes you to a version of BepInEx 6, check out [the full list of releases](https://github.com/BepInEx/BepInEx/releases).
2. Expand the "Assets" tab at the bottom and download the correct `.zip` file for your operating system.
   
    > ℹ️ For example, if you use 64-bit Windows, download `BepInEx_win_x64_5.X.Y.Z.zip`.
    
4. Extract the contents of the `.zip` file into your Rift of the NecroDancer game folder.
   
    > ℹ️ You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

6. If you're on Mac or Linux, configure Steam to run BepInEx when you launch your game. Follow the guide [here](https://docs.bepinex.dev/articles/advanced/steam_interop.html).

7. Run Rift of the NecroDancer to set up BepInEx.
    > ℹ️ If done correctly, your `BepInEx` folder should now contain several subfolders, such as `BepInEx/plugins`.

### Installing Custom PortRifts
1. Navigate to the latest release of Custom PortRifts [here](https://github.com/96-LB/CustomPortRifts/releases/latest).
   
   > ⚠️ Do NOT download the source code using the button at the top of this page. If you're downloading a `.zip`, you are at the wrong place. 

2. Expand the "Assets" tab at the bottom and download `CustomPortRifts.dll`.

3. Place `CustomPortRifts.dll` in the `BepInEx/plugins` directory inside the Rift of the NecroDancer game folder.

   > ℹ️ You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

4. Check that your mod is working by playing the [mod showcase](https://steamcommunity.com/sharedfiles/filedetails/?id=3450077451) level.

   > ⚠️ Loading times will be longer than usual, as custom portraits are only loaded when you play the level for the first time.

### Installing Rift of the NecroManager (highly recommended)

In order to configure the mod to your liking, you are strongly encouraged to additionally install [Rift of the NecroManager](https://github.com/96-LB/RiftOfTheNecroManager), which adds an in-game settings menu for mods. If you already have a mod manager installed, or you prefer manually editing your configuration files, you can skip this step. Detailed installation instructions can be found [here](https://github.com/96-LB/RiftOfTheNecroManager), but the process is the same as in the previous subsection.

## Usage

### Basic Setup
This mod works similarly to the game's official custom portrait feature. If you're not yet familiar with how to add custom portraits to workshop levels, you should first take a look at [this guide](https://steamcommunity.com/sharedfiles/filedetails/?id=3487821958). Custom PortRifts uses the same format and folder structure as detailed in the guide, but portraits will be placed in your game directory instead of your custom track directory.

To get started, navigate to the directory with your game executable (the same location where you created your BepInEx folder). Then, create a directory called `CustomPortRifts`. Within it, create two folders named `Tracks` and `Characters`. You should have the following structure:
```
RiftOfTheNecroDancer.exe
CustomPortRifts/
  Tracks/
    ...
  Characters/
    ...
```

### Reskins
Custom PortRifts comes with toggles to replace all instances of certain characters with variants. For instance, you can play with the 10th Anniversary Update portraits on all levels.

To modify any of these settings, it's recommended to have [Rift of the NecroManager](https://github.com/96-LB/RiftOfTheNecroManager) installed. In this case, you can simply navigate to the in-game mod settings menu and easily set your preferences. Changes will take effect immediately. If you would rather change your settings manually, navigate to `BepInEx/config/com.lalabuff.necrodancer.customportrifts.cfg` in your game directory, modify the text file directly, and restart your game.

Currently, the mod only supports three reskins:
- **Crypt Cadence**: Replaces all instances of Cadence with her costume from Crypt of the NecroDancer. Overrides the Supporter Upgrade skin.
- **Crypt NecroDancer**: Replaces all instances of cloaked NecroDancer with his costume from Crypt of the NecroDancer.
- **Burger NecroDancer**: Replace all instances of cloaked NecroDancer with his costume from Magic Ham.

### Track Overrides
TODO

### Character Overrides
TODO

> ⚠️ The below information is outdated information from an old version of the mod.
### Publishing levels with custom portraits
To add custom portraits to your custom level, you do not need the mod installed, but it's recommended to have in order to test that you've done it right.

1. Navigate to your custom track's folder in your file explorer.
   > ℹ️ You can find this folder by opening the official level editor, pressing 'Open', and then scrolling down to 'Open track directory'.

2. Inside the folder for your level, create a new folder called `CustomPortRifts`.

3. Inside the new folder, create subfolders called `Hero` and/or `Counterpart` to add custom portraits for the left and right side, respectively.

4. Inside the counterpart and/or hero folders, create subfolders called `Normal`, `DoingPoorly`, `DoingWell`, `VibePower`, and/or `NormalMiss`.
   > ℹ️ You don't need all of these, but at least one needs to have sprites inside of it for your custom portrait to load.

5. Add your custom sprites as `.png` files inside the corresponding subfolders. To use an animation, upload each individual frame as a separate `.png` file.
   > ℹ️ The frames should be in alphabetical order. See below for more details regarding animations.

   > ℹ️ Here's an example of what your custom track directory could look like:
   > ```
   > info.json
   > level_1.json
   > CustomPortRifts/
   >   Counterpart/
   >     DoingPoorly/
   >       angry00.png
   >     DoingWell/
   >       happy00.png
   >     Normal/
   >       stand00.png
   >       stand01.png
   >       stand02.png
   >   Hero/
   >     Normal/
   >       heroic00.png
   >     VibePower/
   >       superheroic00.png
   >     NormalMiss/
   >       heroicmiss00.png
   > ```

6. Publish your track to the Steam workshop using the official editor. Your portraits will automatically be attached.

That's it! If everything is properly configured, your track will have custom portraits for any players with the mod installed. Here are some important notes and caveats:

- The character you choose in the editor is what all players without the mod will see. It also affects which background is displayed when custom portraits are active.
- Every vanilla character's portrait is a little bit different in the codebase—the exact size, animation timings, and mask all vary. The mod uses Dove's portrait as a base for counterpart portraits because it has the most open mask—only the bottom of your sprite will be covered. This does mean that designing modified vanilla sprites isn't as simple as just drawing over the original sprites; the character might not look the same in Dove's portrait as they do in their own portrait. Future updates may make this more configurable.
- Sprites are scaled up to fill the context box, so no specific resolution is needed. If your sprite is small, however, it might look blurry. Aspect ratio is preserved when scaling up. It's recommended that you test out how the sprite looks in-game and modify it accordingly because it's very hard to predict just from your image file.
- Animations are created by loading the sprites in alphabetical order. The first sprite is the "resting" position which they'll hold for most of the beat. At the start of each beat, they quickly cycle through the rest of the sprites before returning to the first sprite. The second sprite lasts for 3/31ths of a beat, and all the remaining ones last for 2/31ths of a beat (these numbers were chosen to most closely match the vanilla portraits, which all vary a little bit). The exact timings may be configurable in future versions.
- Not all four pose subfolders are necessary. When a folder is missing, the mod will use the other folders to generate the animation. The resolution order is as follows: 
  - When vibe power is active: `VibePower`, `DoingWell`, `Normal`, `DoingPoorly`
  - When the player has less than 3/10 HP: `DoingPoorly`, `Normal`, `DoingWell`, `VibePower`
  - When the player has 80+ combo: `DoingWell`, `Normal`, `DoingPoorly`, `VibePower`
  - All other times: `Normal`, `DoingWell`, `DoingPoorly`, `VibePower`
- Missing overrides other animations for 1 beat whenever a note is missed.
- Voicelines are silenced when custom portraits are active. Future versions may provide the ability to add custom voicelines.
- To reduce load times, custom portraits are not reloaded when the map is replayed using the retry feature. If you modified your portrait, to see the changes you must first exit to the track selection menu and reopen the map.
- To update (or delete) your portrait on the workshop, just edit (or delete) the contents of the `CustomPortRifts` folder and re-upload your track to the workshop.
