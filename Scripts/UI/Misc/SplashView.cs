using System;
using System.Collections;
using UnityEngine;

namespace Scuti.UI {
    public class SplashView : View {
        [SerializeField] Animator animator;

        public float ShowDuration = 3f;
        private Action _callback;

        public void ShowSplash(Action callback) {
            if (_destroyed) return;
            _callback = callback;
            if (animator!=null) animator.enabled = true;
            Open();
            if (animator != null)
            {
                animator.Play("Start", () =>
                {
                    if (animator) animator.enabled = false;
                    Complete();
                });
            } else
            {
                StartCoroutine(DelayedClose());
            }
        }

        private IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(ShowDuration);
            Complete();
        }

        private void Complete()
        {
            if (_destroyed) return;
            Close();
            _callback?.Invoke();
        }
    }
}
