using UnityEngine;

namespace Scuti {
    public class EqualsAny : InputFieldValidator {
        [SerializeField] string[] acceptableValues;
        [SerializeField] string msgOnInvalid;
        [SerializeField] string msgOnValid;

        public override bool EvaluateInputField(string value) {
            foreach (var acceptable in acceptableValues) {
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
