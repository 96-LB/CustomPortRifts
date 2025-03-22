using RhythmRift;
using UnityEngine;

namespace CustomPortRifts;


public class Portrait {
    public static Portrait Hero { get; } = new();
    public static Portrait Counterpart { get; } = new();
    public static RRPerformanceLevel PerformanceLevel { get; set; }
    public static string LevelId { get; set; }
    public static bool Enabled { get; set; }

    public Sprite[] NormalSprites { get; private set; }
    public Sprite[] PoorlySprites { get; private set; }
    public Sprite[] WellSprites { get; private set; }
    public Sprite[] VibePowerSprites { get; private set; }
    public Sprite[] ActiveSprites => PerformanceLevel switch {
        RRPerformanceLevel.Awesome or RRPerformanceLevel.Amazing => WellSprites,
        RRPerformanceLevel.Poor or RRPerformanceLevel.Terrible or RRPerformanceLevel.GameOver => PoorlySprites,
        RRPerformanceLevel.VibePower => VibePowerSprites,
        _ => NormalSprites
    };
    public bool HasSprites => NormalSprites != null && NormalSprites.Length > 0;
    public bool UsingCustomSprites => Enabled && HasSprites;
    
    public static void Reset() {
        LevelId = "";
        PerformanceLevel = RRPerformanceLevel.Normal;
        Hero.NormalSprites = null;
        Hero.PoorlySprites = null;
        Hero.WellSprites = null;
        Hero.VibePowerSprites = null;
        Counterpart.NormalSprites = null;
        Counterpart.PoorlySprites = null;
        Counterpart.WellSprites = null;
        Counterpart.VibePowerSprites = null;
    }

    public void SetSprites(string levelId, Sprite[] normal, Sprite[] poorly, Sprite[] well, Sprite[] vibePower) {
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
