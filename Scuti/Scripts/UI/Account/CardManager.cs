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
    public class CardManager : Presenter<CardManager.Model>
    {
        [Serializable]
        public class Model : Presenter.Model
        {
            public CreditCardData Card;
            public AddressData Address;
        }

        [SerializeField] CardDetailsForm cardDetailForm;
        [SerializeField] List<CreditCardView> creditCardList;
        [SerializeField] GameObject emptyCardView;
        [SerializeField] GameObject prefabCards;
        [SerializeField] GameObject contentCards;

        private UserCard _cachedCard = null;
        private bool _cachedAddress = false;
        private bool _autoPurchase = false;

        public bool isSelectCardMode;


        protected override void Awake()
        {
            base.Awake();
            Data = new Model();
        }
        
        /*public override CardDetailsForm.Model GetDefaultDataObject()
        {
            var model = new CardDetailsForm.Model() { Card = new CreditCardData() { ExpirationMonth = DateTime.Now.Month, ExpirationYear = DateTime.Now.Year }, Address = new AddressData() { Line2 = "" } };
            model.Card.Reset();

            return model;
        }*/

        /// <summary>
        /// Instance and update the credit cards when it starts. It also updates when cards are removed and added during the session.
        /// </summary>
        /// <param name="cards"></param>
        public void CreateAndUpdateCardsView(List<UserCard> cards)
        {
            // If a card does not exist, display the empty card.
            if (cards.Count <= 0)            
                emptyCardView.gameObject.SetActive(true);                        
            else            
                emptyCardView.gameObject.SetActive(false);                           


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
                        creditCardInfo.scheme = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.isDefault = (bool)cards[i].IsDefault;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        
                        CreditCardView cardView = card.GetComponent<CreditCardView>();
                        cardView.onShowCardInfo -= UpdatedValueData;
                        cardView.onShowCardInfo += UpdatedValueData;
                        cardView.onSelectCard -= ConfirmSetCardByDefault;
                        cardView.onSelectCard += ConfirmSetCardByDefault;

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
                        creditCardInfo.scheme = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.isDefault = (bool)cards[i].IsDefault;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        
                        creditCardList[i].onShowCardInfo -= UpdatedValueData;
                        creditCardList[i].onShowCardInfo += UpdatedValueData;
                        creditCardList[i].onSelectCard -= ConfirmSetCardByDefault;
                        creditCardList[i].onSelectCard += ConfirmSetCardByDefault;

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
                        cardView.onSelectCard -= ConfirmSetCardByDefault;
                        cardView.onSelectCard += ConfirmSetCardByDefault;
                        creditCardList.Add(cardView);
                    }

                    for (int i = 0; i < cards.Count; i++)
                    {
                        CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();
                        creditCardInfo.id = cards[i].Id;
                        creditCardInfo.scheme = cards[i].Scheme;
                        creditCardInfo.number = cards[i].Last4;
                        creditCardInfo.cvv = cards[i].Id;
                        creditCardInfo.isDefault = (bool)cards[i].IsDefault;
                        creditCardInfo.date = cards[i].ExpiryMonth.ToString() + "/" + cards[i].ExpiryYear.ToString().Substring(cards[i].ExpiryYear.ToString().Length - 2);
                        creditCardList[i].onShowCardInfo -= UpdatedValueData;
                        creditCardList[i].onShowCardInfo += UpdatedValueData;
                        creditCardList[i].onSelectCard -= ConfirmSetCardByDefault;
                        creditCardList[i].onSelectCard += ConfirmSetCardByDefault;
                        creditCardList[i].Refresh(creditCardInfo);
                        creditCardList[i].Show();
                    }
                }
            }            
        }


        /*public override void Refresh()
        {
            throw new NotImplementedException();
        }

        public override void Bind()
        {
            throw new NotImplementedException();
        }*/

        public override void Close()
        {
            isSelectCardMode = false;
            base.Close();
        }

        public override void Open()
        {
            Debug.Log("CardMananger: OPENED");

            // Find the CardDetailsForm component 
            if (cardDetailForm == null)
            {
                var parent = GetComponentInParent<ViewSetInstantiator>();
                cardDetailForm = (CardDetailsForm)parent.GetComponentInChildren(typeof(CardDetailsForm), true);

                cardDetailForm.onDeleteCard -= TryToLoadData;
                cardDetailForm.onDeleteCard += TryToLoadData;

                cardDetailForm.onAddCard -= TryToLoadData;
                cardDetailForm.onAddCard += TryToLoadData;

                cardDetailForm.onOpenCardDetails -= BtnAddNewCard;
                cardDetailForm.onOpenCardDetails += BtnAddNewCard;
            }

            // Hide empty credit card view
            emptyCardView.gameObject.SetActive(false);

            base.Open();
            TryToLoadData();
        }

        /// <summary>
        /// Get info for payment methods stored on server
        /// </summary>
        private async void TryToLoadData()
        {
            try
            { 
                var cards = await ScutiAPI.GetPayments();

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

                // Save credit cards info
                List<UserCard> cardAux = (List<UserCard>)cards;
                CreateAndUpdateCardsView(cardAux);

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

        /*private void GetCardsInfo()
        {
            TryToLoadData();
        }*/

        #region Set Default Card

        private async void GetCardDetailsForSelectCard(CreditCardView.CreditCardModel creditCardInfo)
        {
            UIManager.ShowLoading(false);
            try
            {
                var rest = await ScutiAPI.GetCardDetails(creditCardInfo.id);
                UIManager.HideLoading(false);               

                if (rest != null)
                {
                    Data.Card.Name = rest.Name;
                    SaveInformationForSelectCard(creditCardInfo);
                }
                //Data.Card.Name = rest.Name;
                //Debug.Log("NAME 2----------------: " + rest.Name);
                //SaveInformationForSelectCard(creditCardInfo);
            }
            catch (Exception ex)
            {
                UIManager.HideLoading(false);
                UIManager.Alert.SetHeader("Error").SetBody("Card credit info failed.").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
            }
        }

        private void SaveInformationForSelectCard(CreditCardView.CreditCardModel creditCardInfo)
        {

            Data.Card.Number = creditCardInfo.number;
            Data.Card.CVV = creditCardInfo.cvv;
            Data.Card.CardType = creditCardInfo.scheme;
            Data.Card.IsValid();
            Submit();
            Close();

        }


        private void ConfirmSetCardByDefault(CreditCardView.CreditCardModel creditCardInfo)
        {
            if (isSelectCardMode)
            {

                //Save data of Credit card selected
                Data.Card = new CreditCardData();
                Data.Card.Reset();
                Data.Address = cardDetailForm.Data.Address;

                GetCardDetailsForSelectCard(creditCardInfo);
               
            }
            else 
            {               
                UIManager.Confirmation.SetHeader("Default credit card").SetBody("Do you want to have the card with number " + creditCardInfo.number + " as default card?").SetPositive("Yes").SetNegative("No").Show((bool callback) => {
                    if (callback)
                        SetCardByDefault(creditCardInfo);
                    else
                        return;
                });
            }
        }

        private async void SetCardByDefault(CreditCardView.CreditCardModel creditCardInfo)
        {
            UIManager.ShowLoading(false);

            try
            {
                await ScutiAPI.SetMyDefaultCard(creditCardInfo.id);
                UIManager.HideLoading(false);

            }
            catch (Exception ex)
            {
                UIManager.HideLoading(false);
                UIManager.Alert.SetHeader("Error").SetBody("Card credit info failed.").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
            }

            TryToLoadData();
        }

        #endregion

        #region CardDetaildFom

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

        #endregion

        #region Testing

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

        #endregion

    }
}
