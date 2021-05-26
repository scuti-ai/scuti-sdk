using UnityEngine;

namespace Scuti.Examples.DataBinding {
    public class Modifier : MonoBehaviour {
        void Start() {
            Data.Profile.name.Value = System.Guid.NewGuid().ToString();
            Data.Profile.rating.Value = Random.Range(0f, 5f);
            Data.Profile.ordersCompleted.Value = 0;
        }

        [Button("Change")]
        void Change() {
            Data.Profile.name.Value = System.Guid.NewGuid().ToString();
            Data.Profile.rating.Value = Random.Range(0f, 5f);
            Data.Profile.ordersCompleted.Value = Data.Profile.ordersCompleted.Value + 1;
        }

        [Button]
        void AddNew(){
            Data.GUIDs.Add(System.Guid.NewGuid().ToString());
        }

        [Button]
        void RemoveRandom(){
            if(Data.GUIDs.Count > 0)
                Data.GUIDs.RemoveAt(Random.Range(0, Data.GUIDs.Count));
        }
    }
}
