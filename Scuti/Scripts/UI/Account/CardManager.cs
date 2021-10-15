using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;
using System.Threading.Tasks;
using Scuti.Net;
using Scuti.GraphQL;
using Scuti.GraphQL.Generated;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Scuti.UI
{
    public class CardManager : Form<CardDetailsForm.Model>
    {
        [SerializeField] CardDetailsForm cardDetailForm;

        [SerializeField] List<CreditCardView> creditCardList;
        [SerializeField] GameObject prefabCards;
        [SerializeField] GameObject parentCards;

        private UserCard _cachedCard = null;
        private bool _cachedAddress = false;
        private bool _autoPurchase = false;

        protected override void Awake()
        {

        }

        public override CardDetailsForm.Model GetDefaultDataObject()
        {
            var model = new CardDetailsForm.Model() { Card = new CreditCardData() { ExpirationMonth = DateTime.Now.Month, ExpirationYear = DateTime.Now.Year }, Address = new AddressData() { Line2 = "" } };
            model.Card.Reset();

            return model;
        }


        public void CreateCards(List<UserCard> cards)
        {

            for (int i = 0; i < cards.Count; i++)
            {
                var card = Instantiate(prefabCards, parentCards.transform);
                CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();

                creditCardInfo.id = cards[i].Id;
                creditCardInfo.name = cards[i].Scheme;
                creditCardInfo.number = cards[i].Last4;
                creditCardInfo.cvv = cards[i].Id;
                creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
     
                CreditCardView cardView = card.GetComponent<CreditCardView>();
                cardView.onShowCardInfo -= UpdatedValueData;
                cardView.onShowCardInfo += UpdatedValueData;

                creditCardList.Add(cardView);
                cardView.Refresh(creditCardInfo);
            }

        }

        public override void Refresh()
        {

        }

        public override void Bind()
        {

        }

        public override void Open()
        {
            if (cardDetailForm == null)
            {
                var parent = GetComponentInParent<ViewSetInstantiator>();
                cardDetailForm = (CardDetailsForm)parent.GetComponentInChildren(typeof(CardDetailsForm), true);
            }

            base.Open();
            Debug.Log("CardManager: OPEN");
            if (_cachedCard == null || !_cachedAddress) TryToLoadData(_autoPurchase);
        }


        private async void TryToLoadData(bool checkout)
        { 
            try
            {
                if (_cachedCard == null)
                {
                    var cards = await ScutiAPI.GetPayments();

                    Debug.Log("CartPresenter: Card amounts:" + cards.Count);

                    if (cards != null && cards.Count > 0)
                    {
                        Data.Card = new CreditCardData();
                        Data.Card.Reset();
                        _cachedCard = cards.Last();
                        ScutiLogger.Log(_cachedCard.Scheme + "  Last: " + _cachedCard.Last4 + " and " + _cachedCard.ToString());
                        List<UserCard> cardAux = (List<UserCard>)cards;
                        CreateCards(cardAux);
                    }
                    else if (Data.Card == null)
                    {
                        Data.Card = new CreditCardData();
                        Data.Card.Reset();
                    }
                }

                if (!_cachedAddress)
                {
                    var shippingInfo = await ScutiAPI.GetShippingInfo();
                    if (shippingInfo != null)
                    {
                        Data.Address = new AddressData()
                        {
                            Line1 = shippingInfo.Address1,
                            Line2 = shippingInfo.Address2,
                            State = shippingInfo.State,
                            Zip = shippingInfo.ZipCode,
                            Phone = shippingInfo.Phone,
                            Country = shippingInfo.Country,
                            City = shippingInfo.City
                        };
                        _cachedAddress = true;
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex is GQLException)
                {
                    var gqlException = ex as GQLException;
                    if (gqlException.responseCode == 401 && gqlException.response.Contains("jwt expired"))
                    {
                        //Debug.LogError("SHOULD RE LOG NOW");
                    }
                }
                checkout = false;
                ScutiLogger.LogError(ex);

            }

        }

        private void UpdatedValueData(CreditCardView.CreditCardModel creditCardInfo)
        {
            GetCardDetails(creditCardInfo);           
        }

        public async void GetCardDetails(CreditCardView.CreditCardModel creditCardInfo)
        {
            var rest = await ScutiAPI.GetCardDetails(creditCardInfo.id);

            if(rest != null)
            {
                cardDetailForm.Data.Address.Line1 = rest.BillingAddress.Address1;
                cardDetailForm.Data.Address.Line2 = rest.BillingAddress.Address2;
                cardDetailForm.Data.Address.City = rest.BillingAddress.City;
                cardDetailForm.Data.Address.State = rest.BillingAddress.State;
                cardDetailForm.Data.Address.Country = rest.BillingAddress.Country;
                cardDetailForm.Data.Address.Zip = rest.BillingAddress.ZipCode;

                cardDetailForm.CurrentCardId = rest.Id;

                cardDetailForm.Data.Card.Number = rest.Last4;
                cardDetailForm.Data.Card.CardType = rest.Scheme;
            }

            cardDetailForm.IsRemoveCardAvailable(true);
            cardDetailForm.Refresh();
        }

        public void BtnAddNewCard()
        {
            cardDetailForm.IsRemoveCardAvailable(false);
            cardDetailForm.Data.Card.Reset();
        }
    }
}
