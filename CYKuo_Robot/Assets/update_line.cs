using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class update_line : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public Text text;
    public GameObject img;
    // Update is called once per frame
    void Update()
    {
        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        
        if(img!=null)
        {
            if (GetComponent<LineRenderer>().positionCount == 2)
            {
                img.SetActive(true);
                double d = Vector3.Distance(GetComponent<LineRenderer>().GetPosition(0), GetComponent<LineRenderer>().GetPosition(1));
                Debug.Log("Distance:" + d);
                if (text != null)
                    text.text = d.ToString("#0.000") +" m";
            }
            else
            {
                img.SetActive(false);
            }
        }
    }
}
