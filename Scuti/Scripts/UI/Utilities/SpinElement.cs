using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinElement : MonoBehaviour
{
    [SerializeField]
    private int speed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * Time.deltaTime * 150);
    }
}
