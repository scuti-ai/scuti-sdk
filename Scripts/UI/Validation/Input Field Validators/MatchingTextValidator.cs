using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class MatchingTextValidator : InputFieldValidator {
        [SerializeField] InputField fieldToMatch;
        [SerializeField] string messageOnNotMatch;

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
    }
}
