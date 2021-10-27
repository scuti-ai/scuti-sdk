using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Scuti.UI
{
    public class CreditCardView : View
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
        [SerializeField] Image checkmarkByDefault;

        [SerializeField] bool isDefaultCard;

        private CreditCardModel creditCardInfo;

        public Action<CreditCardModel> onShowCardInfo;
        public Action<CreditCardModel> onSelectCard;

        // ----------------------------------------------------------------------

        public void Refresh(CreditCardModel creditCardInfo)
        {
            this.creditCardInfo = creditCardInfo;

            cardholderScheme.text = creditCardInfo.scheme;
            cardNumber.text = "**** **** ****" + creditCardInfo.number;
            cvv.text = "***";
            expirationDate.text = creditCardInfo.date;           

            checkmarkByDefault.gameObject.SetActive(creditCardInfo.isDefault);

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

        /*public void Hide()
        {
            gameObject.SetActive(false);        
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }*/

        public void ButtonOnSelectCard()
        {
            onSelectCard?.Invoke(creditCardInfo);
        }

    }
}

