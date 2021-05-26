using UnityEngine;

namespace Scuti {
    public class ScreenX {
        public static int Height {
            get { return (int)GetWindowSize().y; }
        }

        public static int Width {
            get { return (int)GetWindowSize().x; }
        }

        /// <summary>
        /// Gets the current window size.
        /// In the build, this returns Screen.width, Screen.height
        /// In the editor, this returns the size of the Game Window using reflection
        /// </summary>
        public static Vector2 GetWindowSize() {
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

        /// <summary>
        /// Converts a Vector from screen space A to B. Where:
        /// A is the size of the window the game is running in. This can be in windowed or editor mode
        /// B is the resolution that the application is set to
        /// </summary>
        public static Vector2 VectorToResolution(Vector2 res) {
            var windowSize = GetWindowSize();
            var scaleFactor = new Vector2(
                (Screen.currentResolution.width / windowSize.x),
                (Screen.currentResolution.height / windowSize.y)
            );

            return new Vector2(
                res.x * scaleFactor.x,
                res.y * scaleFactor.y
            );
        }

        /// <summary>
        /// Converts a Vector from screen space A to B. Where:
        /// A is the resolution that the application is set to
        /// B is the size of the window the game is running in. This can be in windowed or editor mode
        /// </summary>
        public static Vector2 VectorToWindow(Vector2 res) {
            var windowSize = GetWindowSize();
            var scaleFactor = new Vector2(
                (Screen.currentResolution.width / windowSize.x),
                (Screen.currentResolution.height / windowSize.y)
            );

            return new Vector2(
                res.x / scaleFactor.x,
                res.y / scaleFactor.y
            );
        }
    }
}
