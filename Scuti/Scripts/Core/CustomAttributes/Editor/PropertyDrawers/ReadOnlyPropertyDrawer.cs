using UnityEditor;
using UnityEngine;

namespace Scuti.Editor
{
    [PropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void DrawProperty(SerializedProperty property)
        {
            GUI.enabled = false;
            Scuti.Editor.EditorDrawUtility.DrawPropertyField(property);
            GUI.enabled = true;
        }
    }
}
