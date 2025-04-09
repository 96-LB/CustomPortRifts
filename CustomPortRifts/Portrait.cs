using CustomPortRifts.Settings;
using RhythmRift;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace CustomPortRifts;


public class Portrait {
    public static Portrait Hero { get; } = new();
    public static Portrait Counterpart { get; } = new();
    public static Texture2D Particles { get; private set; }
    public static LevelSettings Settings { get; private set; } = new();
    public static PoseType Pose { get; private set; }
    public static string LevelId { get; set; }
    public static bool Loading { get; set; }

    public Sprite[] NormalSprites { get; private set; }
    public Sprite[] PoorlySprites { get; private set; }
    public Sprite[] WellSprites { get; private set; }
    public Sprite[] VibePowerSprites { get; private set; }
    public Sprite[] ActiveSprites => Pose switch {
        PoseType.Normal => NormalSprites,
        PoseType.DoingPoorly => PoorlySprites,
        PoseType.DoingWell => WellSprites,
        PoseType.VibePower => VibePowerSprites,
        _ => NormalSprites // should never happen
    };
    public bool HasSprites => NormalSprites != null && NormalSprites.Length > 0;

    public static void Reset() {
        Hero.NormalSprites = null;
        Hero.PoorlySprites = null;
        Hero.WellSprites = null;
        Hero.VibePowerSprites = null;
        Counterpart.NormalSprites = null;
        Counterpart.PoorlySprites = null;
        Counterpart.WellSprites = null;
        Counterpart.VibePowerSprites = null;
        Particles = null;
        Pose = PoseType.Normal;
        LevelId = "";
    }

    public static void ResetSettings() {
        Settings = new();
    }

    public static async Task LoadSettings(string file) {
        var settingsText = await File.ReadAllTextAsync(file);
        var serializerSettings = new JsonSerializerSettings {
            Error = (sender, args) => {
                if(args.CurrentObject == args.ErrorContext.OriginalObject) {
                    Plugin.Log.LogError($"Encountered error while deserializing settings:\n{args.ErrorContext.Error.Message}");
                }
                args.ErrorContext.Handled = true;
            }
        };

        try {
            Settings = JsonConvert.DeserializeObject<LevelSettings>(settingsText, serializerSettings);
        } catch(Exception e) {
            Plugin.Log.LogError($"Failed to load settings:\n{e}");
            Settings = new();
            return;
        }
        Plugin.Log.LogMessage(JsonConvert.SerializeObject(Settings, Formatting.Indented));
    }

    public static async Task<Texture2D> LoadImage(string file) {
        if(File.Exists(file)) {
            try {
                var bytes = await File.ReadAllBytesAsync(file);
                var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture.LoadImage(bytes);
                return texture;
            } catch(Exception e) {
                Plugin.Log.LogError($"Failed to load image from {file}: {e}");
            }
        } else {
            Plugin.Log.LogError($"Attempted to load image from file {file}, which does not exist.");
        }
        return null;
    }

    public static async Task LoadParticles(string file) {
        Particles = await LoadImage(file);
    }

    public static async Task<Sprite[]> LoadPose(string dir, string pose) {
        var fullDir = Path.Combine(dir, pose);
        List<Sprite> sprites = [];
        if(Directory.Exists(fullDir)) {
            var files = Directory.GetFiles(fullDir, "*.png");
            Array.Sort(files);
            foreach(var file in files) {
                try {
                    var texture = await LoadImage(file);

                    // TODO: remove this
                    // simulates 'Alpha Is Transparency' import setting
                    // https://stackoverflow.com/a/77746375
                    var pixels = texture.GetPixels32();
                    for(int i = 0; i < pixels.Length; i++) {
                        if(pixels[i].a <= 1e-6) {
                            pixels[i] = new();
                        }
                    }
                    texture.SetPixels32(pixels);
                    texture.Apply();

                    var sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(0.5f, 0.5f));
                    sprites.Add(sprite);
                    Plugin.Log.LogInfo($"Loaded sprite from {Path.Join(Path.GetFileName(dir), Path.GetRelativePath(dir, file))}");
                } catch(Exception e) {
                    Plugin.Log.LogError($"Failed to load sprite from {file}: {e}");
                }
            }
        }
        return sprites.Count > 0 ? [.. sprites] : null;
    }

    public static PoseType SetPerformanceLevel(RRPerformanceLevel level) {
        return Pose = level switch {
            RRPerformanceLevel.Awesome or RRPerformanceLevel.Amazing => PoseType.DoingWell,
            RRPerformanceLevel.Poor or RRPerformanceLevel.Terrible or RRPerformanceLevel.GameOver => PoseType.DoingPoorly,
            RRPerformanceLevel.VibePower => PoseType.VibePower,
            _ => PoseType.Normal
        };
    }

    public async Task LoadSprites(string dir) {
        var normalSprites = await LoadPose(dir, "Normal");
        var poorlySprites = await LoadPose(dir, "DoingPoorly");
        var wellSprites = await LoadPose(dir, "DoingWell");
        var vibePowerSprites = await LoadPose(dir, "VibePower");

        normalSprites ??= wellSprites ?? poorlySprites ?? vibePowerSprites;
        wellSprites ??= normalSprites;
        poorlySprites ??= normalSprites;
        vibePowerSprites ??= wellSprites;

        SetSprites(normalSprites, poorlySprites, wellSprites, vibePowerSprites);
    }

    public void SetSprites(Sprite[] normal, Sprite[] poorly, Sprite[] well, Sprite[] vibePower) {
        if(normal == null || normal.Length == 0) {
            throw new ArgumentException("Normal sprites must not be null or empty.", nameof(normal));
        }
        
        if(poorly == null || poorly.Length == 0) {
            throw new ArgumentException("DoingPoorly sprites must not be null or empty.", nameof(poorly));
        }

        if(well == null || well.Length == 0) {
            throw new ArgumentException("DoingWell sprites must not be null or empty.", nameof(well));
        }

        if(vibePower == null || vibePower.Length == 0) {
            throw new ArgumentException("VibePower sprites must not be null or empty.", nameof(vibePower));
        }

        NormalSprites = normal;
        PoorlySprites = poorly;
        WellSprites = well;
        VibePowerSprites = vibePower;
    }
}
