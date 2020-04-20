using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceItem : MonoBehaviour
{
    [Range(0, 100f)]
    public float force = 10;

    void Update()
    {
    	if(Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0.03f, 0);
        }
        else if(Input.GetKey(KeyCode.S))
        {
            transform.position -= new Vector3(0, 0.03f, 0);
        }

        if(Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(0, 0, 0.03f);
        }
        else if(Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(0, 0, 0.03f);
        }
    }
}
