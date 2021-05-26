using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Touchable : Graphic
{
    public override bool Raycast(UnityEngine.Vector2 sp, Camera eventCamera)
    {
        //return base.Raycast(sp, eventCamera);
        return true;
    }


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
