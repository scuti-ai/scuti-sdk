using System.Linq;
using System;
using System.Collections.Generic;

using UnityEngine;

namespace Scuti {
    public class UIGenerator : MonoBehaviour {
        [ReorderableList] public List<ViewSetInstantiator> instantiators;
        public Dictionary<string, ViewSet> instances = new Dictionary<string, ViewSet>();

        public void Load(string id) {
            var match = instantiators.First(x => x.id == id);
            if (match == null)
                throw new Exception("No ViewGroupConfig found for ID " + id);

            var instance = match.Instantiate();
            instances.Add(id, instance);
        }

        public void Unload(string id){
            if (!instances.ContainsKey(id))
                throw new Exception("No ViewGroup instance foudn for ID " + id);

            var match = instances[id];
            match.Unload();
        }

        public ViewSet this[string id, bool ensure = true] {
            get { return GetViewSet(id, ensure); }
        }

        public ViewSet GetViewSet(string setID, bool ensure = true) {
            if (instances.ContainsKey(setID))
                return instances[setID];
            else if (ensure){
                Load(setID);
                return GetViewSet(setID, ensure);
            }

            throw new Exception("No View Group instance found by ID " + setID);
        }
    }
}