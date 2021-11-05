using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


    public class ColorOptionDataTMP : TMP_Dropdown.OptionData
    {
        public ColorOptionDataTMP(string text, Color color, bool interactable): base (text)
        {
            Color = color;
            Interactable = interactable;
        }

        public Color Color { get; set; }
        public bool Interactable { get; set; }


}




