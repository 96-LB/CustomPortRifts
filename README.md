# Custom PortRifts

This project is a mod for Rift of the NecroDancer which adds support for customizing character portraits. Players can choose to replace the characters which appear in a specific tracks, or alternatively change the sprites for a specific character across all tracks (including quick toggles to display certain variants of some characters). The mod follows the same specification that the base game uses for custom portraits in custom levels.

> ⚠️ BepInEx mods are <ins>**not officially supported**</ins> by Rift of the NecroDancer. If you encounter any issues with this mod, please open an issue on this GitHub repository, and do not submit reports to Brace Yourself Games! In order to prevent serious bugs, this mod will automatically disable itself when you update your game, and you will have to return here to download a new, compatible version.

The current version is <ins>**v1.0.2**</ins> and is compatible with Rift of the NecroDancer Patch 1.8.0 released on 18 September 2025. Downloads for the latest version can be found [here](https://github.com/96-LB/CustomPortRifts/releases/latest). The changelog can be found [here](Changelog.md).

## Installation

Custom PortRifts runs on BepInEx 5. In order to use this mod, you must first install BepInEx into your Rift of the NecroDancer. A more detailed guide can be found [here](https://docs.bepinex.dev/articles/user_guide/installation/index.html), but a summary is provided below. If BepInEx is already installed, you can skip the next subsection.

### Installing BepInEx
1. Navigate to the latest release of BepInEx 5 [here](https://github.com/BepInEx/BepInEx/releases).
    > ⚠️ This mod is only tested for compatibility with BepInEx 5. If the above link takes you to a version of BepInEx 6, check out [the full list of releases](https://github.com/BepInEx/BepInEx/releases).
2. Expand the 'Assets' tab at the bottom and download the correct `.zip` file for your operating system.
   
    > ℹ️ For example, if you use 64-bit Windows, download `BepInEx_win_x64_5.X.Y.Z.zip`.
    
4. Extract the contents of the `.zip` file into your Rift of the NecroDancer game folder.
   
    > ℹ️ You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

6. If you're on Mac or Linux, configure Steam to run BepInEx when you launch your game. Follow the guide [here](https://docs.bepinex.dev/articles/advanced/steam_interop.html).

7. Run Rift of the NecroDancer to set up BepInEx.
    > ℹ️ If done correctly, your `BepInEx` folder should now contain several subfolders, such as `BepInEx/plugins`.

### Installing Custom PortRifts
1. Navigate to the latest release of Custom PortRifts [here](https://github.com/96-LB/CustomPortRifts/releases/latest).
   
   > ⚠️ Do NOT download the source code using the button at the top of this page. If you're downloading a `.zip`, you are at the wrong place.

2. Expand the 'Assets' tab at the bottom and download `CustomPortRifts.dll`.

3. Place `CustomPortRifts.dll` in the `BepInEx/plugins` directory inside the Rift of the NecroDancer game folder.

   > ℹ️ You can find this folder by right clicking on the game in your Steam library and clicking 'Properties'. Then navigate to 'Installed Files' and click 'Browse'.

4. Check that your mod is working by launching the game and following the basic setup directions below!

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
Track overrides provide a way to replace the portraits for a specific level. To create a track override, create a folder in `CustomPortRifts/Tracks` with name equal to the ID of the track you would like to change the portraits for. Within it, add a `Counterpart` folder to replace the right character, and/or a `Hero` folder to replace the left character. Inside those folders, you can use the usual format for creating a custom portrait.

A sample folder might look like the following:
```
CustomPortRifts/
  Tracks/
    DLCOG02/
      Hero/
        ...
    RRDiscoDisaster/
      CounterPart/
        ...
      Hero/
        ...
```

Here's a full list of track IDs. Your folders should use the name in the second column. (on Windows, these are case-sensitive!)
> ⚠️ On Windows, these names are case-sensitive!

| Track Name  | Track ID |
| ------------- | ------------- |
| Amalgamaniac | RRAmalgamaniac |
| Baboosh | RRBaboosh |
| Brave the Harvester | RRReaper |
| Count Funkula | RRCountFunkula |
| Cryp2que | RRCryp2que |
| Disco Disaster | RRDiscoDisaster |
| Eldritch House | RREldritchHouse |
| Elusional | RRElusional |
| Final Fugue | RRFinalFugue |
| Glass Cages | RRGlassCages |
| Hallow Queen | RRHallowQueen |
| Hang Ten Heph | RRHangTenHeph |
| Heph's Mess | RRHephsMess |
| King's Ruse | RRDeepBlues |
| Matriarch | RRMatriarch |
| Morning Dove | RRMorningDove |
| Necro Sonata | RRNecroSonatica |
| Necropolis | RRNecropolis |
| Nocturning | RRNocturning |
| Om and On | RROmandOn |
| Overthinker | RROverthinker |
| Portamello | RRPortamello |
| Progenitor | RRProgenitor |
| RAVEVENGE | RRRavevenge |
| Rift Within | RRRiftWithin |
| She Banned | RRHarmonie |
| Spookhouse Pop | RRSpookhousePop |
| Suzu's Quest | RRSuzusQuest |
| Twombtorial | RRTwombtorial |
| Under the Thunder | RRThunder |
| Visualize Yourself | RRVisualizeYourself |
| What's In The Box? | RRMatron |
| **Super Meatboy DLC** | |
| Bootus Bleez | DLCApricot03 |
| Got Danged | DLCApricot02 |
| Slugger's Refrain | DLCApricot01 |
| **Celeste DLC** |  |
| Confronting Myself | DLCBanana04 |
| Reach for the Summit | DLCBanana03 |
| Resurrections | DLCBanana05 |
| Resurrections (dannyBstyle Remix) | DLCBanana01 |
| Scattered and Lost | DLCBanana02 |
| **Pizza Tower DLC** | |
| It's Pizza Time! | DLCCherry01 |
| The Death That I Deservioli | DLCCherry02 |
| Unexpectancy, Pt. 3 | DLCCherry03 |
| World Wide Noise | DLCCherry04 |
| **10th Anniversary Update** | |
| Crypteque | DLCOG02 |
| Fungal Funk | DLCOG07 |
| Power Cords | DLCOG06 |
| **Hatsune Miku DLC** | |
| Intergalactic Bound | DLCKiwi03 |
| Just 1dB Louder | DLCKiwi04 |
| M@GICAL☆CURE! LOVE ♥ SHOT! | DLCKiwi02 |
| MikuFiesta | DLCKiwi05 |
| Radiant Revival | DLCKiwi06 |
| Too Real | DLCKiwi01 |
| **Hololive DLC** | |
| Ahoy!! 我ら宝鐘海賊団☆ | DLCGuava04 |
| Bibbidiba | DLCGuava01 |
| Carbonated Love | DLCGuava05 |
| Play Dice! | DLCGuava03 |
| Reflect | DLCGuava02 |
| **Everhood DLC** | |
| Feisty Flowers | Eggplant02 |
| Powers Of Destruction | Eggplant05 |
| Revenge | Eggplant03 |
| The Final Battle | Eggplant01 |
| Why Oh You Are LOVE | Eggplant04 |
| **Monstercat DLC** | |
| Crab Rave | Mango03 |
| Final Boss | Mango01 |
| New Game | Mango02 |
| PLAY | Mango04 |
| Waiting For You | Mango05 |


To override the portraits for a workshop map, first find its Steam ID. You can identify this from the link to the workshop page (for example, the Tetoris map at [https://steamcommunity.com/sharedfiles/filedetails/?id=**3422450367**](https://steamcommunity.com/sharedfiles/filedetails/?id=3422450367) has ID `3422450367`). Then, prepend `ws` to it to get the name of the folder you should create (for example, Tetoris would use the folder `CustomPortRifts/Tracks/ws3422450367` for track overrides).

### Character Overrides
If you'd rather replace the portrait for a character across all tracks they appear in, you can instead use character overrides. To do this, create a new folder in `CustomPortRifts/Characters` with name equal to the ID of the character you would like to change the sprites for. Then use the usual custom portrait conventions to create your portrait inside of this folder.
> ⚠️ Do not make `Counterpart` or `Hero` directories when using character overrides—just place your portraits directly in the character folder.

A sample folder might look like the following:
```
CustomPortRifts/
  Characters/
    Cadence/
        ...
    Cherry/
        ...
```

Here's a list of all the base game character IDs you can override:
- Beastmaster
- Cadence
- Cadence_Supporter
- Coda
- Dove
- Harmonie
- Heph
- Matron
- Merlin
- NecrodancerBurger
- NecrodancerCloak
- Nocturna
- Queen
- Reaper
- Shopkeeper
- Suzu

There are a few DLC characters you can also override:
- Apricot (Meatboy)
- Banana (Madeline)
- Banana02 (Badeline)
- Cherry (Peppino)
- CadenceCrypt (10th Anniversary Cadence)

> ⚠️ Due to the changes in how the game handled DLC portraits after the 10th Anniversary update, it is **not possible** to use character overrides to replace any other portraits. Use track overrides instead, and take a look at the following section for further tips.

### Combining Track and Character Overrides

If you want to use the same character in many track (or character) overrides, you can use the character override feature to avoid duplicating your image files and wasting storage space. In order to take advantage of this, make sure you have both the 'Track Override' and 'Character Override' configuration options turned on (this is the default). Then, anywhere you can add a portrait, instead create a file called `portrait.json` with the following contents:
```
{"PortraitId": "ID_GOES_HERE"}
```
In place of `ID_GOES_HERE`, you can write any character ID to load them in place of the regular portrait. This can be either a character ID from the base game, or a new character ID you create yourself. To create a new character ID, just add a folder in your character override directory with the same name. This way, you can link multiple tracks or characters to the same set of files instead of copying them around. For example, consider the following directory structure:
```
CustomPortRifts/
  Characters/
    Teto/
        ...
    Tracks/
      DLCKiwi01/
        portrait.json    <=  {"PortraitID": "Teto"}
      DLCKiwi02/
        portrait.json
      DLCKiwi03/
        portrait.json
      DLCKiwi04/
        portrait.json
      DLCKiwi05/
        portrait.json
      DLCKiwi06/
        portrait.json
```
This allows you to replace Hatsune Miku with Kasane Teto in all of the Miku DLC tracks without needing six copies of Teto's portrait on your hard drive.
