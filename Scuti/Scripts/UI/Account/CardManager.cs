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
        [SerializeField] GameObject emptyCardView;
        [SerializeField] GameObject prefabCards;
        [SerializeField] GameObject contentCards;

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


        public void UpdateCardsView(List<UserCard> cards)
        {
            if(cards.Count <= 0)
            {
                emptyCardView.gameObject.SetActive(true);
                Debug.Log("CardManager: No Card added");
            }                
            else
            {
                emptyCardView.gameObject.SetActive(false);
            }               

            if (creditCardList.Count == 0)
            {
                if(cards.Count > 0)
                {
                    // If dont exist card created
                    for (int i = 0; i < cards.Count; i++)
                    {
                        var card = Instantiate(prefabCards, contentCards.transform);
                        CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();

                        creditCardInfo.id = cards[i].Id;
                        creditCardInfo.name = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        Debug.Log("UpdateCardView: " + creditCardInfo.date);
                        Debug.Log("UpdateCardView IsDefault: " + cards[i].IsDefault);
                        CreditCardView cardView = card.GetComponent<CreditCardView>();
                        cardView.onShowCardInfo -= UpdatedValueData;
                        cardView.onShowCardInfo += UpdatedValueData;

                        creditCardList.Add(cardView);
                        cardView.Refresh(creditCardInfo);
                    }
                }          
            }

            else
            {
                // Hide all current credit card views
                for (int i = 0; i < creditCardList.Count; i++)
                {
                    creditCardList[i].Hide();
                }

                if(creditCardList.Count >= cards.Count)
                {
                    for (int i = 0; i < cards.Count; i++)
                    {
                        CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();

                        creditCardInfo.id = cards[i].Id;
                        creditCardInfo.name = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        Debug.Log("UpdateCardView: " + creditCardInfo.date);
                        creditCardList[i].onShowCardInfo -= UpdatedValueData;
                        creditCardList[i].onShowCardInfo += UpdatedValueData;

                        creditCardList[i].Refresh(creditCardInfo);
                        creditCardList[i].Show();
                    }
                }
                else if(creditCardList.Count < cards.Count)
                {
                    int index = cards.Count - creditCardList.Count;

                    for (int i = 0; i < index; i++)
                    {
                        var card = Instantiate(prefabCards, contentCards.transform);
                        CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();
                        CreditCardView cardView = card.GetComponent<CreditCardView>();
                        cardView.onShowCardInfo -= UpdatedValueData;
                        cardView.onShowCardInfo += UpdatedValueData;
                        creditCardList.Add(cardView);
                    }

                    for (int i = 0; i < cards.Count; i++)
                    {
                        CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();
                        creditCardInfo.id = cards[i].Id;
                        creditCardInfo.name = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        creditCardList[i].onShowCardInfo -= UpdatedValueData;
                        creditCardList[i].onShowCardInfo += UpdatedValueData;
                        creditCardList[i].Refresh(creditCardInfo);
                        creditCardList[i].Show();
                    }
                }
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

                cardDetailForm.onDeleteCard -= UpdateCards;
                cardDetailForm.onDeleteCard += UpdateCards;

                cardDetailForm.onAddCard -= UpdateCards;
                cardDetailForm.onAddCard += UpdateCards;

                cardDetailForm.onOpenCardDetails -= BtnAddNewCard;
                cardDetailForm.onOpenCardDetails += BtnAddNewCard;
            }

            emptyCardView.gameObject.SetActive(false);

            base.Open();
            Debug.Log("CardManager: OPEN");
            TryToLoadData();
        }


        private async void TryToLoadData()
        {
            try
            { 
                var cards = await ScutiAPI.GetPayments();

                Debug.Log("CartPresenter: Card amounts:" + cards.Count);

                if (cards != null && cards.Count > 0)
                {
                    Data.Card = new CreditCardData();
                    Data.Card.Reset();
                    _cachedCard = cards.Last();
                    ScutiLogger.Log(_cachedCard.Scheme + "  Last: " + _cachedCard.Last4 + " and " + _cachedCard.ToString());
                  
                }
                else if (Data.Card == null)
                {
                    Data.Card = new CreditCardData();
                    Data.Card.Reset();
                }

                // Create Cards
                List<UserCard> cardAux = (List<UserCard>)cards;
                UpdateCardsView(cardAux);

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
                ScutiLogger.LogError(ex);
            }

        }


        private void UpdateCards()
        {
            TryToLoadData();
        }


        private void UpdatedValueData(CreditCardView.CreditCardModel creditCardInfo)
        {
            GetCardDetails(creditCardInfo);           
        }

        public async void GetCardDetails(CreditCardView.CreditCardModel creditCardInfo)
        {
            UIManager.ShowLoading(false);

            try 
            {
                var rest = await ScutiAPI.GetCardDetails(creditCardInfo.id);
                UIManager.HideLoading(false);

                if (rest != null)
                {
                    cardDetailForm.Data.Address.Line1 = rest.BillingAddress.Address1;
                    cardDetailForm.Data.Address.Line2 = rest.BillingAddress.Address2;
                    cardDetailForm.Data.Address.City = rest.BillingAddress.City;
                    cardDetailForm.Data.Address.State = rest.BillingAddress.State;
                    cardDetailForm.Data.Address.Country = rest.BillingAddress.Country;
                    cardDetailForm.Data.Address.Zip = rest.BillingAddress.ZipCode;

                    cardDetailForm.Data.Card.Name = rest.Name;
                    Debug.Log("CARD DETAILS: "+rest.IsDefault);
                    cardDetailForm.CurrentCardId = rest.Id;

                    cardDetailForm.Data.Card.Number = "************"+rest.Last4;
                    cardDetailForm.Data.Card.CardType = rest.Scheme;
                }
            }
            catch (Exception ex)
            {
                UIManager.HideLoading(false);
                UIManager.Alert.SetHeader("Error").SetBody("Card credit info failed.").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
                cardDetailForm.Close();
            }           

            cardDetailForm.IsRemoveCardAvailable(true);
            cardDetailForm.Refresh();
        }

        public void BtnAddNewCard()
        {
            cardDetailForm.IsRemoveCardAvailable(false);
            cardDetailForm.Data.Card.Reset();
            cardDetailForm.Data.Address.Reset();
        }

        // FOR TESTING
        public void BtnDeleteAllCards()
        {
            string[] ids = new string[creditCardList.Count];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = creditCardList[i].GetId();
            }

            cardDetailForm.DeleteAllCards(ids);

        }

    }
}
