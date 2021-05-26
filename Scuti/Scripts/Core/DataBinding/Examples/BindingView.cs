using UnityEngine;
using UnityEngine.UI;
 
namespace Scuti.Examples.DataBinding {
    public class BindingView : MonoBehaviour {
        [SerializeField] new Text name;
        [SerializeField] Text rating;
        [SerializeField] Text ordersCompleted;

        void Awake() {
            Data.Profile.name.OnValueChanged += value => name.text = value;
            Data.Profile.rating.OnValueChanged += value => rating.text = value.ToString();
            Data.Profile.ordersCompleted.OnValueChanged += value => ordersCompleted.text = value.ToString();

            Data.Profile.OnMemberChanged += memberName => Debug.Log(memberName + " changed");

            Data.GUIDs.OnElementAdded += element => {
                string all = string.Empty;
                foreach (var guid in Data.GUIDs)
                    all += guid + "\n";
                
                Debug.Log("Added : " + element + " \nTotal list: \n" + all);
            };
            Data.GUIDs.OnElementRemoved += element => {
                string all = string.Empty;
                foreach (var guid in Data.GUIDs)
                    all += guid + "\n";

                Debug.Log("Removed : " + element + " \nTotal list: \n" + all);
            };
        } 
    }
}