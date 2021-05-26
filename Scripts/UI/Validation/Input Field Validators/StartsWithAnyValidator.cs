using UnityEngine;

namespace Scuti {
    public class StartsWithAnyValidator : InputFieldValidator {
        [SerializeField] string[] acceptableStarts;
        [SerializeField] string msgOnInvalid;
        [SerializeField] string msgOnValid;

        public override bool EvaluateInputField(string value) {
            foreach (var acceptable in acceptableStarts) {
                if (!value.StartsWith(acceptable)) {
                    SetInvalid(msgOnInvalid);
                    return false;
                }
            }
            SetValid();
            return true;
        }
    }
}
