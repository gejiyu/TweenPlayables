using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    public sealed class TweenTimelineControlMixerBehaviour : TweenAnimationMixerBehaviour<PlayableDirector, TweenTimelineControlBehaviour>
    {
        public override void Blend(PlayableDirector binding, TweenTimelineControlBehaviour behaviour, float weight, float progress){}

        public override void Apply(PlayableDirector binding){}
    }
}