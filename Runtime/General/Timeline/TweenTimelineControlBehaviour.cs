using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace TweenPlayables
{
    [Serializable]
    public sealed class TweenTimelineControlBehaviour : TweenAnimationBehaviour<PlayableDirector>
    {
        [SerializeField] TimelineControlAction controlAction = TimelineControlAction.None;
        [SerializeField] bool enableLoop = false;
        [SerializeField] bool enableFrameRangeLoop = false;

        public TimelineControlAction ControlAction => controlAction;
        public bool EnableLoop => enableLoop;
        public bool EnableFrameRangeLoop => enableFrameRangeLoop;

        private double clipStartTime;

        public override void OnTweenStarted(PlayableDirector binding, TweenAnimationBehaviour<PlayableDirector> behaviour, Playable playable, FrameData info)
        {
            if (binding == null) return;

            var self = behaviour as TweenTimelineControlBehaviour;
            if (self == null) return;

            // 如果启用帧范围循环，记录开始时间
            if (self.enableFrameRangeLoop || self.controlAction == TimelineControlAction.FrameRangeLoop)
            {
                self.clipStartTime = binding.time;
            }

            // 设置循环模式
            if (self.enableLoop)
            {
                binding.extrapolationMode = DirectorWrapMode.Loop;
            }

            ExecuteControlAction(binding, self.controlAction);
        }

        public override void OnTweenFinished(PlayableDirector binding, TweenAnimationBehaviour<PlayableDirector> behaviour, Playable playable, FrameData info)
        {
            var self = behaviour as TweenTimelineControlBehaviour;
            if (self == null || binding == null) return;

            // 如果启用帧范围循环，跳回到开始时间
            if (self.enableFrameRangeLoop)
            {
                binding.time = self.clipStartTime;
                return;
            }

            // 如果启用循环且 Timeline 结束了，重新开始播放
            if (self.enableLoop && binding.state != PlayState.Playing)
            {
                binding.time = 0;
                binding.Play();
            }
        }

        private void ExecuteControlAction(PlayableDirector director, TimelineControlAction action)
        {
            switch (action)
            {
                case TimelineControlAction.None:
                    // 不执行任何操作
                    break;
                case TimelineControlAction.Play:
                    director.Play();
                    break;
                case TimelineControlAction.Pause:
                    director.Pause();
                    break;
                case TimelineControlAction.Stop:
                    director.Stop();
                    break;
                case TimelineControlAction.Resume:
                    if (director.state == PlayState.Paused)
                        director.Resume();
                    break;
                case TimelineControlAction.Loop:
                    director.extrapolationMode = DirectorWrapMode.Loop;
                    director.Play();
                    break;
                case TimelineControlAction.FrameRangeLoop:
                    enableFrameRangeLoop = true;
                    director.Play();
                    break;
            }
        }
    }

    public enum TimelineControlAction
    {
        None,
        Play,
        Pause,
        Stop,
        Resume,
        Loop,
        FrameRangeLoop
    }
}