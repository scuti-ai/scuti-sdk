using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


    public class ColorOptionDataTMP : TMP_Dropdown.OptionData
    {
        public ColorOptionDataTMP(string text, Color color): base (text)
        {
            Color = color;
        }

        public Color Color { get; set; }
    }




