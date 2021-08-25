using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Scuti.Net;
using Scuti.GraphQL.Generated;
using Scuti.GraphQL;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Scuti.UI
{
    public class CartPresenter : Presenter<CartPresenter.Model>
    {
        [Serializable]
        public class Model : Presenter.Model
        { 
            public CreditCardData Card;
            public AddressData ShippingAddress;
            public AddressData BillingAddress;

            public decimal ShippingFee;
            public decimal SalesTax;
            public List<CartEntryPresenter.Model> Items = new List<CartEntryPresenter.Model>();


            public decimal GetCalculatedSubtotal()
            {
                return Items.Sum(x => x.price.Value * x.quantity) + ShippingFee + SalesTax;
            }

            public int GetScutisTotal()
            {
                return Items.Sum(x => (x.scutiCoinReward * x.quantity));
            }

            internal void RemoveItem(CartEntryPresenter.Model model)
            {
                Items.Remove(model);
#pragma warning disable 4014
                ScutiAPI.CartMetric(GraphQL.Generated.CartAction.Remove, 1);
#pragma warning restore 4014
            }

            internal void AddItem(CartEntryPresenter.Model model)
            {

                // check for duplicate
                foreach (var item in Items)
                {
                    if (item.id == model.id)
                    {
                        item.quantity += model.quantity;
                        item.InvokeEvent("item-quantity-changed", null); 
                        ScutiAPI.CartMetric(GraphQL.Generated.CartAction.ChangeCount, item.quantity);
                        InvokeEvent("item-quantity-changed", item);
  
                        return;
                    }
                }


                Items.Add(model); 
                ScutiAPI.CartMetric(GraphQL.Generated.CartAction.Add, 1);
                InvokeEvent("item-added", model); 
            }
        }

        public event Action OnCheckout;

        [Header("Instantiation")]
        [SerializeField] CartEntryPresenter entryPrefab;
        [SerializeField] Transform entryContainer;

        [Header("Summarization")]
        [SerializeField] Text itemCountText1;
        [SerializeField] Text itemCountText2;
        [SerializeField] Text addressText;
        [SerializeField] Text cardText;
        [SerializeField] Text shippingFeeText;
        [SerializeField] Text salesTaxText;
        [SerializeField] Text subtotalAmountText;

        [Header("User Interaction")]
        [SerializeField] Button m_CheckoutButton;

        List<CartEntryPresenter> m_Instances = new List<CartEntryPresenter>();

        public Model state;
        private UserCard _cachedCard = null;
        private bool _cachedAddress = false;

        private bool _autoPurchase = false;
        public void PurchaseOnLoad(bool value)
        {
            _autoPurchase = value;
        }

        public bool IsEmpty
        {
            get { return Data.Items == null || Data.Items.Count == 0; }
        }

        // ================================================
        // INITIALIZATION
        // ================================================

        protected override void Awake()
        {
            base.Awake();
            HandleInteractions(); 
            Data = Mappers.GetEmptyCart();
        }

        void HandleInteractions()
        {
            m_CheckoutButton.onClick.AddListener(() => OnCheckout?.Invoke());
        }

        public override void Open()
        {
            base.Open();

            if (_cachedCard == null || !_cachedAddress) TryToLoadData(_autoPurchase);
            else UpdatePriceBreakdown(_autoPurchase);
            _autoPurchase = false;
        }


        private async void TryToLoadData(bool checkout)
        {
            try
            {
                if (_cachedCard == null)
                {
                    var cards = await ScutiAPI.GetPayments();
                    if(cards!=null && cards.Count>0)
                    {
                        Data.Card = new CreditCardData();
                        Data.Card.Reset();
                        _cachedCard = cards.Last();
                        ScutiLogger.Log(_cachedCard.Scheme + "  Last: " + _cachedCard.Last4 + " and " + _cachedCard.ToString());
                    } else if(Data.Card==null)
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
                        Data.ShippingAddress = new AddressData()
                        {
                            Line1 = shippingInfo.Address1,
                            Line2 = shippingInfo.Address2,
                            State = shippingInfo.State,
                            Zip = shippingInfo.ZipCode,
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

            UpdatePriceBreakdown(checkout);
        }

        // ================================================
        // STATEFUL UI OVERRIDES
        // ================================================
        protected override void OnSetState()
        {
            state = Data;

            Clear();
            if (Data != null)
            {
                foreach (var item in Data.Items)
                    InstantiateWidget(item);
            }
            UpdatePriceBreakdown();
        }

        protected void RefreshText()
        {
            if(Data.ShippingFee>-1)
                shippingFeeText.text = $"$ {Data.ShippingFee.ToString("0.00")}";
            else
            {
                shippingFeeText.text = "N/A";
            }

            if(Data.SalesTax>-1)
                salesTaxText.text = $"$ {Data.SalesTax.ToString("0.00")}";
            else
            {
                salesTaxText.text = "N/A";
            }

            if (Data == null || Data.ShippingAddress == null || !Data.ShippingAddress.IsValid())
            {
                addressText.text = $"ENTER SHIPPING ADDRESS";
            }
            else
            {
                addressText.text = $"DELIVER TO:  {Data.ShippingAddress.ToString()}";
            }

            if(_cachedCard !=null)
            {
                cardText.text = $"Card ending in {_cachedCard.Last4}";
            }
            else if (Data != null && Data.Card!=null && Data.Card.IsValid())
            {

                cardText.text = $"{Data.Card.CardType} ending in {Data.Card.Number.ToString().Substring(Data.Card.Number.Length - 4)}";
            }
            else
            {
                cardText.text = "ENTER CREDIT CARD";
            }
        }

        public override void OnEvent(string notification, object payload)
        {
            switch (notification)
            {
                case "item-deleted":
                    DestroyWidget(payload as CartEntryPresenter.Model);
                    UpdatePriceBreakdown();
                    break;
                case "item-added":
                    InstantiateWidget(payload as CartEntryPresenter.Model);
                    UpdatePriceBreakdown();
                    break;
                case "item-quantity-changed":
                    UpdatePriceBreakdown();
                    break;
            }
        }


        internal void RecordApplicationBackgrounded()
        {

            //#pragma warning disable 4014
            //            MetricsAPI.CartMetric(GraphQL.Generated.CartAction.ChangeCount((int)diff);
            //#pragma warning restore 4014
        }

        // ================================================
        // DYNAMIC UI
        // ================================================
        public void InstantiateWidget(CartEntryPresenter.Model entry)
        {
            var instance = Instantiate(entryPrefab, entryContainer);
            instance.gameObject.hideFlags = HideFlags.DontSave;
            instance.name = entry.title;
            instance.Data = entry;

            instance.OnDelete += widget =>
            {
                Data.RemoveItem(entry);
                Data.InvokeEvent("item-deleted", instance.Data);
            };

            instance.OnQuantityChanged += newQuantity =>
            {
                entry.quantity = newQuantity;
                Data.InvokeEvent("item-quantity-changed", instance.Data);

#pragma warning disable 4014
                ScutiAPI.CartMetric(GraphQL.Generated.CartAction.ChangeCount, newQuantity);
#pragma warning restore 4014
            };

            m_Instances.Add(instance);
        }



        public void DestroyWidget(CartEntryPresenter.Model state)
        {
            var match = m_Instances.First(x => x.Data.Equals(state));
            if (match != null)
            {
                m_Instances.Remove(match);
                Destroy(match.gameObject);
            }
        }

        public async void UpdatePriceBreakdown(bool checkout = false)
        {
            itemCountText1.text = $"({Data.Items.Count}  ITEMS)";
            itemCountText2.text = $"({Data.Items.Count}  ITEMS)";

            subtotalAmountText.text = "Calculating...";

            Data.ShippingFee = -1;
            Data.SalesTax = -1;
            
            if (ScutiNetClient.Instance.IsAuthenticated && IsOpenOrOpening )
            {
                var address = GetAddress();
                if (address != null)
                {
                    try
                    {
                        var items = Data.Items;
                        CheckoutItemInput[] orders = new CheckoutItemInput[items.Count];

                        // cancel the auto checkout if they have more than 1 item already in their cart. Eventually could try to treat that as a separate cart. -mg 
                        if (items.Count > 1) checkout = false;

                        for (int i = 0; i < orders.Length; ++i)
                        {
                            var item = items[i];
                            orders[i] = new CheckoutItemInput
                            {
                                OfferId = item.campaignId,
                                VariantId = item.variant.ToString(),
                                Quantity = item.quantity
                            };
                        }

                        var response = await ScutiAPI.CalculateCart(orders, address);

                        if (response != null)
                        {
                            Data.ShippingFee = response.Breakdown.Shipping.Value;
                            Data.SalesTax = response.Breakdown.Tax.Value;
                            subtotalAmountText.text = $"$ {response.Breakdown.Total.Value.ToString("0.00")}";
                        } else
                        {
                            subtotalAmountText.text = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        ScutiLogger.LogException(ex);
                        UIManager.Open(UIManager.Alert);
                        UIManager.Alert.SetHeader("Failed To Calculate Cart").SetBody($"Failed to calculate cart with error: {ex.Message}").SetButtonText("Ok").Show(() => { });


                        subtotalAmountText.text = "N/A";
                    }
                } else
                {
                    subtotalAmountText.text = "Address Required";
                }
            } else
            {
                subtotalAmountText.text = "Login Required";
            }
            RefreshText();
            if(checkout)
            {
                Checkout();
            }
        }

        public void Clear()
        {
            foreach (var instance in m_Instances)
                Destroy(instance.gameObject);
            m_Instances.Clear();
        }


        // ================================================
        // HANDLERS
        // ================================================
        public void Checkout()
        {

            //UIManager.ShowLoading();
            if (Data != null && Data.Items.Count > 0)
            {
                if (Data.ShippingAddress == null || !Data.ShippingAddress.IsValid())
                {
                    UIManager.Open(UIManager.Alert);
                    UIManager.Alert.SetHeader("Missing Information").SetBody("Please set your shipping address.").SetButtonText("Ok").Show(() => { });
                }
                else
                {
                    PaymentSource paymentSource = null; 
                    if (_cachedCard != null)
                    {
                        UIManager.Open(UIManager.CVV);
                        UIManager.CVV.SetButtonText("CONFIRM").Show(OnCVV);
                       

                    } else if (Data.Card!=null && Data.Card.IsValid())
                    { 
                        paymentSource = new PaymentSource() { Type = PaymentSourceType.Card, Card = new CreditCard() {  BillingAddress = GetBillingAddress(),    Encrypted = Data.Card.Encrypted, ExpiryMonth = Data.Card.ExpirationMonth, ExpiryYear = Data.Card.ExpirationYear, Name = Data.Card.Name  }, Persist = Data.Card.SaveCard };
                        CheckoutHelper(paymentSource);
                    } else
                    {
                        EditCard();
                    }
                }
            }

            //UIManager.HideLoading();
            //var historyData = UIManager.Navigator.GetHistory();
            //MetricsAPI.AdImpressionPriorToBuyingMetric(historyData.Path, historyData.Count); 


        }

        private void OnCVV(string cvv)
        {
            CVVHelper(cvv);
        }

        private async Task CVVHelper(string cvv)
        {
            JObject data = new JObject();
            data["cvv"] = cvv;

            var paymentSource = new PaymentSource() { Type = PaymentSourceType.StoredCard, Id = _cachedCard.Id };
            paymentSource.Encrypted = await ScutiUtils.Encrypt(data.ToJson().ToUTF8Bytes());
            CheckoutHelper(paymentSource);
        }

        private async Task CheckoutHelper(PaymentSource paymentSource)
        {
            if (paymentSource != null)
            {
                bool success = false;
                try
                {

                    UIManager.Open(UIManager.Alert);

                    var items = Data.Items;
                    CheckoutItemInput[] orders = new CheckoutItemInput[items.Count];
                    for (int i = 0; i < orders.Length; ++i)
                    {
                        var item = items[i];
                        orders[i] = new CheckoutItemInput
                        {
                            OfferId = item.campaignId,
                            VariantId = item.variant.ToString(),
                            Quantity = item.quantity
                        };
                    }


                    UIManager.Alert.SetHeader("Please Wait").SetBody("Processing your order...").SetButtonText("Ok").SetButtonsEnabled(false).Show(() => { });
                    await ScutiAPI.Checkout(paymentSource, orders, GetAddress());
                    UIManager.Alert.Close();
                    success = true;

                }
                catch (Exception ex)
                {
                    UIManager.Alert.Close();
                    ScutiLogger.LogException(ex);
                    UIManager.Open(UIManager.Alert);
                    UIManager.Alert.SetHeader("Failed To Checkout").SetBody($"Failed to checkout with error: {ex.Message}").SetButtonText("Ok").Show(() => { });
                }

                if (success)
                {
                    Data.Items.Clear();
                    Clear();

                    UIManager.Open(UIManager.Offers);
                    try
                    {

                        var diff = await ScutiNetClient.TryToActivateRewards();

                        if (diff > 0)
                        {
                            UIManager.Rewards.SetData(new RewardPresenter.Model() { reward = (int)diff, subtitle = "Collect your rewards from your purchase!", title = "CONGRATULATIONS!" });
                            UIManager.Open(UIManager.Rewards);
                        }
                    }
                    catch (Exception ex)
                    {
                        ScutiLogger.LogException(ex);
                        UIManager.Open(UIManager.Alert);
                        UIManager.Alert.SetHeader("Purchase Completed").SetButtonText("Ok").SetBody($"Your rewards are pending and will be activated on your next login.");
                    }
                }
            }
            else
            {
                EditCard();
                //UIManager.Open(UIManager.Alert);
                //UIManager.Alert.SetHeader("Missing Information").SetBody("Please set your credit card information.").SetButtonText("Ok").Show(() => { });
            }
        }

        public override void Close()
        {
            base.Close();
            PurchaseOnLoad(false);

        }
        private ShippingInfo GetAddress()
        {
            ShippingInfo address = null;
            if (Data.ShippingAddress != null && Data.ShippingAddress.IsValid())
            {
                address = new ShippingInfo()
                {
                    Address1 = Data.ShippingAddress.Line1,
                    Address2 = Data.ShippingAddress.Line2,
                    City = Data.ShippingAddress.City,
                    Country = Data.ShippingAddress.Country,
                    State = Data.ShippingAddress.State,
                    ZipCode = Data.ShippingAddress.Zip
                };
            }
            return address;
        }

        private AddressInput GetBillingAddress()
        {
            AddressInput address = null;
            if (Data.BillingAddress != null && Data.BillingAddress.IsValid())
            {
                address = new AddressInput()
                {
                    Address1 = Data.BillingAddress.Line1,
                    Address2 = Data.BillingAddress.Line2,
                    City = Data.BillingAddress.City,
                    Country = Data.BillingAddress.Country,
                    State = Data.BillingAddress.State,
                    ZipCode = Data.BillingAddress.Zip
                };
            }  
            return address;
        }


        public void ApplyPromo()
        {
            UIManager.TextInput.SetBody("Enter Promo Code").SetButtonText("APPLY").Show(OnPromoApplied);
        }

        private void OnPromoApplied(string promo)
        {
            UIManager.Alert.SetHeader("PROMO CODE FAILED").SetBody("Please check your code and try again: " + promo).SetButtonText("OK").Show();
        }

        public void EditCard()
        {
            UIManager.Open(UIManager.Card);
            UIManager.Card.OnSubmit -= OnCreditCard;
            UIManager.Card.OnSubmit += OnCreditCard;
            UIManager.Card.SetCached(_cachedCard, Data.ShippingAddress);
        }


        public void EditShipping()
        {
            UIManager.Open(UIManager.Shipping);

            UIManager.Shipping.OnSubmit -= OnShippingSubmitted;
            UIManager.Shipping.OnSubmit += OnShippingSubmitted;

        }

        // Handlers
        private void OnCreditCard(CardDetailsForm.Model data)
        {
            _cachedCard = null;
            _cachedAddress = false;



            Data.Card = data.Card;
            Data.BillingAddress = data.Address;
            RefreshText();
        }

        private void OnShippingSubmitted(AddressForm.Model data)
        {
            _cachedAddress = false;
            Data.ShippingAddress = data.Address;
            UpdatePriceBreakdown();
        }
    }
}
