using UnityEngine;

namespace Scuti {
    public class ContainsSubstringsValidator : InputFieldValidator {
        [SerializeField] [ReorderableList] string[] requiredSubstrings;
        [SerializeField] string msgOnInvalid;

        public override bool EvaluateInputField(string value) {
            foreach (var substring in requiredSubstrings) {
                if (!value.Contains(substring)) {
                    SetInvalid(msgOnInvalid);
                    return false;
                }
            }
            SetValid();
            return true;
        }
    }
}
