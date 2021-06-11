
using Scuti;
using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
    [ExecuteInEditMode]
    public class CustomScaling : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvas;
        [SerializeField] private float scale = 0.56f;
        private float _aspectRatio = -1;

        int _count = 0;

        public float AspectRatio
        {
            get
            {
                {
                    _aspectRatio = ScreenX.Width / (float)ScreenX.Height;
                }
                return _aspectRatio;
            }
        }


        private void Start()
        {
            RefreshScale();
        }
        private void Update()
        {
            if (_count % 60 == 0)
                RefreshScale();
            _count++;
        }

        private void RefreshScale()
        {
            canvas.matchWidthOrHeight = AspectRatio < 1.7f ? scale : 1;
        }

        private void OnValidate()
        {
            canvas = GetComponent<CanvasScaler>();
        }
    }
}
