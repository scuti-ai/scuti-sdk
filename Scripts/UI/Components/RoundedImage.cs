using UnityEngine;
using UnityEngine.UI;

// Credit to Nobinator : https://github.com/Nobinator/Unity-UI-Rounded-Corners/

namespace Scuti.UI {
    [RequireComponent(typeof(Image))]
    //[ExecuteInEditMode]
    public class RoundedImage : MonoBehaviour {
        const string k_ShaderName = "Scuti/RoundedImage";
        private static readonly int k_RoundnessProperty = Shader.PropertyToID("_WidthHeightRadius");

        public float radius;

        void Awake() {
            Refresh();
        }

        void OnRectTransformDimensionsChange() {
            Refresh();
        }

        void OnValidate() {
            Refresh();
        }

        Image Image {
            get { return GetComponent<Image>(); }
        }

        [ContextMenu ("Refresh") ]
        public void Refresh() {
            EnsureShader();
            UpdateRoundness();
        }

        void EnsureShader() {

            Shader shader = null;
            if (Image && Image.material)
            {
                shader = Image.material.shader;
                if (shader.name != k_ShaderName)
                {
                    Image.material = new Material(Shader.Find(k_ShaderName));
                }
            } else
            {
                ScutiLogger.Log("Unable to find image on roundedImage: " + this);
            }
        }

        void UpdateRoundness() {
            var rect = ((RectTransform)transform).rect;
            Image?.material.SetVector(k_RoundnessProperty, new Vector4(rect.width, rect.height, radius, 0));
        }
    }
}
