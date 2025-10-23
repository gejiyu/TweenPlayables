using System;
using UnityEngine;

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
    }
}