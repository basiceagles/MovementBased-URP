using UnityEngine;

namespace SmallHedge.SoundManager
{
    public class PlayStateSound : StateMachineBehaviour
    {
        public enum TriggerMoment { OnEnter, OnExit, Both }

        [SerializeField] private SoundType sound;
        [SerializeField, Range(0, 1)] private float volume = 1f;
        [SerializeField] private TriggerMoment triggerWhen = TriggerMoment.OnEnter;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (triggerWhen == TriggerMoment.OnEnter || triggerWhen == TriggerMoment.Both)
            {
                SoundManager.PlaySound(sound, null, volume);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (triggerWhen == TriggerMoment.OnExit || triggerWhen == TriggerMoment.Both)
            {
                SoundManager.PlaySound(sound, null, volume);
            }
        }
    }
}
