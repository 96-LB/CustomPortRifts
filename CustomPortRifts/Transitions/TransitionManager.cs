using System;

namespace CustomPortRifts.Transitions;


public class TransitionManager<T> {
    public (Transition<T> Transition, Action<T> Callback)? Transition { get; private set; } = null;
    public T? EndState { get; private set; } = default;
    public bool IsTransitioning => Transition != null;

    public void StartTransition(Transition<T> transition, Action<T> callback) {
        Update(float.PositiveInfinity); // immediately apply any pending transition
        Transition = (transition, callback);
        EndState = transition.EndState;
    }

    public void Update(float beat) {
        if(Transition == null) {
            return;
        }

        var (transition, callback) = Transition.Value;
        var progress = transition.BeatToProgress(beat);
        if(progress > 0) {
            var state = transition.Interpolate(progress);
            callback?.Invoke(state);
            if(progress >= 1) {
                Transition = null;
            }
        }
    }
}
