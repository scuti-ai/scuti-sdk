using System.Reflection;
using UnityEditor;

namespace Scuti.Editor
{
    [NativePropertyDrawer(typeof(ShowNativePropertyAttribute))]
    public class ShowNativePropertyNativePropertyDrawer : NativePropertyDrawer
    {
        public override void DrawNativeProperty(UnityEngine.Object target, PropertyInfo property)
        {
            object value = property.GetValue(target, null);

            if (value == null)
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNativePropertyNativePropertyDrawer).Name, "Reference");
                Scuti.Editor.EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, context: target);
            }
            else if (!Scuti.Editor.EditorDrawUtility.DrawLayoutField(value, property.Name))
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNativePropertyNativePropertyDrawer).Name, property.PropertyType.Name);
                Scuti.Editor.EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, context: target);
            }
        }
    }
}
