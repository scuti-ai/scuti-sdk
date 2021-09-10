using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scuti
{
    public class AddressData
    {

        public string Line1;
        public string Line2 = "";
        public string City;
        public string State;
        public string Zip;
        public string Phone;
        public string Country;

        public override string ToString()
        {

            var address2 = ", ";
            if (!string.IsNullOrEmpty(Line2)) address2 = ", " + Line2 + ", ";
            return $"{Line1}{address2}{City}, { State} { Zip}";
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Line1) && !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State) && !string.IsNullOrEmpty(Zip) && !string.IsNullOrEmpty(Country);
        }
    }
}