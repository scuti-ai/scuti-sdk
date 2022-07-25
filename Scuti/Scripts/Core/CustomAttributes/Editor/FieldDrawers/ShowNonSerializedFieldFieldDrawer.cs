using System.Reflection;
using UnityEditor;

namespace Scuti.Editor
{
    [FieldDrawer(typeof(ShowNonSerializedFieldAttribute))]
    public class ShowNonSerializedFieldFieldDrawer : FieldDrawer
    {
        public override void DrawField(UnityEngine.Object target, FieldInfo field)
        {
            object value = field.GetValue(target);

            if (value == null)
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNonSerializedFieldFieldDrawer).Name, "Reference");
                Scuti.Editor.EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, context: target);
            }
            else if (!Scuti.Editor.EditorDrawUtility.DrawLayoutField(value, field.Name))
            {
                string warning = string.Format("{0} doesn't support {1} types", typeof(ShowNonSerializedFieldFieldDrawer).Name, field.FieldType.Name);
                Scuti.Editor.EditorDrawUtility.DrawHelpBox(warning, MessageType.Warning, context: target);
            }
        }
    }
}
