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
        public class StockModelUpdated
        {
            [SerializeField] public string labelOpt1;
            [SerializeField] public string labelOpt2;
            [SerializeField] public string labelOpt3;
        }

       
        [Serializable]
        public class Model : Presenter.Model {

            public static string DEFAULT = "null";

            [SerializeField] private List<StockModelUpdated> _stockOutModelUpdated;

            public int Quantity;
            public ProductVariant[] Variants
            {
                set
                {                   
                    _stockOutModelUpdated = new List<StockModelUpdated>();     

                    _variantMap.Clear();
                    _selectedOption1 = _selectedOption2 = _selectedOption3 = null;
                    if (value != null)
                    {
                        foreach (var variant in value)
                        {
                            //if (variant.InStock.GetValueOrDefault(0) > 0)
                            //{
                            if (variant.InStock.GetValueOrDefault(0) == 0)
                            {
                                //Debug.Log("OFFERCUSTOMIZACION: 1 " + variant.Option1);
                                //Debug.Log("OFFERCUSTOMIZACION: 2 " + variant.Option2);
                                //Debug.Log("OFFERCUSTOMIZACION: 3 " + variant.Option3);

                                StockModelUpdated stockUpdated = new StockModelUpdated();

                                string oneLabel = "null";
                                string secondLabel = "null";
                                string thirdLabel = "null";

                                if (!string.IsNullOrEmpty(variant.Option1))
                                {
                                    oneLabel = variant.Option1;
                                    if (!string.IsNullOrEmpty(variant.Option2))
                                    {
                                        secondLabel = variant.Option2;
                                        if (!string.IsNullOrEmpty(variant.Option3))
                                        {
                                            thirdLabel = variant.Option3;
                                        }
                                    }
                                }
                                // ---------------------------------------------
                                Debug.Log("OFFERCUSTOMIZACION: 1 " + oneLabel);
                                Debug.Log("OFFERCUSTOMIZACION: 2 " + secondLabel);
                                Debug.Log("OFFERCUSTOMIZACION: 3 " + thirdLabel);

                                stockUpdated.labelOpt1 = oneLabel;
                                stockUpdated.labelOpt2 = secondLabel;
                                stockUpdated.labelOpt3 = thirdLabel;

                                _stockOutModelUpdated.Add(stockUpdated);
                                // ---------------------------------------------                               

                            }

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
                            //}
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

            public string GetCurrentSelectOption1()
            {
                return _selectedOption1;
            }

            public string GetCurrentSelectOption2()
            {
                return _selectedOption2;
            }

            public string GetCurrentSelectOption3()
            {
                return _selectedOption3;
            }


            public List<StockModelUpdated> GetInfoItemOutOfStock()
            {
                return _stockOutModelUpdated;
            }

        }

        [SerializeField] IntegerStepperWidget quantityStepper;

        [SerializeField] TextMeshProUGUI firstVariantLabel;
        [SerializeField] TMP_Dropdown firstVariant;
        [SerializeField] TextMeshProUGUI secondVariantLabel;
        [SerializeField] TMP_Dropdown secondVariant;
        [SerializeField] TextMeshProUGUI thirdVariantLabel;
        [SerializeField] TMP_Dropdown thirdVariant;

        [SerializeField] List<StockModelUpdated> stockOut;
        [SerializeField] List<string> _option2OutOfStock;


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
            Debug.Log("----- THRIRD VARIANT: " + value);
            Data.SelectOption3(thirdVariant.options[value].text);
            VariantChanged?.Invoke();
            Debug.Log(Data.GetCurrentSelectOption1() + " - " + Data.GetCurrentSelectOption2() + " - " + Data.GetCurrentSelectOption3());

        }

        private void OnSecondVariantChanged(int value)
        {
            Debug.Log("----- SECOND VARIANT: " + value);
            Data.SelectOption2(secondVariant.options[value].text);
            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);
            VariantChanged?.Invoke();
            Debug.Log(Data.GetCurrentSelectOption1() + " - " + Data.GetCurrentSelectOption2() + " - " + Data.GetCurrentSelectOption3());

        }

        private void OnFirstVariantChanged(int value)
        {
            Debug.Log("----- FIRST VARIANT: " + value);
            Data.SelectOption1(firstVariant.options[value].text);
            Populate(secondVariant, Data.GetOption2DropDowns(), 2);
            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);
            VariantChanged?.Invoke();
            Debug.Log(Data.GetCurrentSelectOption1() + " - " + Data.GetCurrentSelectOption2() + " - " + Data.GetCurrentSelectOption3());
        }

        protected override void OnSetState()
        {
            quantityStepper.Value = Data.Quantity;

            firstVariantLabel.text = string.IsNullOrEmpty(Data.Option1) ? string.Empty : Data.Option1;
            secondVariantLabel.text = string.IsNullOrEmpty(Data.Option2) ? string.Empty : Data.Option2;
            thirdVariantLabel.text = string.IsNullOrEmpty(Data.Option3) ? string.Empty : Data.Option3;


            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);
            Populate(secondVariant, Data.GetOption2DropDowns(), 2);
            Populate(firstVariant, Data.GetOption1DropDowns(), 1);

            VariantChanged?.Invoke();

            Debug.Log(Data.GetCurrentSelectOption1() + " - " + Data.GetCurrentSelectOption2() + " - " + Data.GetCurrentSelectOption3());

        }

        private void Populate(TMP_Dropdown dropdown, string[] options, int dropdownId)
        {
            Color colorLight = new Color(0, 0, 0, 255);
            Color32 colorOpaque = new Color32(0, 0, 0, 180);

            if (dropdownId == 2)
            {
                Debug.Log("--- OPTION 2 ----- " + options.Length);
                _option2OutOfStock = new List<string>(options);
            }


            dropdown.Hide();
            dropdown.ClearOptions();            
            if (options == null || options.Length < 1 || options[0].Equals(Model.DEFAULT))
            {
                dropdown.gameObject.SetActive(false);
            }
            else
            {
                dropdown.gameObject.SetActive(true);

                //_option2OutOfStock = new List<string>();
                List<ColorOptionDataTMP> optionsColor = new List<ColorOptionDataTMP>();
                for (int i = 0; i < options.Length; i++)
                {
                    // For firstVariant Dropdown

                    if (dropdownId == 1)
                    {
                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);
                                   
                        if(_option2OutOfStock.Count > 0)
                        {

                            for(int j = 0; j < options.Length; j++)
                            {

                                Debug.Log("----------- DROPDOWN 11111 ------------------------"+ options[i]);

                                List<StockModelUpdated> stock2 = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt1 == options[i]);                                
                                if (_option2OutOfStock.Count == stock2.Count)
                                {
                                    Debug.Log("----------- DROPDOWN 1 ------------------------");
                                    colorOption.text = options[i];
                                    colorOption.Color = colorOpaque;
                                    colorOption.Interactable = false;
                                }

                            }                                     
                        }

                        optionsColor.Add(colorOption);
                    }

                    if (dropdownId == 2)
                    {
                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);

                        stockOut = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt1 == Data.GetCurrentSelectOption1());
                        StockModelUpdated stock2 = stockOut.Find(f => f.labelOpt2 == options[i]);
                        if(stock2 != null)
                        {
                            Debug.Log("----------- DROPDOWN 2 ------------------------");
                            //_option2OutOfStock.Add(stock2.labelOpt2);
                            colorOption.text = options[i];
                            colorOption.Color = colorOpaque;
                            colorOption.Interactable = false;
                        }

                        optionsColor.Add(colorOption);                      
                    }

                    if (dropdownId == 3)
                    {
                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);

                        stockOut = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt2 == Data.GetCurrentSelectOption2());
                        StockModelUpdated stock3 = stockOut.Find(f => f.labelOpt3 == options[i]);
                        if (stock3 != null)
                        {
                            Debug.Log("----------- DROPDOWN 2 ------------------------");

                            colorOption.text = options[i];
                            colorOption.Color = colorOpaque;
                            colorOption.Interactable = false;
                        }

                        optionsColor.Add(colorOption);
                    }
                }

                dropdown.AddOptions(new List<TMP_Dropdown.OptionData>(optionsColor));
                //dropdown.AddOptions(options.ToList());
            }

            //Data.GetInfoItemOutOfStock();

            Debug.Log("********** OfferCustomization: Number Options " + dropdown.options.Count());
            Debug.Log("********** OfferCustomization: Name " + dropdown.name);

           // dropdown.itemText.color = new Color(0, 0, 0, 0);

            var listOptions = dropdown.options;
            for(int i = 0; i < listOptions.Count; i++)
            {
             //   listOptions[i].image.colo     

            }

            dropdown.RefreshShownValue();
        }

        private bool CheckOutOfStock()
        {
            // If is TRUE is OutOfStock
            return Data.GetInfoItemOutOfStock().Exists(f => f.labelOpt1 == Data.GetCurrentSelectOption1() && f.labelOpt2 == Data.GetCurrentSelectOption2() && f.labelOpt3 == Data.GetCurrentSelectOption3());
        }

    }
}
