using Scuti.Net;
using UnityEngine;

namespace Scuti.UI {
    public class UITrigger : MonoBehaviour {
        public void Open(string args){
            var page = ScutiUtils.ParseScutiURL(args);
            if(page!=null)
            {
                Open(page.SetID, page.ViewID);
            }
        }

        public void OpenURL(string url)
        { 
            Application.OpenURL(url);
        }

        public void Open(string viewSetID, string viewID)
        {
            UIManager.Open(viewSetID, viewID);
        }

        public void Open(View view) {
            UIManager.Open(view);
        }
        
        public void Back() {
            UIManager.Back();
        }
    }
}
