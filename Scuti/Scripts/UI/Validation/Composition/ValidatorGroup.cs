using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Scuti {
    public class ValidatorGroup : MonoBehaviour {
        [SerializeField] GameObject[] subjects;

        public bool Evaluate() {
            bool flag = true;

            foreach (var subject in subjects) {
                var validatable = subject.GetComponent<Validatable>();
                if (validatable == null) continue;

                if (!validatable.IsValid) {
                    validatable.Refresh();
                    flag = false;
                }
            }

            return flag;
        }
    }
}
