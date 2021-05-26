using UnityEngine;

namespace Scuti {
    public class EndWithAnyValidator : InputFieldValidator {
        [SerializeField] string[] acceptableEnds;
        [SerializeField] string msgOnInvalid;
        [SerializeField] string msgOnValid;

        public override bool EvaluateInputField(string value) {
            foreach (var acceptable in acceptableEnds) {
                if (!value.EndsWith(acceptable)) {
                    SetInvalid(msgOnInvalid);
                    return false;
                }
            }
            return true;
        }
    }
}
