using UnityEngine;
using UnityEngine.UI;

namespace Scuti.UI
{
	[ExecuteInEditMode]
    public class CustomScaling : MonoBehaviour
    {
        [HideInInspector][SerializeField] private CanvasScaler canvas;
        [SerializeField] private float scale = 0.56f;
        private float _aspectRatio = -1;

        private float AspectRatio
        {
            get
            {
	            {
		            _aspectRatio = ScreenX.Width < ScreenX.Height
			            ? (float) ScreenX.Height / ScreenX.Width
			            : ScreenX.Width / (float) ScreenX.Height;
	            }
                return _aspectRatio;
            }
        }

        private void Start()
        {
            RefreshScale();
        }

#if UNITY_EDITOR
	    private void OnGUI()
        {
	        RefreshScale();
        }
#endif
	    private void RefreshScale()
        {
	        canvas.referenceResolution = ScreenX.Width < ScreenX.Height ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
	        var dynamicScale = ScreenX.Width < ScreenX.Height ? 1 : scale;
            canvas.matchWidthOrHeight = AspectRatio < 1.7f ? dynamicScale : 1;
        }

        private void OnValidate()
        {
            canvas = GetComponent<CanvasScaler>();
        }
    }
}
