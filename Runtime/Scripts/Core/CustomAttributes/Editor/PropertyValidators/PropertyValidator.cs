using UnityEditor;

namespace Scuti.Editor
{
    public abstract class PropertyValidator
    {
        public abstract void ValidateProperty(SerializedProperty property);
    }
}
