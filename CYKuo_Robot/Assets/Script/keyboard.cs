using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keyboard : MonoBehaviour {
    static public float px = 200f, py = 350f, pz = 200f;
    static public float rx = 0f, ry = 0f, rz = 0f;
    static public float b = 0f, g = 0f;
    // Use this for initialization
    void Start () {
        px = 200f; py = 0; pz = 200f;
        rx = 0f; ry = 0f; rz = 0f;
    }
	
	// Update is called once per frame
	void Update () {
		
        if(Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.Space))
        {
            px += 1;
        }
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.Space))
        {
            px -= 1;
        }
        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.Space))
        {
            py += 1;
        }
        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.Space))
        {
            py -= 1;
        }
        if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Space))
        {
            pz += 1;
        }
        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.Space))
        {
            pz -= 1;
        }
        if (Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.Space))
        {
            b += 1;
        }
        if (Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Space))
        {
            b -= 1;
        }


        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.Space))
        {
            rx += 1;
        }
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.Space))
        {
            rx -= 1;
        }
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.Space))
        {
            ry += 1;
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.Space))
        {
            ry -= 1;
        }
        if (Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.Space))
        {
            rz += 1;
        }
        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.Space))
        {
            rz -= 1;
        }

        if (Input.GetKey(KeyCode.Z) && Input.GetKey(KeyCode.Space))
        {
            g = 0;
        }
        if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.Space))
        {
            g = 70;
        }
       
    }
}
