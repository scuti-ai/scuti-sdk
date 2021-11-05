using System;

using UnityEngine;

using Scuti;
using Scuti.GraphQL.Generated;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace Scuti.UI {
    // NOTE: This class is quite unclear at the moment. Further work should be done
    // only after the requirements are certain.
    public class OfferCustomizationPresenter : Presenter<OfferCustomizationPresenter.Model> {

        [Serializable]
        public class Model : Presenter.Model {

            public static string DEFAULT = "null";

            public int Quantity;
            public ProductVariant[] Variants
            {
                set
                {
                    _variantMap.Clear();
                    _selectedOption1 = _selectedOption2 = _selectedOption3 = null;
                    if (value != null)
                    {
                        foreach (var variant in value)
                        {
                            if (variant.InStock.GetValueOrDefault(0) > 0)
                            {

                                var opt1 = (string.IsNullOrEmpty(variant.Option1)) ? DEFAULT : variant.Option1;
                                var opt2 = (string.IsNullOrEmpty(variant.Option2)) ? DEFAULT : variant.Option2;
                                var opt3 = (string.IsNullOrEmpty(variant.Option3)) ? DEFAULT : variant.Option3;

                                if (_selectedOption1.IsNullOrEmpty()) _selectedOption1 = opt1;
                                if (_selectedOption2.IsNullOrEmpty()) _selectedOption2 = opt2;
                                if (_selectedOption3.IsNullOrEmpty()) _selectedOption3 = opt3;

                                if (!_variantMap.ContainsKey(opt1))
                                {
                                    _variantMap[opt1] = new Dictionary<string, Dictionary<string, ProductVariant>>();
                                }

                                var innerMap = _variantMap[opt1];
                                if (!innerMap.ContainsKey(opt2))
                                {
                                    innerMap[opt2] = new Dictionary<string, ProductVariant>();
                                }

                                var finalMap = innerMap[opt2];
                                finalMap[opt3] = variant;
                            }
                        } 

                    }
                }
            }

            public string Option1;
            public string Option2;
            public string Option3;
            private Dictionary<string, Dictionary<string, Dictionary<string, ProductVariant>>> _variantMap = new Dictionary<string, Dictionary<string, Dictionary<string, ProductVariant>>>();

            private string _selectedOption1;
            private string _selectedOption2;
            private string _selectedOption3;

            public void SelectOption1(string value)
            {
                _selectedOption1 = (string.IsNullOrEmpty(value)) ? DEFAULT : value;
            }

            public void SelectOption2(string value)
            {
                _selectedOption2 = (string.IsNullOrEmpty(value)) ? DEFAULT : value;
            }
            public void SelectOption3(string value)
            {
                _selectedOption3 = (string.IsNullOrEmpty(value)) ? DEFAULT : value;
            }

            internal ProductVariant GetSelectedVariant()
            {
                return _variantMap[_selectedOption1][_selectedOption2][_selectedOption3];
            }

            internal string[] GetOption1DropDowns()
            {
                return _variantMap.Keys.ToArray();
            }

            internal string[] GetOption2DropDowns()
            {
                return _variantMap[_selectedOption1].Keys.ToArray();
            }

            internal string[] GetOption3DropDowns()
            {
                return _variantMap[_selectedOption1][_selectedOption2].Keys.ToArray();
            }

        }

        [SerializeField] IntegerStepperWidget quantityStepper;

        [SerializeField] TextMeshProUGUI firstVariantLabel;
        [SerializeField] TMP_Dropdown firstVariant;
        [SerializeField] TextMeshProUGUI secondVariantLabel;
        [SerializeField] TMP_Dropdown secondVariant;
        [SerializeField] TextMeshProUGUI thirdVariantLabel;
        [SerializeField] TMP_Dropdown thirdVariant;

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
            thirdVariant.onValueChanged.AddListener(OnThirdVariantChanged);
        }

        public void SetIsVideo(bool value)
        {
            quantityStepper.gameObject.SetActive(!value);
        }

        private void OnThirdVariantChanged(int value)
        {
            Data.SelectOption3(thirdVariant.options[value].text);
            VariantChanged?.Invoke();
        }

        private void OnSecondVariantChanged(int value)
        {
            Data.SelectOption2(secondVariant.options[value].text);
            Populate(thirdVariant, Data.GetOption3DropDowns());
            VariantChanged?.Invoke();
        }

        private void OnFirstVariantChanged(int value)
        {
            Data.SelectOption1(firstVariant.options[value].text);
            Populate(secondVariant, Data.GetOption2DropDowns());
            Populate(thirdVariant, Data.GetOption3DropDowns());
            VariantChanged?.Invoke();
        }

        protected override void OnSetState()
        {
            quantityStepper.Value = Data.Quantity;

            firstVariantLabel.text = string.IsNullOrEmpty(Data.Option1) ? string.Empty : Data.Option1;
            secondVariantLabel.text = string.IsNullOrEmpty(Data.Option2) ? string.Empty : Data.Option2;
            thirdVariantLabel.text = string.IsNullOrEmpty(Data.Option3) ? string.Empty : Data.Option3;

            Populate(firstVariant, Data.GetOption1DropDowns());
            Populate(secondVariant, Data.GetOption2DropDowns());
            Populate(thirdVariant, Data.GetOption3DropDowns());

            VariantChanged?.Invoke();
        }

        private void Populate(TMP_Dropdown dropdown, string[] options)
        {

            dropdown.Hide();
            dropdown.ClearOptions();
            if (options == null || options.Length < 1 || options[0].Equals(Model.DEFAULT))
            {
                dropdown.gameObject.SetActive(false);
            }
            else
            {
                dropdown.gameObject.SetActive(true);
                dropdown.AddOptions(options.ToList());
            }

            dropdown.RefreshShownValue();
        }
    }
}
