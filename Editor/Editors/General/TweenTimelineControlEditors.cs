using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;

namespace TweenPlayables.Editor
{
    [CustomTimelineEditor(typeof(TimelineControlTrack))]
    public sealed class TimelineControlTrackEditor : TweenAnimationTrackEditor
    {
        public override Color TrackColor => Styles.TimelineControlColor;
        public override Texture2D TrackIcon => Styles.TimelineControlIcon;
        public override string DefaultTrackName => "Timeline Control Track";
    }

    [CustomTimelineEditor(typeof(TimelineControlClip))]
    public sealed class TimelineControlClipEditor : TweenAnimationClipEditor
    {
        public override string DefaultClipName => "Timeline Control";
        public override Color ClipColor => Styles.TimelineControlColor;
        public override Texture2D ClipIcon => Styles.TimelineControlIcon;
    }

    [CustomPropertyDrawer(typeof(TweenTimelineControlBehaviour))]
    public sealed class TweenTimelineControlBehaviourDrawer : TweenAnimationBehaviourDrawer
    {
        static readonly string[] parameters = new string[]
        {
            "controlAction", "enableLoop", "enableFrameRangeLoop", "startFrame", "endFrame"
        };

        protected override IEnumerable<string> GetPropertyNames() => parameters;
    }
}