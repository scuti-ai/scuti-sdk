using System;
using System.Collections.Generic;

using UnityEngine;

namespace Scuti {
    [Serializable]
    public class ViewSetInstantiator : MonoBehaviour {
        public string id = "ID";
        [ReorderableList] public List<ViewInstantiator> creators;

        void OnValidate() {
            if(id.Contains("/"))
                id = id.Replace("/", "");
        }

        public ViewSet Instantiate(){
            var result = new ViewSet();
            foreach (var creator in creators) {
                var view = creator.Instantiate();
                view.Close();
                result.map.Add(creator.id, view);
            }
            return result;
        }
    }
}