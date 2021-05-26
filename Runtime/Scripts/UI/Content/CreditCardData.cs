using Scuti.GraphQL.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scuti
{
    public class CreditCardData
    {

        public string Name;
        public string Number;
        public int ExpirationMonth;
        public int ExpirationYear;
        public string CVV;
        public string CardType;
        public bool MakeDefault;
        public bool SaveCard;
        public EncryptedInput Encrypted;


        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Number) && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(CVV) && !string.IsNullOrEmpty(CardType);
        }

        internal void Reset()
        {
            Encrypted = null;
            CVV = "";
            ExpirationMonth = DateTime.Now.Month;
            ExpirationYear = DateTime.Now.Year;
            MakeDefault = true;
            Name = "";
            Number = "";
            CardType = "";
        }
    }
}