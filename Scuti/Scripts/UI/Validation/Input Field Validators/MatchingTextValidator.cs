using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class MatchingTextValidator : InputFieldValidator {
        [SerializeField] InputField fieldToMatch;
        [SerializeField] string messageOnNotMatch;


        protected override void Awake()
        {
            base.Awake();
            switch (mode)
            {
                case ValidationMode.OnDoneEditing:
                    fieldToMatch.onEndEdit.AddListener(value =>
                        EvaluateInputField(inputField.text));
                    break;
                case ValidationMode.OnValueChanged:
                    fieldToMatch.onValueChanged.AddListener(value =>
                        EvaluateInputField(inputField.text));
                    break;
            }
            EvaluateInputField(inputField.text);
        }

        public override bool EvaluateInputField(string value) {

            Debug.LogError("Matching text?? " + fieldToMatch.text + " vs " + value);
            if (!fieldToMatch.text.Equals(value)) {
                Debug.LogError("invalid");
                SetInvalid(messageOnNotMatch);
                return false;
            }
            else {
                Debug.LogError("valid");
                SetValid();
                return false;
            }
        }
    }
}
