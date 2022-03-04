using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scuti.UI;

public class TestOpenCart : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenCart()
    {
        UIManager.Open(UIManager.Cart);
    }

}
