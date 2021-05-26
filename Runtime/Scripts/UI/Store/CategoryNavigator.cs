
using Scuti.GraphQL.Generated;
using Scuti.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    /// <summary>
    /// UI for user to cycle through different store categories
    /// </summary>
    public class CategoryNavigator : MonoBehaviour
    {
        private bool _destroyed;

        public event Action<string> OnOpenRequest;

        private string[] m_Categories = { "DEFAULT" };

        public Text Label;

        int m_Index;
        private bool _invalid = true;

        private void Start()
        {
            ScutiNetClient.Instance.OnAuthenticated += ResetCategories;
            ScutiNetClient.Instance.OnLogout += ResetCategories;
        }


        public async void OpenCurrent()
        {
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
                unordered = readCategories.ByCategories.Where(x => x.Count >= ScutiUtils.GetAdsPerPage()).Select(x => x.Category).ToArray();
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
                ordered.Add("DEFAULT");

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
            m_Index += delta;
            ValidateIndex();
            OpenCurrent();
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
    }
}
