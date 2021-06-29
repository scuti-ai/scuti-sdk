using System;

using UnityEngine;

namespace Scuti {
    [Serializable]
    public abstract class Selector<T> : View
    {
        public event Action<T> OnSubmit;
        public event Action OnCancel;

        protected T selection;

        protected abstract T GetDefaultSelection();
        protected abstract bool Evaluate();

        protected override void Awake() {
            base.Awake();
            selection = GetDefaultSelection();
        }

        public void Cancel() {
            OnCancel?.Invoke();
        }

        public void Submit() {
            if (!Evaluate()) {
                return;
            }

            OnSubmit?.Invoke(selection);
        }
    }
}
