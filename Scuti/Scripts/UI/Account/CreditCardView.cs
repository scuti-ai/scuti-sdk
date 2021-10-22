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
            public string id;
            public string name;
            public string number;
            public string cvv;
            public string date;
            public bool isDefault;
        }

        [SerializeField] Text titleByDefault;
        [SerializeField] Text cardholderName;
        [SerializeField] Text cardNumber;
        [SerializeField] Text cvv;
        [SerializeField] Text expirationDate;

        [SerializeField] bool isDefaultCard;

        private CreditCardModel creditCardInfo;

        public Action<CreditCardModel> onShowCardInfo;

        // ----------------------------------------------------------------------

        public void Refresh(CreditCardModel creditCardInfo)
        {
            this.creditCardInfo = creditCardInfo;

            cardholderName.text = creditCardInfo.name;
            cardNumber.text = "**** **** ****" + creditCardInfo.number;
            cvv.text = "***";
            expirationDate.text = creditCardInfo.date;

            if (creditCardInfo.isDefault)
                titleByDefault.text = "Credit card "+cardholderName.text + " (Default)";
            else
                titleByDefault.text = "Credit card " + cardholderName.text;
        }

        public string GetId()
        {
            return creditCardInfo.id;
        }

        public void EditAndShowCardInfo()
        {
            onShowCardInfo?.Invoke(creditCardInfo);
        }

        public void Hide()
        {
            gameObject.SetActive(false);        
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

    }
}

