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
        private bool _landscape = true;

        private float AspectRatio
        {
            get
            {
	            {
		            _aspectRatio = Landscape
			            ? (float) Camera.main.pixelWidth / Camera.main.pixelHeight
			            : Camera.main.pixelHeight / (float) Camera.main.pixelWidth;
	            }
                return _aspectRatio;
            }
        }

        private bool Landscape
        {
            get
            {
	            {
		            _landscape = Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight;
                    #if UNITY_EDITOR
		            _landscape = Camera.main.pixelWidth > Camera.main.pixelHeight;
                    #endif
	            }
                return _landscape;
            }
        }

        private void Update()
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
	        canvas.referenceResolution = Landscape ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
	        var dynamicScale = scale;
            canvas.matchWidthOrHeight = AspectRatio < 1.7f ? dynamicScale : 1;
        }

        private void OnValidate()
        {
            canvas = GetComponent<CanvasScaler>();
        }
    }
}
