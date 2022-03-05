using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Scuti.Net;
using System.Collections;

// WIP
namespace Scuti.UI
{
    public class WelcomeView : View
    {

        [Serializable]
        // hack to get nested objects
        public class ChildrenInfo
        {
            public List<GameObject> Children;
        }


        protected Coroutine _currencyRoutine;
        private int retryCount = 0;

        //public GameObject ExchangeInstructions;
        public Image ExchangeIcon;

        //public List<ChildrenInfo> Pages = new List<ChildrenInfo>();
        //private int index = 0;


        public override void Open()
        {
            base.Open();

            Debug.LogError("WElcome?? ");
            try
            {
                PlayerPrefs.SetInt(ScutiConstants.KEY_WELCOME, 1);
            }
            catch (Exception e)
            {
                ScutiLogger.LogError(e);
            }

            //if (ExchangeInstructions) ExchangeInstructions.gameObject.SetActive(false);
            //#if !UNITY_IOS
            //            _currencyRoutine = StartCoroutine(LoadCurrencyIcon());
            //        }

            //        private IEnumerator LoadCurrencyIcon()
            //        {
            //            Sprite sprite = null;

            //            while (sprite == null && retryCount < 10)
            //            {
            //                sprite = ScutiNetClient.Instance.CurrencyIconToSprite();
            //                if (sprite != null)
            //                {
            //                    ExchangeIcon.sprite = sprite;
            //                    ExchangeInstructions.gameObject.SetActive(true);
            //                }
            //                retryCount++;
            //                yield return new WaitForSeconds(0.5f);
            //            }
            //        }
            //#else
            //        }
            //#endif
        }

        //public void TogglePage()
        //{
        //    index++;
        //    if (index > Pages.Count - 1) index = 0;
        //    for(var i = 0; i< Pages.Count; i++)
        //    {
        //        var children = Pages[i].Children;
        //        var active = i == index;
        //        foreach (var child in children)
        //        {
        //            child?.SetActive(active);
        //        }
        //    }
        //}

        public void Register()
        {
            UIManager.Open(UIManager.Onboarding);
            Close();
        }

        public void ContinueWithoutRegistering()
        {
            UIManager.RefreshLoading();
            Close();
        }

        public void ReturningUser()
        {
            UIManager.Open(UIManager.Login);
            Close();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_currencyRoutine != null)
            {
                StopCoroutine(_currencyRoutine);
                _currencyRoutine = null;
            }
        }
    }
}
