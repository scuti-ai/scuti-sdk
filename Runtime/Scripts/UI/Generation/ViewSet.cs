using System.Collections.Generic;

using UnityEngine;

namespace Scuti {
    [System.Serializable]
    public class ViewSet {
        [SerializeField] public Dictionary<string, View> map = new Dictionary<string, View>();
        public Dictionary<string, View> Map { get { return map; } }

        public View this[string id] {
            get { return GetView(id); }
        }

        public View GetView(string id) {
            if (map.ContainsKey(id))
                return map[id];
            throw new System.Exception($"No View with ID {id} found!");
        }

        public void Unload() {
            foreach (var pair in map)
                if (pair.Value != null)
                    Object.Destroy(pair.Value.gameObject);
            map.Clear();
        }
    }
}
