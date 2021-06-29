using System;

using UnityEngine;

using Scuti;
using Scuti.GraphQL.Generated;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;


namespace Scuti.UI {
    // NOTE: This class is quite unclear at the moment. Further work should be done
    // only after the requirements are certain.
    public class OfferCustomizationPresenter : Presenter<OfferCustomizationPresenter.Model> {
        [Serializable]
        public class Model : Presenter.Model {
            public int Quantity;
            public ProductVariant[] Variants;

            [NonSerialized]
            public int SelectedVariant;
            public int SelectedOption;
            internal ProductVariant GetVariant()
            {
                if(SelectedVariant>-1 && Variants.Length > SelectedVariant)
                {
                    var variant = Variants[SelectedVariant];
                    if (SelectedOption > -1 && variant.Options!=null && variant.Options.Count > SelectedOption)
                    {
                        int i = 0;
                        foreach(var opt in variant.Options)
                        {
                            if (i == SelectedOption) return opt;
                            i++;
                        }
                    }
                    return variant;
                }
                return null;
            }

            internal ProductVariant[] GetOptions()
            {
                if (SelectedVariant > -1 && Variants.Length > SelectedVariant)
                {
                    var variant = Variants[SelectedVariant];
                    if(variant !=null && variant.Options!=null)
                        return variant.Options.ToArray();
                }
                return null;
            }
        }

        [SerializeField] IntegerStepperWidget quantityStepper;


        [SerializeField] Text firstVariantLabel;
        [SerializeField] Dropdown firstVariant;
        [SerializeField] Text secondVariantLabel;
        [SerializeField] Dropdown secondVariant;
        [SerializeField] Text thirdVariantLabel;
        [SerializeField] Dropdown thirdVariant;


        public Action VariantChanged;

        protected override void Awake()
        {
            base.Awake();
            HandleInteractions();
        }

        void HandleInteractions() {
            quantityStepper.OnValueChanged += value => {
                value = Mathf.Clamp(value, 1, int.MaxValue);
                Data.Quantity = value;
            };
            firstVariant.onValueChanged.AddListener(OnFirstVariantChanged);
            secondVariant.onValueChanged.AddListener(OnSecondVariantChanged);
        }

        public void SetIsVideo(bool value)
        {
            quantityStepper.gameObject.SetActive(!value);
        }

        private void OnSecondVariantChanged(int value)
        {
            Data.SelectedOption = value;
            VariantChanged?.Invoke();
        }

        private void OnFirstVariantChanged(int value)
        {
            Data.SelectedVariant = value;
            Data.SelectedOption = 0;
            Populate(secondVariant, Data.GetOptions(), 1, secondVariantLabel);
            Populate(thirdVariant, null, 1, thirdVariantLabel);
            VariantChanged?.Invoke();
        }

        protected override void OnSetState() {
            quantityStepper.Value = Data.Quantity;
            Data.SelectedOption = 0;
            Data.SelectedVariant = 0;

            Populate(firstVariant, Data.Variants, 2, firstVariantLabel);
            Populate(secondVariant, Data.GetOptions(), 1, secondVariantLabel);
            Populate(thirdVariant, null, 1, thirdVariantLabel);

            VariantChanged?.Invoke();
        }

        private void Populate(Dropdown dropdown, ProductVariant[] options, int min, Text title)
        {
            dropdown.Hide();
            dropdown.ClearOptions();
            if (options == null || options.Length < min)
            {
                dropdown.gameObject.SetActive(false);
            }
            else
            {
                dropdown.gameObject.SetActive(true);
                title.text = string.Empty; // TODO: Populate once we get data from server -mg
                var optionList = new List<string>();
                foreach (var variant in options)
                {
                    if (variant != null && variant.InStock.Value > 0)
                        optionList.Add(variant.Name);
                }
                dropdown.AddOptions(optionList);
            }
            dropdown.RefreshShownValue();
        }
    }
}
