using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    [RequireComponent(typeof(Validatable))]
    public abstract class InputFieldValidator : Validator {
        public enum ValidationMode {
            OnDoneEditing,
            OnValueChanged
        }

        [BoxGroup("Input Field Validator")] [SerializeField] protected InputField inputField;
        [BoxGroup("Input Field Validator")] [SerializeField] protected ValidationMode mode;

        protected virtual void Awake() {
            switch (mode) {
                case ValidationMode.OnDoneEditing:
                    inputField.onEndEdit.AddListener(value =>
                        EvaluateInputField(inputField.text));
                    break;
                case ValidationMode.OnValueChanged:
                    inputField.onValueChanged.AddListener(value =>
                        EvaluateInputField(inputField.text));
                    break;
            }
            EvaluateInputField(inputField.text);
        }

        public abstract bool EvaluateInputField(string value);
    }
}
