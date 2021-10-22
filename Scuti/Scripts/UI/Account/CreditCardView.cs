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
            public string scheme;
            public string number;
            public string cvv;
            public string date;
            public bool isDefault;
        }

        [SerializeField] Text titleByDefault;
        [SerializeField] Text cardholderScheme;
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

            cardholderScheme.text = creditCardInfo.scheme;
            cardNumber.text = "**** **** ****" + creditCardInfo.number;
            cvv.text = "***";
            expirationDate.text = creditCardInfo.date;

            Debug.Log("CardView: Default: " + creditCardInfo.isDefault);

            if (creditCardInfo.isDefault)
                titleByDefault.text = cardholderScheme.text + " (Default)";
            else
                titleByDefault.text = cardholderScheme.text;
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

