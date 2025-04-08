using Newtonsoft.Json;
using System;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct PortraitSettings {
    public readonly PoseSettings normal;
    public readonly PoseSettings doingPoorly;
    public readonly PoseSettings doingWell;
    public readonly PoseSettings vibePower;

    public readonly string character;
    public readonly float x;
    public readonly float y;

    public PoseSettings GetPose(PoseType pose) {
        return pose switch {
            PoseType.Normal => normal,
            PoseType.DoingPoorly => doingPoorly,
            PoseType.DoingWell => doingWell,
            PoseType.VibePower => vibePower,
            _ => throw new ArgumentException($"Invalid pose: {pose}")
        };
    }
}
