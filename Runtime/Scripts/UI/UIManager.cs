using System;

using UnityEngine;

using Scuti.UI;

namespace Scuti.UI {

    public class UIManager : MonoBehaviour {
        static UIManager instance;

        [SerializeField] UIGenerator generator;
        [SerializeField] Navigation navigation;
        [SerializeField] EffectsManager MainEffectsManager;
        [SerializeField] CategoryFallbackManager Categories;

        private bool _useLargeLayout;
        public bool EditorUseLargeDisplay = false;





        void Awake() {
            instance = this;
            DetermineLayout();
        }

        

        private void Start()
        {
            Newtonsoft.Json.Utilities.AotHelper.EnsureList<OfferSummaryPresenter.Model>();
        }

       

        private void DetermineLayout()
        {
            _useLargeLayout = false;
            // TODO: Re-enable when we finish portrait mode + test PC mode -mg
//#if UNITY_EDITOR
//            _useLargeLayout = EditorUseLargeDisplay;
//#elif UNITY_STANDALONE
//            _useLargeLayout = true;
//#else
//            _useLargeLayout = false;
//#endif
        }

        public static bool IsLargeDisplay()
        {
            return instance._useLargeLayout;
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

        // ACCOUNT
        public static ViewSet Account {
            get { return instance.generator["ACCOUNT", true]; }
        }

        public static OrdersPresenter Orders
        {
            get { return Account["ORDERS"] as OrdersPresenter; }
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

        // STORE
        public static VideoView VideoPlayer
        {
            get { return Store["VIDEOPLAYER"] as VideoView; }
        }

        public static TopBarView TopBar
        {
            get { return Store["TOPBAR"] as TopBarView; }
        }

        public static ViewSet Store {
            get { return instance.generator["STORE", true]; }
        }

        public static OffersPresenter Offers {
            get { return Store["OFFERS"] as OffersPresenter; }
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