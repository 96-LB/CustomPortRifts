using RhythmRift;
using UnityEngine;

namespace CustomPortRifts;


public static class CustomPortraits {
    public static string LevelId { get; set; }
    public static RRPerformanceLevel PerformanceLevel { get; set; }
    public static Sprite[] NormalSprites { get; private set; }
    public static Sprite[] PoorlySprites { get; private set; }
    public static Sprite[] WellSprites { get; private set; }
    public static Sprite[] VibePowerSprites { get; private set; }

    public static Sprite[] ActiveSprites => PerformanceLevel switch {
        RRPerformanceLevel.Awesome or RRPerformanceLevel.Amazing => WellSprites,
        RRPerformanceLevel.Poor or RRPerformanceLevel.Terrible or RRPerformanceLevel.GameOver => PoorlySprites,
        RRPerformanceLevel.VibePower => VibePowerSprites,
        _ => NormalSprites
    };

    public static RRPortraitView Portrait { get; set; }
    public static bool Enabled { get; set; }
    public static bool HasSprites => NormalSprites != null && NormalSprites.Length > 0;
    public static bool UsingCustomSprites => Enabled && HasSprites;
    
    public static void Reset() {
        LevelId = "";
        PerformanceLevel = RRPerformanceLevel.Normal;
        NormalSprites = null;
        PoorlySprites = null;
        WellSprites = null;
        VibePowerSprites = null;
    }

    public static void SetSprites(string levelId, Sprite[] normal, Sprite[] poorly, Sprite[] well, Sprite[] vibePower) {
        if(normal == null || normal.Length == 0) {
            throw new System.ArgumentException("Normal sprites must not be null or empty.", nameof(normal));
        }
        
        if(poorly == null || poorly.Length == 0) {
            throw new System.ArgumentException("DoingPoorly sprites must not be null or empty.", nameof(poorly));
        }

        if(well == null || well.Length == 0) {
            throw new System.ArgumentException("DoingWell sprites must not be null or empty.", nameof(well));
        }

        if(vibePower == null || vibePower.Length == 0) {
            throw new System.ArgumentException("VibePower sprites must not be null or empty.", nameof(vibePower));
        }

        LevelId = levelId;
        NormalSprites = normal;
        PoorlySprites = poorly;
        WellSprites = well;
        VibePowerSprites = vibePower;
    }
}
