using UnityEngine;

namespace TweenPlayables
{
    public sealed class TweenTransformMixerBehaviour : TweenAnimationMixerBehaviour<Transform, TweenTransformBehaviour>
    {
        readonly Vector3ValueMixer positionMixer = new();
        readonly Vector3ValueMixer rotationMixer = new();
        readonly Vector3ValueMixer scaleMixer = new();
        readonly Vector3ValueMixer positionChangeMixer = new();
        readonly Vector3ValueMixer rotationChangeMixer = new();
        readonly Vector3ValueMixer scaleChangeMixer = new();

        public override void Blend(Transform binding, TweenTransformBehaviour behaviour, float weight, float progress)
        {
            positionMixer.TryBlend(behaviour.Position, binding, progress, weight);
            rotationMixer.TryBlend(behaviour.Rotation, binding, progress, weight);
            scaleMixer.TryBlend(behaviour.Scale, binding, progress, weight);

            positionChangeMixer.TryBlend(behaviour.PositionChange, binding, progress);
            rotationChangeMixer.TryBlend(behaviour.RotationChange, binding, progress);
            scaleChangeMixer.TryBlend(behaviour.ScaleChange, binding, progress);
        }

        public override void Apply(Transform binding)
        {
            positionMixer.TryApplyAndClear(binding, (x, binding) => binding.localPosition = x);
            rotationMixer.TryApplyAndClear(binding, (x, binding) => binding.localEulerAngles = x);
            scaleMixer.TryApplyAndClear(binding, (x, binding) => binding.localScale = x);

            positionChangeMixer.TryApplyAndClear(binding, (x, binding) => binding.localPosition = x);
            rotationChangeMixer.TryApplyAndClear(binding, (x, binding) => binding.localEulerAngles = x);
            scaleChangeMixer.TryApplyAndClear(binding, (x, binding) => binding.localScale = x);
        }
    }
}
