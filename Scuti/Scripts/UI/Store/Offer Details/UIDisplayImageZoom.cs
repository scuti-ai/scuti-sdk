using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{   
    public class UIDisplayImageZoom : MonoBehaviour
    {
        [SerializeField] private RectTransform rectParentImage;
        [SerializeField] private Image imageLarge;

        [SerializeField] private GameObject firstSelection;

        public void Init()
        {
            Hide();
        }

        // Start is called before the first frame update
        public void Show(Sprite sprite)
        {
            imageLarge.sprite = sprite;
            rectParentImage.gameObject.SetActive(true);

            UIManager.SetFirstSelected(firstSelection);

        }

        // Update is called once per frame
        public void Hide()
        {
            rectParentImage.gameObject.SetActive(false);
        }


        public void OnBtnCloseLargeImage()
        {
            Hide();
        }

    }


}


