using UnityEngine;

namespace Scuti.UISystem {
    [RequireComponent(typeof(CanvasGroup))]
    public class OpacityTransition : Transition {
        [BoxGroup("Opacity Transition")]
        [Required]
        public CanvasGroup group;

        [BoxGroup("Opacity Transition")]
        public float m_FadeInDuration = .2f;

        [BoxGroup("Opacity Transition")]
        public float m_FadeOutDuration = .2f;

        protected override void OnEndTransitionIn() {

            group.alpha = 1;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        protected override void OnEndTransitionOut() {
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        protected override void OnStartTransitionIn() {
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        protected override void OnStartTransitionOut() {
            group.alpha = 1;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public override float GetInDuration()
        {
            return m_FadeInDuration;
        }

        public override float GetOutDuration()
        {
            return m_FadeOutDuration;
        }

        protected override bool TransitionInOverTime() {
            group.alpha = Mathf.MoveTowards(group.alpha, 1, Time.deltaTime * (1 / m_FadeInDuration));
            return group.alpha > .9f;
        }

        protected override bool TransitionOutOverTime() {
            group.alpha = Mathf.MoveTowards(group.alpha, 0, Time.deltaTime * (1 / m_FadeOutDuration));
            return group.alpha < .1f;
        }
    }
}
