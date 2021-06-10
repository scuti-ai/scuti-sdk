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
        private bool _orientation = true;

        private float AspectRatio
        {
            get
            {
	            {
		            _aspectRatio = Orientation
			            ? (float) ScreenX.Height / ScreenX.Width
			            : ScreenX.Width / (float) ScreenX.Height;
	            }
                return _aspectRatio;
            }
        }

        private bool Orientation
        {
            get
            {
                return true; // todo: replace this but since we only support landscape I'm hacking for now
	            //{
             //       return true;
		           // _orientation = Screen.orientation == ScreenOrientation.Landscape;
             //       #if UNITY_EDITOR
             //       _orientation = ScreenX.Width > ScreenX.Height;
             //       #endif
	            //}
             //   return _orientation;
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
	        canvas.referenceResolution = Orientation ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
	        var dynamicScale = Orientation ? 1 : scale;
            canvas.matchWidthOrHeight = AspectRatio < 1.7f ? dynamicScale : 1;
        }

        private void OnValidate()
        {
            canvas = GetComponent<CanvasScaler>();
        }
    }
}
