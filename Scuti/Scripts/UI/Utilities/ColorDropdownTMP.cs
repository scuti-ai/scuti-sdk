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
        Debug.Log(item.transform.childCount);
        var backgroundTemplante = item.transform.GetChild(0);
        var text = backgroundTemplante.GetComponent<TextMeshProUGUI>();

        var data = this.options[_dataIndex];
        if(data is ColorOptionDataTMP colorOptionData)
        {
            text.color = colorOptionData.Color;
        }
        else 
        {
            text.color = Color.green;
        }

        _dataIndex++;
        return item;
    }

}
