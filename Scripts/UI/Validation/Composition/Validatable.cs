using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    [DisallowMultipleComponent]
    public class Validatable : MonoBehaviour {
        [SerializeField] Validator[] validators;
        public Validator[] Validators {
            get { return Validators; }
        }

        [Header("Input Validator")]
        [SerializeField] Text messageText;
        [SerializeField] string msgOnValid;
        [SerializeField] Color invalidColor;
        [SerializeField] Color validColor;

        private void Start() {
            foreach (var validator in validators)
                validator.OnSet += now => Refresh();
        }

        public bool IsValid {
            get {
                foreach (var validator in validators)
                    if (!validator.IsValid)
                        return false;
                return true;
            }
        }

        public void Refresh() {
            string message = string.Empty;
            foreach (var validator in validators) {
                if (!validator.IsValid)
                    message = validator.Message;
            }

            if (message.Trim().IsNullOrEmpty()) {
                messageText.text = msgOnValid;
                messageText.color = validColor;
            }
            else {
                messageText.text = message;
                messageText.color = invalidColor;
            }
        }
    }
}
