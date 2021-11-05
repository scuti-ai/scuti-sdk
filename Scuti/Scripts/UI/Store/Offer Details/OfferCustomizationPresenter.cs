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
        public class StockModel
        {
            [SerializeField] public string labelOpt1;
            [SerializeField] public List<SecondOption> opt2;
        }

        [Serializable]
        public class SecondOption
        {
            [SerializeField] public string labelOpt2;
            [SerializeField] public List<string> opt3;

        }



        [Serializable]
        public class Model : Presenter.Model {

            public static string DEFAULT = "null";

            [SerializeField] private List<StockModel> stockOutModel;

            [SerializeField] private List<StockModelUpdated> stockOutModelUpdated;

            public int Quantity;
            public ProductVariant[] Variants
            {
                set
                {
                    stockOutModel = new List<StockModel>();
                    stockOutModelUpdated = new List<StockModelUpdated>();
                    //parentModel.stock. = new List<StockModel>();

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
                                StockModel stock = new StockModel() { labelOpt1 = "null", opt2 = new List<SecondOption>() }; ;
                                SecondOption second = new SecondOption() { labelOpt2 = "null", opt3 = new List<string>() };
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

                                stockOutModelUpdated.Add(stockUpdated);
                                // ---------------------------------------------

                                // Se busca si exisate uno ya
                                var aux = stockOutModel.Find(f => f.labelOpt1 == oneLabel);
                                if (aux != null && oneLabel != "null")
                                {
                                    aux.labelOpt1 = oneLabel;
                                    var aux2 = aux.opt2.Find(f => f.labelOpt2 == secondLabel);
                                    if (aux2 != null && secondLabel != "null")
                                    {                                        
                                        aux2.labelOpt2 = secondLabel;
                                        if (thirdLabel != "null")
                                        {
                                            aux2.opt3.Add(thirdLabel);
                                        }    
                                    }
                                    else
                                    {
                                        // Agregar a la lista 2 una nueva entrada
                                        second.labelOpt2 = secondLabel;
                                        if (thirdLabel != "null")
                                        {
                                            second.opt3.Add(thirdLabel);
                                        }
                                        stock.opt2.Add(second);
                                    }
                                    stock = aux;
                                    stockOutModel.Add(stock);
                                }
                                else
                                {
                                    if(oneLabel != "null")
                                    {                                   
                                        // Agregar a la lista 1 una nueva entrada
                                        stock.labelOpt1 = oneLabel;
                                        if (secondLabel != "null")
                                        {
                                            second.labelOpt2 = secondLabel;
                                            if (thirdLabel != "null")
                                            {
                                                second.opt3.Add(thirdLabel);
                                            }
                                            stock.opt2.Add(second);
                                        }
                                        stockOutModel.Add(stock);
                                    }
                                }

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

                        string json = JsonUtility.ToJson(stockOutModel);
                        Debug.Log(">>>> JSON STOCK: " + json);
                        /*Debug.Log("Stock opt1: " + stockOutModel.Count);
                        if(stockOutModel.Count > 0)
                        {
                            for (int i = 0; i < stockOutModel.Count; i++)
                            {
                                Debug.Log("Stock opt1 label: " + stockOutModel[i].labelOpt1);
                                Debug.Log("Stock opt2 Count: " + stockOutModel[i].opt2.Count);
                            }                           
                        }*/

                       


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


            public List<StockModelUpdated> GetInfoItemOutOfStock()
            {
                return stockOutModelUpdated;
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
            Color32 colorLight = new Color32(24, 229, 6, 255);
            Color32 colorOpaque = new Color32(229, 24, 176, 126);

            dropdown.Hide();
            dropdown.ClearOptions();            
            if (options == null || options.Length < 1 || options[0].Equals(Model.DEFAULT))
            {
                dropdown.gameObject.SetActive(false);
            }
            else
            {
                dropdown.gameObject.SetActive(true);
                List<ColorOptionDataTMP> optionsColor = new List<ColorOptionDataTMP>();
                for (int i = 0; i < options.Length; i++)
                {
                    optionsColor.Add(new ColorOptionDataTMP(options[i], colorLight, true));
                }

                dropdown.AddOptions(new List<TMP_Dropdown.OptionData>(optionsColor));
                //dropdown.AddOptions(options.ToList());
            }

            Data.GetInfoItemOutOfStock();

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
    }
}
