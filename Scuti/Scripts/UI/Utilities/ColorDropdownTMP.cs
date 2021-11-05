using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorDropdownTMP : TMP_Dropdown
{

    private int _dataIndex = 0;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        _dataIndex = 0;
        return base.CreateDropdownList(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        var item = base.CreateItem(itemTemplate);
        var toggle = item.GetComponent<Toggle>();
        var text = item.GetComponent<TextMeshProUGUI>();

        var data = this.options[_dataIndex];
        if(data is ColorOptionDataTMP colorOptionData)
        {
            Debug.Log("Color: " + colorOptionData.Color);
            text.color = colorOptionData.Color;
            toggle.interactable = colorOptionData.Interactable;
        }
        else 
        {
            text.color = Color.green;
            toggle.interactable = true;
        }

        _dataIndex++;
        return item;
    }

}
