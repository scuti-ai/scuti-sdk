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
    public class CardManager : Form<CardManager.Model>
    {
        [Serializable]
        public class Model : Form.Model
        {
            public CreditCardData Card;
            public AddressData Address;
        }

        [SerializeField] List<CreditCardView> creditCardList;
        [SerializeField] GameObject emptyCardView;
        [SerializeField] CreditCardView prefabCards;
        [SerializeField] GameObject contentCards;

        private UserCard _cachedCard = null;

        private List<UserCard> _cardsInformation;
        public bool isSelectCardMode;


        protected override void Awake()
        {
            base.Awake();
            Data = new Model();
        }

        #region Form methods
        public override void Bind() { }
        public override void Refresh() { }
        public override Model GetDefaultDataObject()
        {
            var model = new Model() { Card = new CreditCardData() { ExpirationMonth = DateTime.Now.Month, ExpirationYear = DateTime.Now.Year }, Address = new AddressData() { Line2 = "" } };
            model.Card.Reset();

            return model; throw new NotImplementedException();
        }

        #endregion

        #region Card Info

        /// <summary>
        /// Instance and update the credit cards when it starts. It also updates when cards are removed and added during the session.
        /// </summary>
        /// <param name="cards"></param>
        public void CreateAndUpdateCardsView()
        {
            // If a card does not exist, display the empty card.
            if (_cardsInformation.Count <= 0)
            {
                emptyCardView.gameObject.SetActive(true);                
            }
            else
            {
                emptyCardView.gameObject.SetActive(false);
            }

            // It happens when you enter the first time
            if (creditCardList.Count <= _cardsInformation.Count)
            {
                int diff = _cardsInformation.Count - creditCardList.Count; 
                for(int i = 0; i < diff; i++)
                {
                    CreditCardView cardView = Instantiate<CreditCardView>(prefabCards, contentCards.transform);
                    creditCardList.Add(cardView);
                }
            }
            else
            {
                // Reusing the created cards and update the information.
                int diff =  creditCardList.Count - _cardsInformation.Count;
                int countCards = creditCardList.Count;
                for (int i = 0; i < diff; i++)
                {
                    Destroy(creditCardList[countCards - 1 - i].gameObject);
                    creditCardList.RemoveLast();
                }                
            }

            UpdateCardInfoView();

        }

        private void UpdateCardInfoView()
        {
            // It sorts the UserCard list in increasing order according to the last card numbers.
            _cardsInformation = _cardsInformation.OrderBy(f => (f.Last4)).ToList(); ;

            for (int i = 0; i < _cardsInformation.Count; i++)
            {
                CreditCardView.CreditCardModel creditCardInfo = new CreditCardView.CreditCardModel();

                creditCardInfo.id = _cardsInformation[i].Id;
                creditCardInfo.scheme = _cardsInformation[i].Scheme;
                creditCardInfo.number = _cardsInformation[i].Last4;
                creditCardInfo.cvv = _cardsInformation[i].Id;
                creditCardInfo.isDefault = (bool)_cardsInformation[i].IsDefault;
                creditCardInfo.date = _cardsInformation[i].ExpiryMonth.ToString() + "/" + _cardsInformation[i].ExpiryYear.ToString().Substring(2, 2);

                creditCardList[i].onShowCardInfo -= UpdatedValueData;
                creditCardList[i].onShowCardInfo += UpdatedValueData;
                creditCardList[i].onSelectCard -= ConfirmSetCardByDefault;
                creditCardList[i].onSelectCard += ConfirmSetCardByDefault;

                creditCardList[i].Refresh(creditCardInfo);
                creditCardList[i].Open();
            }
        }

        #endregion


        public override void Close()
        {
            isSelectCardMode = false;
            base.Close();
        }

        public override void Open()
        {                
            UIManager.Card.onDeleteCard -= TryToLoadData;
            UIManager.Card.onDeleteCard += TryToLoadData;

            UIManager.Card.onAddCard -= TryToLoadData;
            UIManager.Card.onAddCard += TryToLoadData;

            UIManager.Card.onOpenCardDetails -= BtnAddNewCard;
            UIManager.Card.onOpenCardDetails += BtnAddNewCard;

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
                _cardsInformation = (List<UserCard>)cards;
                CreateAndUpdateCardsView();

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


        #region Set Default Card

        // Get information from the credit card selected
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
            }
            catch (Exception ex)
            {
                UIManager.HideLoading(false);
                UIManager.Alert.SetHeader("Error").SetBody("Card credit info failed.").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
            }
        }

        // Save the selected card information in the shopping cart.
        private void SaveInformationForSelectCard(CreditCardView.CreditCardModel creditCardInfo)
        {
            Data.Card.Number = creditCardInfo.number;
            Data.Card.CVV = creditCardInfo.cvv;
            Data.Card.CardType = creditCardInfo.scheme;
            Data.Card.IsValid();
            Submit();
            Close();
        }

        // Select a card for purchase in the cart or open confirmation popup to select default card.
        // Depending on the instance, If you are in the shopping cart or in the payment methods.
        private void ConfirmSetCardByDefault(CreditCardView.CreditCardModel creditCardInfo)
        {
            if (isSelectCardMode)
            {
                //Save data of Credit card selected
                Data.Card = new CreditCardData();
                Data.Card.Reset();
                Data.Address = UIManager.Card.Data.Address;

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

        // Call the endpoint to set the default credit card after confirmation.
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

        // Get detailed information of the card to be edited or removed.
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
                    UIManager.Card.Data.Address.Line1 = rest.BillingAddress.Address1;
                    UIManager.Card.Data.Address.Line2 = rest.BillingAddress.Address2;
                    UIManager.Card.Data.Address.City = rest.BillingAddress.City;
                    UIManager.Card.Data.Address.State = rest.BillingAddress.State;
                    UIManager.Card.Data.Address.Country = rest.BillingAddress.Country;
                    UIManager.Card.Data.Address.Zip = rest.BillingAddress.ZipCode;

                    UIManager.Card.Data.Card.Name = rest.Name;
                    UIManager.Card.CurrentCardId = rest.Id;

                    UIManager.Card.Data.Card.Number = "************"+rest.Last4;
                    UIManager.Card.Data.Card.CardType = rest.Scheme;
                }
            }
            catch (Exception ex)
            {
                UIManager.HideLoading(false);
                UIManager.Alert.SetHeader("Error").SetBody("Card credit info failed.").SetButtonText("Ok").Show(() => { });
                ScutiLogger.LogError(ex);
                UIManager.Card.Close();
            }

            UIManager.Card.IsRemoveCardAvailable(true);


            Debug.Log("--InBackedn: State: " + UIManager.Card.Data.Address.State);

            Debug.Log("--InBackedn: Country: " + UIManager.Card.Data.Address.Country);

            // new line
            UIManager.Card.UpdatedAddresInfo(UIManager.Card.Data.Address.Country);

            UIManager.Card.Refresh();
        }

        public void BtnAddNewCard()
        {
            UIManager.Card.IsRemoveCardAvailable(false);
            UIManager.Card.Data.Card.Reset();
            UIManager.Card.Data.Address.Reset();
        }

        #endregion        

    }
}
