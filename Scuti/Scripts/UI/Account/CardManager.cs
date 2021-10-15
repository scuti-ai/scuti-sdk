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
    public class CardManager : View
    {
        public CreditCardData Card;
        public AddressData ShippingAddress;
        public AddressData BillingAddress;

        [SerializeField] List<UserCard> creditCardList;
        [SerializeField] GameObject prefabCards;
        [SerializeField] GameObject parentCards;

        private UserCard _cachedCard = null;
        private bool _cachedAddress = false;
        private bool _autoPurchase = false;

        private CardDetailsForm.Model model;

        /*public bool IsEmpty
        {
            get { return Data.Items == null || Data.Items.Count == 0; }
        }*/


        public void CreateCards()
        {


        }

        public void Refresh()
        {
            base.Open();
            Refresh();
        }

        public void Bind()
        {

        }

        public override void Open()
        {
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
                        /*Data.Card = new CreditCardData();
                        Data.Card.Reset();
                        _cachedCard = cards.Last();
                        */
                        _cachedCard = cards.Last();
                        ScutiLogger.Log(_cachedCard.Scheme + "  Last: " + _cachedCard.Last4 + " and " + _cachedCard.ToString());

                        List<UserCard> cardAux = (List<UserCard>)cards; 

                        for(int i = 0; i < cardAux.Count; i++)
                        {
                            var card = Instantiate(prefabCards, parentCards.transform);
                            CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();
                            creditCardInfo.name = cardAux[i].Scheme;
                            creditCardInfo.number = cardAux[i].Last4;
                            creditCardInfo.cvv = cardAux[i].Id;
                            creditCardInfo.date = cardAux[i].ExpiryMonth.ToString()+"/"+ cardAux[i].ExpiryYear.ToString().Substring(cardAux[i].ExpiryYear.ToString().Length - 2);
                            Debug.Log("CardManager Card: " + creditCardInfo.date);
                            CreditCardView cardView = card.GetComponent<CreditCardView>();
                            cardView.Refresh(creditCardInfo);
                        } 

                    }
                    /*else if (Data.Card == null)
                    {
                        Data.Card = new CreditCardData();
                        Data.Card.Reset();
                    }*/
                }

                if (!_cachedAddress)
                {
                    /*var shippingInfo = await ScutiAPI.GetShippingInfo();
                    if (shippingInfo != null)
                    {
                        Data.ShippingAddress = new AddressData()
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
                    }*/
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

            //UpdatePriceBreakdown(checkout);
        }


        public async void UpdatePriceBreakdown(bool checkout = false)
        {




        }
    }
}
