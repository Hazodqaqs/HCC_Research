using UnityEngine;
using System.Collections;

public class EdgeDetection : MonoBehaviour {

    public Texture2D t;

    [Range(0, 0.5f)]
    public float intensity = 0.15f;

    public Color drawColor = new Color32(124, 89, 54, 255);
    public Color defaultColor = new Color32(0, 0, 0, 0);


    Texture2D tt;

    void Start()
    {
        transform.localScale = new Vector3((float)t.width / (float)t.height, 1);
        InvokeRepeating("MyUpdate", 0, 1);
    }

    void MyUpdate()
    {
        //if (t != null)
        //    t = GetComponent<Renderer>().material.mainTexture as Texture2D;

        if (tt != null)
        {
            Destroy(tt);
        }
        tt = new Texture2D(t.width, t.height);
        for (int y = 1; y < t.height - 1; y++)
        {
            for (int x = 1; x < t.width - 1; x++)
            {
                float g = t.GetPixel(x, y).grayscale;
                float gL = t.GetPixel(x - 1, y).grayscale;
                float gR = t.GetPixel(x + 1, y).grayscale;
                float gT = t.GetPixel(x, y - 1).grayscale;
                float gB = t.GetPixel(x, y + 1).grayscale;
                if (Mathf.Abs(g - gL) > intensity)
                {
                    tt.SetPixel(x, y, drawColor);
                }
                else if (Mathf.Abs(g - gR) > intensity)
                {
                    tt.SetPixel(x, y, drawColor);
                }
                else if (Mathf.Abs(g - gT) > intensity)
                {
                    tt.SetPixel(x, y, drawColor);
                }
                else if (Mathf.Abs(g - gB) > intensity)
                {
                    tt.SetPixel(x, y, drawColor);
                }
                else
                {
                    tt.SetPixel(x, y, defaultColor);
                }
            }
        }
        tt.Apply();
        GetComponent<Renderer>().material.mainTexture = tt;
    }
}
