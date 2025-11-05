using System;
using UnityEngine;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenCameraBehaviour : TweenAnimationBehaviour<Camera>
    {
        [SerializeField] FloatTweenParameter orthographicSize;
        [SerializeField] FloatTweenParameter fieldOfView;
        [SerializeField] ColorTweenParameter backgroundColor;

        public ReadOnlyTweenParameter<float> OrthographicSize => orthographicSize;
        public ReadOnlyTweenParameter<float> FieldOfView => fieldOfView;
        public ReadOnlyTweenParameter<Color> BackgroundColor => backgroundColor;

        public override void OnTweenInitialize(Camera playerData)
        {
            orthographicSize.SetInitialValue(playerData, playerData.orthographicSize);
            fieldOfView.SetInitialValue(playerData, playerData.fieldOfView);
            backgroundColor.SetInitialValue(playerData, playerData.backgroundColor);
        }

        public override void ApplyProgress(Camera binding, float progress)
        {
            if (orthographicSize.IsActive)
                binding.orthographicSize = orthographicSize.Evaluate(binding, progress);
            if (fieldOfView.IsActive)
                binding.fieldOfView = fieldOfView.Evaluate(binding, progress);
            if (backgroundColor.IsActive)
                binding.backgroundColor = backgroundColor.Evaluate(binding, progress);
        }

        public override void ApplyFinalState(Camera binding)
        {
            if (orthographicSize.IsActive)
                binding.orthographicSize = orthographicSize.Evaluate(binding, 1f);
            if (fieldOfView.IsActive)
                binding.fieldOfView = fieldOfView.Evaluate(binding, 1f);
            if (backgroundColor.IsActive)
                binding.backgroundColor = backgroundColor.Evaluate(binding, 1f);
        }
    }
}