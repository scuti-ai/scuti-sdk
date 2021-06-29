using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti {
    public class AgeValidator : Validator
    {
        public Dropdown YearDropDown;
        //public Dropdown MonthDropDown;
        //public Dropdown DayDropDown;

        private DateTime Date;
        public string InValidMessage;

        public int Age = 13;

        void Awake()
        {
            YearDropDown.onValueChanged.AddListener(state => Refresh());
            //MonthDropDown.onValueChanged.AddListener(state => Refresh());
            //DayDropDown.onValueChanged.AddListener(state => Refresh());
            Message = InValidMessage;
        }

        private void Refresh()
        {
            Date = new DateTime(int.Parse(YearDropDown.options[YearDropDown.value].text), 12, 29);
            Evaluate();
        }

        protected virtual void Evaluate()
        {
            var now = DateTime.Now;
            var diff = now.Year - Date.Year;
            if(diff>Age)
            {
                SetValid();
            } else  if(diff<Age)
            {
                SetInvalid(InValidMessage);
            }
            else //(diff == Age)
            {
                // check month
                if(now.Month > Date.Month)
                {
                    SetValid();
                } else if(now.Month < Date.Month)
                {
                    SetInvalid(InValidMessage);
                } else  // same month
                {
                    if (now.Day >= Date.Day)
                    {
                        SetValid();
                    } else
                    {
                        SetInvalid(InValidMessage);
                    }
                }
            }   
        }
    }
}
