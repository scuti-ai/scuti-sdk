using UnityEngine;
using UnityEditor;

namespace Scuti.Editor
{
    [PropertyDrawer(typeof(ResizableTextAreaAttribute))]
    public class ResizableTextAreaPropertyDrawer : PropertyDrawer
    {
        public override void DrawProperty(SerializedProperty property)
        {
            Scuti.Editor.EditorDrawUtility.DrawHeader(property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUILayout.LabelField(property.displayName);

                EditorGUI.BeginChangeCheck();

                string textAreaValue = EditorGUILayout.TextArea(property.stringValue, GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 3f));

                if (EditorGUI.EndChangeCheck())
                {
                    property.stringValue = textAreaValue;
                }
            }
            else
            {
                string warning = PropertyUtility.GetAttribute<ResizableTextAreaAttribute>(property).GetType().Name + " can only be used on string fields";
                Scuti.Editor.EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, context: PropertyUtility.GetTargetObject(property));

                Scuti.Editor.EditorDrawUtility.DrawPropertyField(property);
            }
        }
    }
}
