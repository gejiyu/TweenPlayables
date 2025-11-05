using System;
using UnityEngine;
using UnityEngine.Playables;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenTransformBehaviour : TweenAnimationBehaviour<Transform>
    {
        [SerializeField] Vector3TweenParameter position;
        [SerializeField] Vector3TweenParameter rotation;
        [SerializeField] Vector3TweenParameter scale;
        [SerializeField] Vector3ChangeParameter positionChange;
        [SerializeField] Vector3ChangeParameter rotationChange;
        [SerializeField] Vector3ChangeParameter scaleChange;

        public ReadOnlyTweenParameter<Vector3> Position => position;
        public ReadOnlyTweenParameter<Vector3> Rotation => rotation;
        public ReadOnlyTweenParameter<Vector3> Scale => scale;
        public ReadOnlyChangeParameter<Vector3> PositionChange => positionChange;
        public ReadOnlyChangeParameter<Vector3> RotationChange => rotationChange;
        public ReadOnlyChangeParameter<Vector3> ScaleChange => scaleChange;
        public override void OnTweenInitialize(Transform playerData)
        {
            position.SetInitialValue(playerData, playerData.localPosition);
            rotation.SetInitialValue(playerData, playerData.localEulerAngles);
            scale.SetInitialValue(playerData, playerData.localScale);
            positionChange.SetInitialValue(playerData, playerData.localPosition);
            rotationChange.SetInitialValue(playerData, playerData.localEulerAngles);
            scaleChange.SetInitialValue(playerData, playerData.localScale);
        }

        public override void OnTweenStarted(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (positionChange.IsActive && positionChange.scrambleMode == ChangeScrambleMode.Start)
                binding.localPosition = positionChange.ChangeValue;
            if (rotationChange.IsActive && rotationChange.scrambleMode == ChangeScrambleMode.Start)
                binding.localEulerAngles = rotationChange.ChangeValue;
            if (scaleChange.IsActive && scaleChange.scrambleMode == ChangeScrambleMode.Start)
                binding.localScale = scaleChange.ChangeValue;
        }

        public override void OnTweenFinished(Transform binding, TweenAnimationBehaviour<Transform> behaviour, Playable playable, FrameData info)
        {
            if (positionChange.IsActive && positionChange.scrambleMode == ChangeScrambleMode.End)
                binding.localPosition = positionChange.ChangeValue;
            if (rotationChange.IsActive && rotationChange.scrambleMode == ChangeScrambleMode.End)
                binding.localEulerAngles = rotationChange.ChangeValue;
            if (scaleChange.IsActive && scaleChange.scrambleMode == ChangeScrambleMode.End)
                binding.localScale = scaleChange.ChangeValue;
        }

        public override void ApplyProgress(Transform binding, float progress)
        {
            if (position.IsActive)
                binding.localPosition = position.Evaluate(binding, progress);
            if (rotation.IsActive)
                binding.localEulerAngles = rotation.Evaluate(binding, progress);
            if (scale.IsActive)
                binding.localScale = scale.Evaluate(binding, progress);
        }

        public override void ApplyFinalState(Transform binding)
        {
            if (position.IsActive)
                binding.localPosition = position.Evaluate(binding, 1f);
            if (rotation.IsActive)
                binding.localEulerAngles = rotation.Evaluate(binding, 1f);
            if (scale.IsActive)
                binding.localScale = scale.Evaluate(binding, 1f);
        }
    }
}