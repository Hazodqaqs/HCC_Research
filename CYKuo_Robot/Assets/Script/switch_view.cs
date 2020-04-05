using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class switch_view : MonoBehaviour
{
    public Button switchViewer;
    public Transform a;
    public Transform b;
    // Use this for initialization
    void Start()
    {
        switchViewer.onClick.AddListener(Switcher);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Switcher()
    {
        Debug.Log(a.transform.localPosition.z);
        Debug.Log(b.transform.localPosition.z);
        if (a.transform.localPosition.z == 9)
        {
            a.transform.localPosition = new Vector3(b.transform.localPosition.x, b.transform.localPosition.y, b.transform.localPosition.z );
            b.transform.localPosition = new Vector3(b.transform.localPosition.x, b.transform.localPosition.y, b.transform.localPosition.z - 1);
        }
        else if (b.transform.localPosition.z == 9)
        {
            b.transform.localPosition = new Vector3(a.transform.localPosition.x, a.transform.localPosition.y, a.transform.localPosition.z);
            a.transform.localPosition = new Vector3(a.transform.localPosition.x, a.transform.localPosition.y, a.transform.localPosition.z - 1);
        }
        Debug.Log(a.transform.localPosition.z);
        Debug.Log(b.transform.localPosition.z);
    }
}
