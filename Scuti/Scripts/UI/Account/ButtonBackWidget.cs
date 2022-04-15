using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Scuti.UI
{
    public class ButtonBackWidget : MonoBehaviour
    {
        public RectTransform background;
        public TextMeshProUGUI backButtonLabel;
        public Image imageIcon;
        public Sprite iconClose;
        public Sprite iconBack;

        private Vector2 startSize;

        private void Start()
        {
            startSize = background.sizeDelta;
            UIManager.onBackButton += UpdateBackButtonState;

        }


        public void UpdateBackButtonState(bool isMainPanel)
        {
            if(isMainPanel)
            {
                backButtonLabel.gameObject.SetActive(true);
                background.sizeDelta = startSize;
                imageIcon.sprite = iconClose;
            }
            else 
            {
                Debug.Log("--------------Hiden text back");
                backButtonLabel.gameObject.SetActive(false);
                background.sizeDelta = new Vector2(startSize.x-196, startSize.y);
                imageIcon.sprite = iconBack;
            }

        }

    }

}
