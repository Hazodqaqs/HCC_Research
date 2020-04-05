using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class carPosition : MonoBehaviour
{
    public float mSpeed = 1;
    public float rSpeed = 1;
    public Slider C_slider;
    bool car_M = false;
    // Use this for initialization
    void Start()
    {
        //C_slider.value = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Space))
            car_M = !car_M;
        if (car_M)
        //transform.position = new Vector3(transform.position.x, transform.position.y, C_slider.value);
        {
            float h = Input.GetAxis("Horizontal");//獲取水平軸向按鍵
            float v = Input.GetAxis("Vertical");//獲取垂直軸向按鍵
            transform.Translate(0, 0, -mSpeed * v);//根據水平軸向按鍵來前進或後退
            Debug.Log("v: " + v + "   h:  " + h);
            transform.Rotate(0, rSpeed * h, 0);//根據垂直軸向按鍵來旋轉
        }
    }
}
