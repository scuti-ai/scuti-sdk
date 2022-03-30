
using Scuti.GraphQL.Generated;
using Scuti.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI {
    public class InterestsSelector : Selector<HashSet<Categories>> {

        private const string UNASSIGNED = "UNASSIGNED";
        private const int MAX = 3;
        [SerializeField] int minSelection;
        [ReorderableList] [SerializeField] InterestInfo[] interestInfos;
        [SerializeField] Transform container;
        [SerializeField] LabelledImageWidget prefab;
        [SerializeField] Button saveButton;
        [SerializeField] Button prevButton;
        [SerializeField] Text saveButtonLabel;

        public Text Instructions;

        public Text CountLabel;
        public Text CurrentLabel;

        private bool _init = false;

        private int _errorCount = 0;


        protected override HashSet<Categories> GetDefaultSelection() {
            return new HashSet<Categories>();
        }

        public override void Open()
        {
            base.Open();
            Debug.LogError("Open Interest Selector");
            Instructions.color = Color.white;

            if (ScutiNetClient.Instance.FinishedOnBoarding)
            {
                if (prevButton)
                    prevButton.gameObject.SetActive(false);
                if (saveButtonLabel)
                    saveButtonLabel.text = "SAVE";
            }
            else
            {
                if(prevButton)
                    prevButton.gameObject.SetActive(true);
                if(saveButtonLabel)
                    saveButtonLabel.text = "SAVE";
            }
            if (!_init)
            {
#pragma warning disable 4014
                Populate();
#pragma warning restore 4014
            }
        }

        private async Task Populate()
        {
            _init = true;

            List<string> selectedCategories = new List<string>(); ;
            try
            {
                var prefs = await ScutiAPI.GetCategories();
                if (prefs != null && prefs.Categories!=null) selectedCategories = prefs.Categories.ToList();
            } catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }

            // intentionally skipping first/default
            foreach (var catData in UIManager.CategoryManager.ActiveCategories)
            {
                LabelledImageWidget widget = Instantiate(prefab, container);
                widget.SetLabel(catData.Category);
                widget.SetIcon(catData.Icon);
                widget.onInteract += OnCategoryToggled;

                if(selectedCategories.Contains(catData.Category.ToString()))
                {
                    widget.Select(true);
                    selection.Add((Categories)widget.GetEnumValue());
                }  
            }
        }

        private void OnCategoryToggled(LabelledImageWidget widget)
        {
            if (widget.IsSelected)
                selection.Add((Categories)widget.GetEnumValue());
            else selection.Remove((Categories)widget.GetEnumValue());
        }
         
        protected override bool Evaluate() {
            return selection.Count >= minSelection;
        }

        public void NextStep()
        {
            int count = selection.Count;
           
            if (count < 3)
            {
                Instructions.color = Color.red;
                if(_errorCount>1)
                {
                    UIManager.Alert.SetHeader("Select Categories").SetBody("You must select at least 3 categories before continuing.").SetButtonText("OK").Show(() => { });
                }
                _errorCount++;
            } else
            {
                Instructions.color = Color.white;
#pragma warning disable 4014
                SaveChanges();
#pragma warning restore 4014
            }
        }

        private bool _saving = false;
        private async Task SaveChanges()
        {
            if (_saving) return;
            saveButton.interactable = false;
            _saving = true;
            bool submit = false;
            try
            {
                int count = selection.Count;
                string[] categories = new string[count];
                int i = 0;
                foreach (var cat in selection)
                {
                    categories[i] = cat.ToString();
                    i++;
                }
                await ScutiAPI.SetCategories(categories);
                submit = true; 
            } catch (Exception e)
            {
                ScutiLogger.LogException(e);
            }


            // Don't submit in the try catch as it could catch errors unrelated to the actual call
            if (submit)
            {
                Submit();
            }


            saveButton.interactable = true;
            _saving = false;
        }
    }
}
