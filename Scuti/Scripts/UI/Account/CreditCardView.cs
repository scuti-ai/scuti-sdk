using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Scuti.UI
{
    public class CreditCardView : MonoBehaviour
    {
        public class CreditCardModel
        {
            public string name;
            public string number;
            public string cvv;
            public string date;
        }

        [SerializeField] Text titleByDefault;
        [SerializeField] Text cardholderName;
        [SerializeField] Text cardNumber;
        [SerializeField] Text cvv;
        [SerializeField] Text expirationDate;

        [SerializeField] bool isDefaultCard;

        
        public void Refresh(CreditCardModel creditCardInfo)
        {
            cardholderName.text = creditCardInfo.name;
            cardNumber.text = "************" + creditCardInfo.number;
            cvv.text = "***";
            expirationDate.text = creditCardInfo.date;

        }

    }
}

