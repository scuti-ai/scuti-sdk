using System;
using UnityEngine;

namespace Scuti {
    public class InputMonthYearValidator : InputFieldValidator {

        [BoxGroup("Messages")] [SerializeField] string msgOnInvalid;
        [BoxGroup("Messages")] [SerializeField] string msgExpired;
        public int Month { get; private set; }
        public int Year { get; private set; }

        public Action OnUpdated;

        public override bool EvaluateInputField(string expiration) {

            bool valid = false;
            Month = 0;
            Year = 0;
            string message = msgOnInvalid;
            if (expiration.Length == 5)
            {
                var split = expiration.Split('/');
                if (split.Length == 2)
                {
                    Month = Convert.ToInt32(split[0]);
                    Year = Convert.ToInt32("20" + split[1]);
                    if (Month > 0 && Month < 13)
                    {
                        if (Year >= DateTime.Now.Year)
                        {
                            valid = true;
                        } else
                        {
                            message = msgExpired;
                        }
                    }
                }
            }

            if (valid)
            {
                SetValid();
            }
            else
            {
                SetInvalid(message);
            }
            OnUpdated?.Invoke();
            return valid;
        }
    }
}
