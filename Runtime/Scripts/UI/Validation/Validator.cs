using System;

using UnityEngine;

namespace Scuti {
    public abstract class Validator : MonoBehaviour {
        public event Action<bool> OnChange;
        public event Action<bool> OnSet;

        [SerializeField] [ReadOnly] string message;
        public string Message {
            get { return message; }
            protected set { message = value; }
        }

        [SerializeField] [ReadOnly] bool isValid;
        public bool IsValid {
            get { return isValid; }
            private set { isValid = value; }
        }

        protected void SetValid() {
            Message = string.Empty;

            bool wasInvalid = !IsValid;
            IsValid = true;
            if (wasInvalid)
                OnChange?.Invoke(true);
            OnSet?.Invoke(true);
        }

        protected void SetInvalid(string message) {
            Message = message;

            bool wasValid = IsValid;
            IsValid = false;
            if (wasValid)
                OnChange?.Invoke(false);
            OnSet?.Invoke(false);
        }
    }
}
