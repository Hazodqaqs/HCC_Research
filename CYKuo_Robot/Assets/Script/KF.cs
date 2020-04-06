using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class KF : MonoBehaviour
{
    public Texture2D rgb;
    public Texture2D depth;
    public int ID;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    GameObject pc;
    // Update is called once per frame
    int count=0;
    void Update()
    {
        if (count > 300)
        {
            int num = CallDLL.check_kf_ID(ID);
            if (num == -1)
            {
                Debug.Log("Delete");
                Destroy(gameObject);
            }
            count = 0;
        }
        else
        {
            count++;
        }

    }
}
