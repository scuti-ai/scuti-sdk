using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformUtility : MonoBehaviour {


    public enum Action
    {
        None = 0,
        Delete = 1
    }

    public enum Equation
    {
        Equals = 0,
        NotEquals = 1
    }


    [Serializable]
    public struct PlatformData
    {
        public Action TargetAction;
        public Equation Validation;
        public List<RuntimePlatform> Platforms;
    }

    public List<PlatformData> Actions;

	void Awake () {
        var currentPlatform = Application.platform;
        foreach(var action in Actions)
        {
            bool valid = false; 
            switch(action.Validation)
            {
                case Equation.Equals:
                    foreach(var platform in action.Platforms)
                    {
                        if (platform == currentPlatform) valid = true;
                    }
                    break;
                case Equation.NotEquals:
                    valid = true;
                    foreach (var platform in action.Platforms)
                    {
                        if (platform == currentPlatform) valid = false;
                    }
                    break;
            }

            if(valid)
            {
                switch(action.TargetAction)
                {
                    case Action.Delete:
                        Destroy(gameObject);
                        break;
                }
            }
        }
	}
	
}
