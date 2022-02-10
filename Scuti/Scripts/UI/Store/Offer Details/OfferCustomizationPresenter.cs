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

            private List<StockModelUpdated> _stockOutModelUpdated;
            private List<StockModelUpdated> _stockIn;

            private List<StockModelUpdated> distinctOut;
            private List<StockModelUpdated> distinctIn;

            public int Quantity;
            public ProductVariant[] Variants
            {
                set
                {
                    _stockOutModelUpdated = new List<StockModelUpdated>();
                    _stockIn = new List<StockModelUpdated>();

                    _variantMap.Clear();
                    _selectedOption1 = _selectedOption2 = _selectedOption3 = null;
                    if (value != null)
                    {
                        foreach (var variant in value)
                        {
                            //if (variant.InStock.GetValueOrDefault(0) > 0)
                            //{

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

                            stockUpdated.labelOpt1 = oneLabel;
                            stockUpdated.labelOpt2 = secondLabel;
                            stockUpdated.labelOpt3 = thirdLabel;

                            if (variant.InStock.GetValueOrDefault(0) == 0)
                            {
                                _stockOutModelUpdated.Add(stockUpdated);
                            }
                            else
                            {
                                _stockIn.Add(stockUpdated);
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

                        //Debug.Log("Stock Before: " + _stockIn.Count);
                        _stockIn = RemoveDuplicatedItems(_stockIn);
                        //Debug.Log("Stock After: " + _stockIn.Count);


                        //Debug.Log("Stock Before: " + _stockOutModelUpdated.Count);
                        _stockOutModelUpdated = RemoveDuplicatedItems(_stockOutModelUpdated);
                        //Debug.Log("Stock After: " + _stockOutModelUpdated.Count);
     
                        // Delete duplicates in different list: Priority is given to those in stock
                        
                        for (int j = 0; j < _stockIn.Count; j++)
                        {
                            int index = _stockOutModelUpdated.FindIndex(f => f.labelOpt1 == _stockIn[j].labelOpt1 &&
                                                                            f.labelOpt2 == _stockIn[j].labelOpt2 &&
                                                                            f.labelOpt3 == _stockIn[j].labelOpt3);

                            if (index >= 0)
                            {
                                _stockOutModelUpdated.RemoveAt(index);
                            }
                        }    

                    }
                }
            }


            private List<StockModelUpdated> RemoveDuplicatedItems(List<StockModelUpdated> list)
            {
                List<StockModelUpdated> tempList = new List<StockModelUpdated>();

                foreach (StockModelUpdated u1 in list)
                {
                    bool duplicatefound = false;
                    foreach (StockModelUpdated u2 in tempList)
                        if (u1.labelOpt1 == u2.labelOpt1 && u1.labelOpt2 == u2.labelOpt2 && u1.labelOpt3 == u2.labelOpt3)
                            duplicatefound = true;

                    if (!duplicatefound)
                        tempList.Add(u1);
                }

                return tempList;
            }

            public string Option1;
            public string Option2;
            public string Option3;
            private Dictionary<string, Dictionary<string, Dictionary<string, ProductVariant>>> _variantMap = new Dictionary<string, Dictionary<string, Dictionary<string, ProductVariant>>>();

            private string _selectedOption1;
            private string _selectedOption2;
            private string _selectedOption3;

            public List<StockModelUpdated> GetInfoItemOutOfStock()
            {
                return _stockOutModelUpdated;
            }
            public List<StockModelUpdated> GetInfoItemIn()
            {
                return _stockIn;
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

        [SerializeField] List<StockModelUpdated> stockOut;
        [SerializeField] List<string> _option2OutOfStock;
        [SerializeField] List<string> _option3OutOfStock;

        [SerializeField] private int _dropdownHidden;
        [SerializeField] private bool _isInitalize;

        [Header("Colors")]
        [SerializeField] Color colorLight = new Color(0, 0, 0, 255);
        [SerializeField] Color32 colorOpaque = new Color32(0, 0, 0, 180);

        [SerializeField] List<StockModelUpdated> allOutOfStock;
        [SerializeField] List<StockModelUpdated> allInStock;

        public Action VariantChanged;

        protected override void Awake()
        {
            base.Awake();
            HandleInteractions();
            _dropdownHidden = 0;
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
            Debug.Log("Show second options");

            Data.SelectOption2(secondVariant.options[value].text);
            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);

            if (Data.GetInfoItemIn().Count > 0)
            {
                var thirdOpt = Data.GetInfoItemIn().Find(f => f.labelOpt2 == secondVariant.options[value].text);
                thirdVariant.value = thirdVariant.options.FindIndex(f => f.text == thirdOpt.labelOpt3);
            }

            VariantChanged?.Invoke();
        }

        private void OnFirstVariantChanged(int value)
        {
            Data.SelectOption1(firstVariant.options[value].text);
            Populate(secondVariant, Data.GetOption2DropDowns(), 2);
            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);

            if (Data.GetInfoItemIn().Count > 0)
            {
                var secondOpt = Data.GetInfoItemIn().Find(f => f.labelOpt1 == firstVariant.options[value].text);
                if(secondOpt != null)
                {
                    secondVariant.value = secondVariant.options.FindIndex(f => f.text == secondOpt.labelOpt2);
                }
                //thirdVariant.value = thirdVariant.options.FindIndex(f => f.text == Data.GetInfoItemIn()[0].labelOpt3);
                thirdVariant.value = thirdVariant.options.FindIndex(f => f.text == Data.GetCurrentSelectOption3());
            }


            VariantChanged?.Invoke();
        }

        protected override void OnSetState()
        {
            _isInitalize = false;
            _dropdownHidden = 0;

            allOutOfStock = Data.GetInfoItemOutOfStock();
            allInStock = Data.GetInfoItemIn();

            quantityStepper.Value = Data.Quantity;

            firstVariantLabel.text = string.IsNullOrEmpty(Data.Option1) ? string.Empty : Data.Option1;
            secondVariantLabel.text = string.IsNullOrEmpty(Data.Option2) ? string.Empty : Data.Option2;
            thirdVariantLabel.text = string.IsNullOrEmpty(Data.Option3) ? string.Empty : Data.Option3;

            SetItemRandomAvailable();

            Debug.Log("--- CONTROL 1---------------------------------");

            Debug.Log("--- Select option 1: " + Data.GetCurrentSelectOption1());
            Debug.Log("--- Select option 2: " + Data.GetCurrentSelectOption2());
            Debug.Log("--- Select option 3: " + Data.GetCurrentSelectOption3());

            Debug.Log("--- CONTROL 2---------------------------------");
            Populate(thirdVariant, Data.GetOption3DropDowns(), 3);
            Populate(secondVariant, Data.GetOption2DropDowns(), 2);
            Populate(firstVariant, Data.GetOption1DropDowns(), 1);
            Debug.Log("--- CONTROL 3---------------------------------");
            _isInitalize = true;
            Debug.Log("--- CONTROL 4---------------------------------");
            // Initialize dropdowns with items availables
            if (Data.GetInfoItemIn().Count > 0)
            {
                //int value1 = firstVariant.value = firstVariant.options.FindIndex(f => f.text == Data.GetInfoItemIn()[0].labelOpt1);
                Debug.Log("------------------- Select option 3 STAR: " + Data.GetCurrentSelectOption3());
                int value1 = firstVariant.value = firstVariant.options.FindIndex(f => f.text == Data.GetCurrentSelectOption1());
                Debug.Log("-----VALUE 1: " + value1);
                //int value2 = secondVariant.value = secondVariant.options.FindIndex(f => f.text == Data.GetInfoItemIn()[0].labelOpt2);
                Debug.Log("------------------- Select option 3 STAR: " + Data.GetCurrentSelectOption3());
                int value2 = secondVariant.value = secondVariant.options.FindIndex(f => f.text == Data.GetCurrentSelectOption2());
                Debug.Log("-----VALUE 2: " + value2);
                //Debug.Log("------------------- Select option 3: " + Data.GetCurrentSelectOption3());
                //int value3 = thirdVariant.value = thirdVariant.options.FindIndex(f => f.text == Data.GetInfoItemIn()[0].labelOpt3);
                Debug.Log("------------------- Select option 3 STAR: " + Data.GetCurrentSelectOption3());
                int value3 = thirdVariant.value = thirdVariant.options.FindIndex(f => f.text == Data.GetCurrentSelectOption3());
                Debug.Log("-----VALUE 3: " + value3);

            }
            Debug.Log("--- CONTROL 5---------------------------------");
            VariantChanged?.Invoke();
           
        }

        public void SetItemRandomAvailable()
        {
            StockModelUpdated stock = Data.GetInfoItemIn().Find(f => f.labelOpt3 != "null" && f.labelOpt2 != "null" && f.labelOpt1 != "null");
            if (stock != null)
            {

                Data.SelectOption1(stock.labelOpt1);
                Data.SelectOption2(stock.labelOpt2);
                Data.SelectOption3(stock.labelOpt3);
            }

        }


        public void UpdateValues()
        {
            OnFirstVariantChanged(firstVariant.value);
        }

        private void Populate(TMP_Dropdown dropdown, string[] options, int dropdownId)
        {
            if(dropdownId == 3)
            {
                _option3OutOfStock = new List<string>(options);
            }
            else if (dropdownId == 2)
            {
                _option2OutOfStock = new List<string>(options);
            }

            dropdown.Hide();
            dropdown.ClearOptions();
            if (options == null || options.Length < 1 || options[0].Equals(Model.DEFAULT))
            {
                dropdown.gameObject.SetActive(false);
                if (!_isInitalize)
                    _dropdownHidden++;
            }
            else
            {
                dropdown.gameObject.SetActive(true);

                List<ColorOptionDataTMP> optionsColor = new List<ColorOptionDataTMP>();
                for (int i = 0; i < options.Length; i++)
                {
                    // For firstVariant Dropdown
                    if (dropdownId == 1)
                    {
                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);
                        if (_dropdownHidden <= 1)
                        {
                            // Look in the out of stock list if all the variants of the second option are out of stock.
                            if (_option2OutOfStock.Count > 0)
                            {
                                for (int j = 0; j < options.Length; j++)
                                {
                                    /// NO ESTOY TOMANDO EN CUENTA QUE EXISTE UNA TERCERA OPCION
                                    List<StockModelUpdated> stock2 = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt1 == options[i]);
                                    if (_option2OutOfStock.Count == stock2.Count)
                                    {
                                        colorOption.text = options[i];
                                        colorOption.Color = colorOpaque;
                                        colorOption.Interactable = false;
                                        break;
                                    }

                                }
                            }
                        }
                        // Only the first dropdown active
                        else if(_dropdownHidden == 2)
                        {
                            for (int j = 0; j < options.Length; j++)
                            {
                                StockModelUpdated stock2 = Data.GetInfoItemOutOfStock().Find(f => f.labelOpt1 == options[i]);
                                if (stock2 != null)
                                {
                                    colorOption.text = options[i];
                                    colorOption.Color = colorOpaque;
                                    colorOption.Interactable = false;
                                    break;
                                }
                            }
                        }

                        optionsColor.Add(colorOption);
                    }

                    if (dropdownId == 2)
                    {
                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);
                        if (_dropdownHidden == 1)
                        { 
                            // Look in the out of stock list for the second variants of the product selected in the first variant are out of stock;
                            stockOut = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt1 == Data.GetCurrentSelectOption1());
                            StockModelUpdated stock2 = stockOut.Find(f => f.labelOpt2 == options[i]);

                            if (stock2 != null)
                            {
                                colorOption.text = options[i];
                                colorOption.Color = colorOpaque;
                                colorOption.Interactable = false;
                            }
                        }
                        else if(_dropdownHidden == 0)
                        {
                            // Look in the out of stock list for the second variants of the product selected in the first variant are out of stock;
                            stockOut = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt1 == Data.GetCurrentSelectOption1());
                            StockModelUpdated stock2 = stockOut.Find(f => f.labelOpt2 == options[i]);
                            List<StockModelUpdated> stock3 = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt2 == options[i]);

                           int count = 0;
                           bool isOut = false;
                           if (_option3OutOfStock.Count > 0 )
                           {                               
                               for(int k = 0; k < _option3OutOfStock.Count; k++)
                               {
                                   if(Data.GetInfoItemOutOfStock().Exists(f => f.labelOpt1 == Data.GetCurrentSelectOption1() &&
                                                                                              f.labelOpt2 == options[i] &&
                                                                                              f.labelOpt3 == _option3OutOfStock[k]))
                                   {
                                       Debug.Log("******* " +Data.GetCurrentSelectOption1() + " - " + options[i] + " - " + _option3OutOfStock[k]);
                                       count++;
                                   }
                               }
                           }
                           if (count == _option3OutOfStock.Count)
                               isOut = true;


                            if (stock2 != null && /*_option3OutOfStock.Count == stock3.Count*/ isOut)
                            {
                                colorOption.text = options[i];
                                colorOption.Color = colorOpaque;
                                colorOption.Interactable = false;
                            }
                        }

                        optionsColor.Add(colorOption);
                    }

                    if (dropdownId == 3)
                    {
                        Debug.Log("Option " + i + ": " + options[i]);

                        Debug.Log("Option 1 Selected: " + Data.GetCurrentSelectOption1());
                        Debug.Log("Option 2 Selected: " + Data.GetCurrentSelectOption2());
                       


                        ColorOptionDataTMP colorOption = new ColorOptionDataTMP(options[i], colorLight, true);

                        stockOut = Data.GetInfoItemOutOfStock().FindAll(f => f.labelOpt2 == Data.GetCurrentSelectOption2() && f.labelOpt1 == Data.GetCurrentSelectOption1());
                        //stockOut = allOutOfStock.FindAll(f => f.labelOpt2 == Data.GetCurrentSelectOption2() && f.labelOpt1 == Data.GetCurrentSelectOption1());
                        StockModelUpdated stock3 = stockOut.Find(f => f.labelOpt3 == options[i]);
                        if (stock3 != null)
                        {
                            Debug.Log("This option: " + options[i] + " is Disable");
                            colorOption.text = options[i];
                            colorOption.Color = colorOpaque;
                            colorOption.Interactable = false;
                        }
                        else 
                        {
                            Debug.Log("This option: " + options[i] + " is AVAILABLE");
                        }

                        optionsColor.Add(colorOption);
                    }
                }
                dropdown.AddOptions(new List<TMP_Dropdown.OptionData>(optionsColor));
            }

            dropdown.RefreshShownValue();
;
        }
    }
}
