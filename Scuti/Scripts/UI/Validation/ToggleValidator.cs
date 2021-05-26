using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class ToggleValidator : Validator {
        [Header("Toggle Validator")]
        [SerializeField] Toggle toggle;
        [SerializeField] bool desiredState;
        [SerializeField] string msgOnInvalid;

        void Awake() {
            Evaluate();

            toggle.onValueChanged.AddListener(state => Evaluate());
        }

        void Evaluate() {
            if (toggle.isOn == desiredState)
                SetValid();
            else
                SetInvalid(msgOnInvalid);
        }
    }
}
