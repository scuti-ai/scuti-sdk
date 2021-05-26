using UnityEngine;

namespace Scuti {
    public class InputLengthValidator : InputFieldValidator {
        [BoxGroup("Constraints")] [SerializeField] int minLength;
        [BoxGroup("Constraints")] [SerializeField] int maxLength = int.MaxValue;

        [BoxGroup("Messages")] [SerializeField] string msgOnNoValue;
        [BoxGroup("Messages")] [SerializeField] string msgOnShortValue;
        [BoxGroup("Messages")] [SerializeField] string msgOnLongValue;

        public override bool EvaluateInputField(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                SetInvalid(msgOnNoValue);
                return false;
            }
            else if (input.Length < minLength) {
                SetInvalid(msgOnShortValue);
                return false;
            }
            else if (input.Length > maxLength) {
                SetInvalid(msgOnLongValue);
                return false;
            }
            else {
                SetValid();
                return true;
            }
        }
    }
}
