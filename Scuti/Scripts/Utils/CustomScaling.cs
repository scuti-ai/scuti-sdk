
using Scuti;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scuti.UI
{
	[ExecuteAlways]
    public class CustomScaling : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvas;
        [SerializeField] private float _referenceScale = 0.0f;
        [SerializeField] private float _upperScale = 0.23f;
        [SerializeField] private float _aspectRatio = -1;

        int _count = 0;

        public float AspectRatio
        {
            get
            {
                {
                    var screenSize = GetWindowSize();
                    // hack for now -mg
                    if(screenSize.x < screenSize.y)
                    {
                        var tmp = screenSize.x;
                        screenSize.x = screenSize.y;
                        screenSize.y = tmp;
                    }
                    _aspectRatio = screenSize.x / screenSize.y;
                }
                return _aspectRatio;
            }
        }

   /// <summary>
    /// Gets the current window size.
    /// In the build, this returns Screen.width, Screen.height
    /// In the editor, this returns the size of the Game Window using reflection
    /// </summary>
    private Vector2 GetWindowSize()
    {
        // Screen.width and Screen.height sometimes return the dimensions of the inspector
        // window when Screen.width originates from a ContextMenu or Button attribute
        // We use reflection to get the actual dimensions. During runtime we simply use Screen again
#if UNITY_EDITOR
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        var result = (Vector2)Res;
        return result;
#else
            return new Vector2(
                Screen.width,
                Screen.height
            );
#endif
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



#if UNITY_EDITOR
        private void OnGUI()
        {
	        RefreshScale();
        }
		#endif

        private void RefreshScale()
        {
            canvas.matchWidthOrHeight = AspectRatio < 1.7f ? _referenceScale : _upperScale;
        }

        private void OnValidate()
        {
            canvas = GetComponent<CanvasScaler>();
        }
    }
}
