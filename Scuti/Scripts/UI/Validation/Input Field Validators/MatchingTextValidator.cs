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

            if (!fieldToMatch.text.Equals(value)) {
                SetInvalid(messageOnNotMatch);
                return false;
            } 
            else {  
                SetValid();
                return false;
            }
        }

        public void SetTextInput(string text)
        {
            inputField.text = text;
        }

    }
}
