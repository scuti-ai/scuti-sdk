using System;

using UnityEngine;

using Scuti.UI;
using UnityEngine.EventSystems;

namespace Scuti.UI {

    public class UIManager : MonoBehaviour {
        static UIManager instance;

        [SerializeField] UIGenerator generator;
        [SerializeField] Navigation navigation;
        [SerializeField] EffectsManager MainEffectsManager;
        [SerializeField] CategoryFallbackManager Categories;

        private bool _useLargeLayout;
        public bool EditorUseLargeDisplay = false;

        public View LoadingBlocker;

        public GameObject EventSystemObject;

        void Awake() {
            instance = this;
            LoadingBlocker.Close();

            var eventSystems = FindObjectsOfType<EventSystem>();
            if(eventSystems!=null && eventSystems.Length>1)
            {
                EventSystemObject.SetActive(false);
            }
        }

        protected int _loadingCount = 0;
        public static void ShowLoading(bool useCount)
        {
            if (useCount) instance._loadingCount++;
            instance.LoadingBlocker.Open();
        }

        public static void HideLoading(bool useCount)
        {
            if (useCount)
            {
                instance._loadingCount = Math.Max(instance._loadingCount-1, 0);
                RefreshLoading();
            }
            else 
                instance.LoadingBlocker.Close();
        }

        public static void RefreshLoading()
        {
            if (instance._loadingCount>0)
            {
                instance.LoadingBlocker.Open();
            } else
            {
                instance.LoadingBlocker.Close();
            }
        }
        

        private void Start()
        {
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<OfferSummaryPresenterBase.Model>();
        }

       

        public static Navigation Navigator
        {
            get
            {
                return instance.navigation;
            }
        }

        public static EffectsManager Effects
        {
            get
            {
                return instance.MainEffectsManager;
            }
        }

        public static CategoryFallbackManager CategoryManager
        {
            get
            {
                return instance.Categories;
            }
        }

        public static void Open(string viewSetID, string viewID) {
            View view = instance.generator[viewSetID][viewID];
            instance.navigation.Open(view);
        }

        public static void Open(View view) {
            instance.navigation.Open(view);
        }

        public static void Back() {
            instance.navigation.Back();
        }

        public static Action<bool> onBackButton;
        public static bool isLogged;
        public static bool isCheckoutSuccess;

        // OVERLAY

        public static ViewSet Overlay {
            get { return instance.generator["OVERLAY", true]; }
        }

        public static SplashView Splash {
            get { return Overlay["SPLASH"] as SplashView; }
        }


        public static View TopMenu {
            get { return Overlay["TOPMENU"]; }
        }

        public static AlertView Alert {
            get { return Overlay["ALERT"] as AlertView; }
        }

        public static ConfirmationView Confirmation {
            get { return Overlay["CONFIRMATION"] as ConfirmationView ; }
        }

        public static SelectionListView List {
            get { return Overlay["SELECTION-LIST"] as SelectionListView; }
        }

        public static TextInputView TextInput
        {
            get { return Overlay["TEXTINPUT"] as TextInputView; }
        }

        public static AddressForm Shipping
        {
            get { return Overlay["SHIPPING"] as AddressForm; }
        }

        public static TextInputView CVV
        {
            get { return Overlay["CVV"] as TextInputView; }
        }

        public static ScutiWebView WebForm
        {
            get { return Overlay["WEBFORM"] as ScutiWebView; }
        }


        public static SupportContactView SupportContact
        {
            get { return Overlay["SUPPORT"] as SupportContactView; }
        }

        public static LogoutView LogoutPopup
        {
            get { return Overlay["LOGOUT-POPUP"] as LogoutView; }
        }

        // ACCOUNT
        public static ViewSet Account {
            get { return instance.generator["ACCOUNT", true]; }
        }

        public static OrdersPresenter Orders
        {
            get { return Account["ORDERS"] as OrdersPresenter; }
        }

        public static LoginPromotionView PromoAccount
        {
            get { return Account["PROMOACCOUNT"] as LoginPromotionView; }
        }


        public static OnboardingView Onboarding {
            get { return Account["ONBOARDING"] as OnboardingView; }
        }

        public static WelcomeView Welcome
        {
            get { return Account["WELCOME"] as WelcomeView;  }
        }

        public static WalletView Wallet {
            get { return Account["WALLET"] as WalletView; }
        }

        public static LoginForm Login
        {
            get {  return Account["LOGIN"] as LoginForm; }
        }

        public static CardDetailsForm Card
        {
            get { return Account["CARD"] as CardDetailsForm; }
        }

        public static CardManager CardManager
        {
            get { return Account["CARD-MANAGER"] as CardManager; }
        }

        // STORE
        public static VideoView VideoPlayer
        {
            get { return Store["VIDEOPLAYER"] as VideoView; }
        }
        public static View RewardsAd
        {
            get { return Store["REWARDSAD"]; }
        }

        public static TopBarView TopBar
        {
            get { return Store["TOPBAR"] as TopBarView; }
        }

        public static ViewSet Store {
            get { return instance.generator["STORE", true]; }
        }

        public static OffersPresenter Offers {
            get { 
                return Store["OFFERS"] as OffersPresenter; 
            }
        }

        public static OfferDetailsPresenter OfferDetails {
            get { return Store["DETAILS"] as OfferDetailsPresenter; }
        }

        public static CartPresenter Cart {
            get { return Store["CART"] as CartPresenter; }
        }

        public static RewardPresenter Rewards
        {
            get {
                return Store["REWARD"] as RewardPresenter;  }
        }


        private void LeaveApplication()
        {
            if (Cart != null)
            {
                Cart.RecordApplicationBackgrounded();
            }
        }

        // Handlers
        private void OnApplicationQuit()
        {
            LeaveApplication();
        }

        void OnApplicationFocus(bool focus)
        {
            //if (focus) ResumeApplication();
            //else 
            LeaveApplication();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause) LeaveApplication();
            //else ResumeApplication();
        }
    }
}