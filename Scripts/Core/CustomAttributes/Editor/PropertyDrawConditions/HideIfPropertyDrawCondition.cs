using UnityEditor;

namespace Scuti.Editor
{
    [PropertyDrawCondition(typeof(HideIfAttribute))]
    public class HideIfPropertyDrawCondition : ShowIfPropertyDrawCondition
    {
    }
}
