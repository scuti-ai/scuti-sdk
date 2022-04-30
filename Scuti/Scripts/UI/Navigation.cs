using Scuti.Net;
using Scuti.UI;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace Scuti {
    public class Navigation : MonoBehaviour {

        public class BreadCrumbs
        {
            public string Path;
            public int Count;
        }

        [SerializeField] bool allowHistoryRepeatition;
        [ReorderableList] [SerializeField] List<View> history = new List<View>();
        //private BreadCrumbs _breadCrumbs = new BreadCrumbs();

        [ReadOnly] [SerializeField] View currentNonModal;
        public View CurrentNonModal {
            get { return currentNonModal; }
        }

        [ReadOnly] [SerializeField] View currentModal;
        public View CurrentModal {
            get { return currentModal; }
        }

        View lastView;
        [ReadOnly] [SerializeField] private int counterModals = 0;

        public void Open<T>(T component) where T : Component {
            Open(component.gameObject);
        }

        public void Open(GameObject go) {
            var View = go.GetComponent<View>();
            if (View == null)
                View = go.GetComponentInChildren<View>();
            Open(View);
        }

        public void Open(View view) {

            if(view.requiresLogin)
            {
                // check if logged in 
                if(!ScutiNetClient.Instance.IsAuthenticated)
                {            
                    Open(UIManager.PromoAccount);
                    return;
                }
            }

            UIManager.Offers.GetNavigator().isShowingCategories = false;

            if (view.isModal)
                OpenModal(view);
            else
                OpenNonModal(view);
        }

        void OpenModal(View view) {
            currentModal = view;
            view.OnViewClosed += CloseView;
           
            // Here open Modal
            currentModal.Open();
            counterModals++;
        }

        private void CloseView(View v)
        {
            v.OnViewClosed -= CloseView;
            counterModals--;
            if (currentModal == v)
            {
                currentModal = null;                
                // Sanity Check
                if (CurrentNonModal == null)
                {
                    OpenNonModal(UIManager.Offers);
                }
            }

            if (counterModals >= 1)
            {
                UIManager.Offers.GetNavigator().isShowingCategories = false;
            }
            else
            {
                UIManager.Offers.GetNavigator().isShowingCategories = true;
            }
        }


        void OpenNonModal(View view) {

            //UIManager.Offers.GetNavigator().isShowingCategories = false;
            // If this is the first element being added,
            // add it to the list and open it
            if (history.Count == 0)
            {
               // UIManager.Offers.GetNavigator().isShowingCategories = true;

                history.Add(view);
                view.Open();
                view.OnDestroyed += () => {
                    history.Remove(view);
                    AssignCurrentNonModal();
                };
                AssignCurrentNonModal();
                return;
            }

            // If the same element is being added again, return
            if (history.Last() == view)
            {
                return;
            }              


            // If the element is the same as the second last one, 
            // we close and remove the last element and open the last element (AKA the incoming one)
            // So, if the history was A>B>C and the incoming one was B, we are removing C and reopening B which makes is A>B
            // instead of adding B again and opening it, which would have made it A>B>C>B (unnecessary repeatition)
            if (history.Count >= 2 && history.FromLast(1) == view && !allowHistoryRepeatition) {

                history.Last().Close();
                history.RemoveAt(history.Count - 1);
                history.Last().Open();
                view.OnDestroyed += () => {
                    history.Remove(view);
                    AssignCurrentNonModal();
                };
                AssignCurrentNonModal();

                return;
            }

            // If the incoming element is not the same as the second last, we just close the last, then add the incoming and open the new last
            // So if the history was A>B>C and the incoming one was D, the history now would become A>B>C>D
            else {

                history.Last().Close();
                //history.RemoveLast();
                history.Add(view);
                history.Last().Open();
                view.OnDestroyed += () => {
                    history.Remove(view);
                    AssignCurrentNonModal();
                };
                ///UIManager.onBackButton?.Invoke(false);
                AssignCurrentNonModal();
            }
        }

        void AssignCurrentNonModal() {
            if (history.Count == 0)
                currentNonModal = null;
            else
                currentNonModal = history.Last();
        }

        public void Close()
        {
            if (history.Count > 0) { 
                history.Last().Close();
                history.RemoveAt(history.Count - 1);
            }

        }

        public void Back()
        {

            if (history.Count > 0)
            {
                history.Last().Close();
                if(history.Count < 2)
                {
                    lastView = history[history.Count - 1];
                }                   
                history.RemoveAt(history.Count - 1);  
            }

            if (history.Count < 1)
            {
                
                //UIManager.onBackButton?.Invoke(true);
                // may happen if we do deep linking or ads
                if (CurrentNonModal == UIManager.OfferDetails)
                {
                    UIManager.Offers.GetNavigator().isShowingCategories = true;
                    Open(UIManager.Offers);
                }  
                else
                {
                    UIManager.LogoutPopup.Show(OnClosePopUp);
                }
                  
                
                return;
            }
            else
            {               
                history.Last().Open();
                AssignCurrentNonModal();
                //if (history.Count < 2)
                  //  UIManager.onBackButton?.Invoke(true);
            }

        }

        public void Clear() {
            history.Clear();
        }

        private void OnClosePopUp(bool val)
        {
            if (val)
                ScutiSDK.Instance.UnloadUI();
            else
            {
                if(lastView != null)
                {
                    OpenNonModal(lastView);                    
                    //history.Last().Open();
                    lastView = null;
                }
            }                
        }

        internal BreadCrumbs GetHistory()
        {
            throw new NotImplementedException();
        }
    }
}