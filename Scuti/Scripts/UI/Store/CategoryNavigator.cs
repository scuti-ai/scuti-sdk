
using Scuti.GraphQL.Generated;
using Scuti.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Scuti.UI
{
    /// <summary>
    /// UI for user to cycle through different store categories
    /// </summary>
    public class CategoryNavigator : MonoBehaviour
    {
        private bool _destroyed;

        public Scuti.UISystem.OpacityTransition opacityTransition;

        public event Action<string> OnOpenRequest;

        private OffersPresenterBase _offersPresenterBase;

        private string[] m_Categories = { "DEFAULT" };

        public TextMeshProUGUI Label;
        public float SwipeThreshold = 50;

        int m_Index;
        private bool _invalid = true;

        [BoxGroup("Events")] public UnityEvent onShow;
        [BoxGroup("Events")] public UnityEvent onHide;

        private void Start()
        {
            ScutiNetClient.Instance.OnAuthenticated += ResetCategories;
            ScutiNetClient.Instance.OnLogout += ResetCategories;
        }

        //---

        public float GetCanvasGroup()
        {
            return opacityTransition.group.alpha;
        }

        //---

        public async void OpenCurrent()
        {
            if (!ScutiNetClient.Instance.IsInitialized)
            {
                System.Threading.CancellationTokenSource source = new System.Threading.CancellationTokenSource();
                await System.Threading.Tasks.Task.Delay(500, source.Token);
                source.Cancel();
                OpenCurrent();
                return;
            }

            if (_invalid)
            {
                string[] unordered = null;


                OfferStatistics readCategories = null;
                try
                {
                    readCategories = await ScutiAPI.GetCategoryStatistics();
                }
                catch (Exception ex)
                {
                    //TODO make this more robust we need to display error and close window -asm
                    ScutiLogger.LogException(ex);
                }
                if (_destroyed) return;
                unordered = readCategories.ByCategories.Where(x => x.Count >= ScutiUtils.RequiredAdsPerCategory()).Select(x => x.Category).ToArray();
                Preferences prefs = null;
                try
                {
                    if (ScutiNetClient.Instance.IsAuthenticated)
                        prefs = await ScutiAPI.GetCategories();
                }
                catch (Exception e)
                {
                    ScutiLogger.LogException(e);
                }
                if (prefs == null)
                {
                    prefs = new Preferences();
                }
                if (prefs.Categories == null)
                {
                    prefs.Categories = new List<string>();
                }

                var preferred = prefs.Categories.Intersect(unordered);
                var other = unordered.Except(prefs.Categories);


                List<string> ordered = new List<string>();
                // Add default to category list
                foreach(var custom in ScutiConstants.CUSTOM_CATEGORIES)
                {
                    ordered.Add(custom.DisplayName.ToUpper());
                }
                //ordered.Add("DEFAULT");

                ordered.AddRange(preferred);
                ordered.AddRange(other);
                m_Categories = ordered.ToArray();

                _invalid = false;
            }
            var category = m_Categories[m_Index];
            OnOpenRequest?.Invoke(category);

            if (category != "DEFAULT")
                Label.text = category.ToUpper();
            else
                Label.text = "TODAY'S DEALS";
        }

        public void Next()
        {
            ChangeCategory(1);
        }

        public void Previous()
        {
            ChangeCategory(-1);
        }

        // Helpers
        private void ChangeCategory(int delta)
        {
            if(_offersPresenterBase!=null)
            {
                if (_offersPresenterBase.IsUnableToChangeCategory()) return;
            }


            m_Index += delta;
            ValidateIndex();
            OpenCurrent();
        }

        public void Hide()
        {
            if (GetCanvasGroup() > 0)
                onHide?.Invoke();
        }

        public void Show()
        {
            if(GetCanvasGroup() < 1)
                onShow?.Invoke();
        }


        private void ValidateIndex()
        {
            var max = m_Categories.Length - 1;
            if (m_Index < 0) m_Index = max;
            else if (m_Index > max) m_Index = 0;
        }

        internal void ResetCategories()
        {
            _invalid = true;
            m_Index = 0;
            if (UIManager.Store != null)
            {
                UIManager.Offers?.ResetPagination();
            }
            OpenCurrent();
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }


        private Vector2 fingerDown;
        private DateTime fingerDownTime;
        private Vector2 fingerUp;
        private DateTime fingerUpTime;

        public float timeThreshold = 0.3f;

        private void Update()
        {

#if UNITY_STANDALONE || UNITY_EDITOR

            if (Input.GetMouseButtonDown(0))
            {
                fingerDown = Input.mousePosition;
                fingerUp = Input.mousePosition;
                fingerDownTime = DateTime.Now;
            }
            if (Input.GetMouseButtonUp(0))
            {
                fingerDown = Input.mousePosition;
                fingerUpTime = DateTime.Now;
                CheckSwipe();
            }
            
#endif

            if (Input.touchCount > 0)
	        {
		        var touch = Input.touches[0];
		        if (touch.phase == TouchPhase.Began)
		        {

		        }
		        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
		        {
			        Debug.Log(touch.deltaPosition.x);
			        if (touch.deltaPosition.x < -SwipeThreshold)
			        {
				        Next();
			        }
			        else if (touch.deltaPosition.x > SwipeThreshold)
			        {
				        Previous();
			        }
		        }
	        }
        }

        internal void SetPresenter(OffersPresenterBase offersPresenterBase)
        {
            _offersPresenterBase = offersPresenterBase;
        }

        private void CheckSwipe()
        {
            float duration = (float) fingerUpTime.Subtract(fingerDownTime).TotalSeconds;
            if (duration > timeThreshold) return;

            Vector3 dirVector = fingerDown - fingerUp;
            var direction = Vector3.Angle(Vector3.right, dirVector);
            var distance = Vector3.Distance(fingerDown, fingerUp);

            if(Vector3.Magnitude(dirVector) >= SwipeThreshold)
            {
                // Calculate swipe by angle
                if (direction >= 135 && direction < 225)
                {
                    Next();
                }
                else if (direction >= 315 && direction < 360 || direction >= 0 && direction < 45)
                {
                    Previous();
                }
            }
          
        }

    }
}
