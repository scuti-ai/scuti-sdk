using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

// TODO: Split this into more files?
// ================================================
#region IMPLEMENTATIONS
// ================================================
[Serializable] public class ColorArrayBinding : ListBinding<Color> { }

[Serializable] public class FloatBinding : GenericBinding<float> { }
[Serializable] public class StringBinding : GenericBinding<string> { }
[Serializable] public class IntBinding : GenericBinding<int> { }
[Serializable] public class BoolBinding : GenericBinding<bool>{ }

[Serializable] public class ColorBinding : GenericBinding<Color> { }
[Serializable] public class ObjectBinding : GenericBinding<Object> { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatBinding))]
public class FloatBindingDrawer : SingleValuePropertyDrawer {
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").floatValue = 
        EditorGUI.FloatField(position, property.FindPropertyRelative("_value").floatValue);
    }
}

[CustomPropertyDrawer(typeof(StringBinding))]
public class StringBindingDrawer : SingleValuePropertyDrawer {
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").stringValue = 
        EditorGUI.TextField(position, property.FindPropertyRelative("_value").stringValue);
    }
}

[CustomPropertyDrawer(typeof(IntBinding))]
public class IntBindingDrawer : SingleValuePropertyDrawer {
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").intValue =
        EditorGUI.IntField(position, property.FindPropertyRelative("_value").intValue);
    }
}

[CustomPropertyDrawer(typeof(BoolBinding))]
public class BoolBindingDrawer : SingleValuePropertyDrawer {
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").boolValue =
        EditorGUI.Toggle(position, property.FindPropertyRelative("_value").boolValue);
    }
}

[CustomPropertyDrawer(typeof(ColorBinding))]
public class ColorBindingDrawer : SingleValuePropertyDrawer {
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").colorValue =
        EditorGUI.ColorField(position, property.FindPropertyRelative("_value").colorValue);
    }
}

[CustomPropertyDrawer(typeof(ObjectBinding))]
public class ObjectBindingDrawer : SingleValuePropertyDrawer{
    public override void DrawValue(SerializedProperty property, Rect position) {
        property.FindPropertyRelative("_value").objectReferenceValue =
        EditorGUI.ObjectField(position, property.FindPropertyRelative("_value").objectReferenceValue, typeof(Sprite), false);
    }
}

#endif

#endregion

// ================================================
#region BASE DRAWERS
// ================================================
#if UNITY_EDITOR
public abstract class SingleValuePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.LabelField(position, label);

        var space = position.width * .4f;
        var valuePosition = new Rect(position.x + space, position.y, position.width - space, position.height);

        DrawValue(property, valuePosition);

        EditorGUI.EndProperty();
    }

    public abstract void DrawValue(SerializedProperty property, Rect position);
}
#endif
#endregion