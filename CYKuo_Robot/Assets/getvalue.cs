using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class getvalue : MonoBehaviour
{
    public GameObject RGB;
    public GameObject Depth;
    Texture2D edit1;
    Texture2D edit2;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        edit1 = (Texture2D)RGB.GetComponent<RawImage>().mainTexture;
        edit2 = (Texture2D)Depth.GetComponent<RawImage>().mainTexture;

        Debug.Log("R2:" + edit2.GetPixel(0, 0).r);
        Debug.Log("G2:" + edit2.GetPixel(0, 0).g);
        Debug.Log("B2:" + edit2.GetPixel(0, 0).b);

        Debug.Log("R1:" + edit1.GetPixel(0, 0).r);
        Debug.Log("G1:" + edit1.GetPixel(0, 0).g);
        Debug.Log("B1:" + edit1.GetPixel(0, 0).b);
    }
}
