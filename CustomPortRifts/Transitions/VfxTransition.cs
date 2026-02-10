using RiftOfTheNecroManager;
using Shared;
using Shared.TrackData;
using UnityEngine;

namespace CustomPortRifts.Transitions;


public class VfxData(LocalTrackVfxConfig config, Texture2D? particleTexture) {
    public LocalTrackVfxConfig Config { get; } = config;
    public Texture2D? ParticleTexture { get; } = particleTexture;
}

public class VfxTransition(RiftFXColorConfig oldVfx, VfxData vfxData, float startBeat, float duration) : Transition<RiftFXColorConfig>(startBeat, duration) {
    public bool HasNewTexture { get; } = vfxData.ParticleTexture != null && vfxData.ParticleTexture != oldVfx.CustomParticleMaterial.GetTexture("_Texture2D");
    public FadeTransition Fade { get; } = new(startBeat, duration);
    public override RiftFXColorConfig Interpolate(float t) {
        var vfx = Object.Instantiate(oldVfx);
        var newVfx = vfxData.Config;
        vfx.CoreStartColor1 = oldVfx.CoreStartColor1.Lerp(newVfx.CoreStartColor1, t);
        vfx.CoreStartColor2 = oldVfx.CoreStartColor2.Lerp(newVfx.CoreStartColor2, t);
        vfx.SpeedlinesStartColor = oldVfx.SpeedlinesStartColor.Lerp(newVfx.SpeedlinesStartColor, t);
        vfx.CoreColorOverLifetime = oldVfx.CoreColorOverLifetime.Lerp(newVfx.CoreColorOverLifetime, t);
        vfx.SpeedlinesColorOverLifetime = oldVfx.SpeedlinesColorOverLifetime.Lerp(newVfx.SpeedlinesColorOverLifetime, t);

        vfx.RiftGlowColor = Color.Lerp(oldVfx.RiftGlowColor, newVfx.RiftGlowColor ?? oldVfx.RiftGlowColor, t);
        vfx.StrobeColor1 = Color.Lerp(oldVfx.StrobeColor1, newVfx.StrobeColor1 ?? oldVfx.StrobeColor1, t);
        vfx.StrobeColor2 = Color.Lerp(oldVfx.StrobeColor2, newVfx.StrobeColor2 ?? oldVfx.StrobeColor2, t);

        vfx.CustomParticleColor1 = oldVfx.CustomParticleColor1.Lerp(newVfx.CustomParticleColor1, t);
        vfx.CustomParticleColor2 = oldVfx.CustomParticleColor2.Lerp(newVfx.CustomParticleColor2, t);
        vfx.CustomParticleColorOverLifetime = oldVfx.CustomParticleColorOverLifetime.Lerp(newVfx.CustomParticleColorOverLifetime, t);
        vfx.BackgroundMaterial = oldVfx.BackgroundMaterial;
        vfx.CustomParticleMaterial = oldVfx.CustomParticleMaterial;
        vfx.CustomParticleSheetSize = oldVfx.CustomParticleSheetSize;

        var oldMat = vfx.BackgroundMaterial;
        if(oldMat) {
            var newMat = new Material(oldMat);
            newVfx.BackgroundColor1?.Pipe(x => newMat.SetColor("_TopColor", Color.Lerp(oldMat.GetColor("_TopColor"), x, t)));
            newVfx.BackgroundColor2?.Pipe(x => newMat.SetColor("_BottomColor", Color.Lerp(oldMat.GetColor("_BottomColor"), x, t)));
            newVfx.BackgroundGradientIntensity?.Pipe(x => newMat.SetFloat("_GradientIntensity", Mathf.Lerp(oldMat.GetFloat("_GradientIntensity"), x, t)));
            newVfx.BackgroundAdditiveIntensity?.Pipe(x => newMat.SetFloat("_AdditiveIntensity", Mathf.Lerp(oldMat.GetFloat("_AdditiveIntensity"), x, t)));
            vfx.BackgroundMaterial = newMat;
        }

        if(HasNewTexture) {
            if(t >= 0.5f) {
                vfx.CustomParticleMaterial = new Material(vfx.CustomParticleMaterial);
                vfx.CustomParticleMaterial.SetTexture("_Texture2D", vfxData.ParticleTexture);
                var x = newVfx.CustomParticleSheetWidth ?? oldVfx.CustomParticleSheetSize?.x ?? 2;
                var y = newVfx.CustomParticleSheetHeight ?? oldVfx.CustomParticleSheetSize?.y ?? 2;
                vfx.CustomParticleSheetSize = new(x, y);
            }

            // fade out particles during transition
            vfx.CustomParticleColorOverLifetime = vfx.CustomParticleColorOverLifetime.Lerp(new Color(1, 1, 1, 0), Fade.Interpolate(t));
        }

        return vfx;
    }
}
