using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
public class car2 : MonoBehaviour
{

    float x, y;
    public float Vs;
    float a;
    int sign = 1;
    // Use this for initialization
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        a = transform.eulerAngles.y;
        if (a > 180)
            a -= 360;
        x = CrossPlatformInputManager.GetAxis("Horizontal");
        y = CrossPlatformInputManager.GetAxis("Vertical");
        Vector3 movement = new Vector3(x, 0, y);
        if (x != 0 && y != 0)
        {
            if (Mathf.Abs(a - Mathf.Atan2(x, y) * Mathf.Rad2Deg) <= 2)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(x, y) * Mathf.Rad2Deg, transform.eulerAngles.z);
                gameObject.GetComponent<Rigidbody>().velocity = movement * Vs;
            }
            else
            {
                Debug.Log(transform.eulerAngles.y);
                if (transform.eulerAngles.y > 180)
                {
                    sign = (transform.eulerAngles.y - 360 > Mathf.Atan2(x, y) * Mathf.Rad2Deg ? -1 : 1);
                    if (Mathf.Abs(transform.eulerAngles.y - 360 - Mathf.Atan2(x, y) * Mathf.Rad2Deg) >= 180)
                        sign = -sign;
                }
                else
                {
                    sign = (transform.eulerAngles.y > Mathf.Atan2(x, y) * Mathf.Rad2Deg ? -1 : 1);
                    if (Mathf.Abs(transform.eulerAngles.y - Mathf.Atan2(x, y) * Mathf.Rad2Deg) >= 180)
                        sign = -sign;
                }
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + (float)0.5 * sign, transform.eulerAngles.z);
                gameObject.GetComponent<Rigidbody>().velocity = movement*0;
            }
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().velocity = movement * 0;
        }
    }
}
