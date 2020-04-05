using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IK1 : MonoBehaviour
{
    public Transform[] Joints;
    public Transform[] Gripper;
    public Transform[] Catcher;
    public Transform target;
    public Transform target_;
    public Transform target_R;
    public Transform base_;
    public Transform now;
    public Slider S_Slider;    //PositionX min=4 max=12
    public Slider L_Slider;    //PositionY min=-4 max=4
    public Slider U_Slider;    //PositionZ min=4 max=12
    public Slider R_Slider;    //RotationX min=-90 max=90
    public Slider B_Slider;    //RotationY min=-90 max=90
    public Slider T_Slider;    //RotationZ min=-90 max=90
    public Slider BaseSlider;    //RotationZ min=0 max=360
    public Slider CatchSlider;    //RotationZ min=0 max=90
    public Text[] txtArr;
    public static float[] theta = new float[6];    //angle of the joints

    public static float[] angle = new float[6];    //angle of the joints

    public int b;
    public static int g;
    private float L1, L2, L3, L4, L5, L6;    //arm length in order from base
    private float C3;
    private float x,y;
    private Vector3 initial_pos;
    // Use this for initialization
    void Start()
    {
        target_.position = target_R.position;
        initial_pos = new Vector3(target_.localPosition.x, target_.localPosition.y, target_.localPosition.z);

        L1 = 102.3f;
        L2 = 108.5f;
        L3 = 103.23f;
        L4 = 28.0f;
        L5 = 26.0f;
        L6 = 50.0f;
        C3 = 0.0f;
        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0f;
    }
    // Update is called once per frame
    void Update()
    {
        b = (int)BaseSlider.value;
        g = (int)CatchSlider.value;
        target_.position = target_R.position;
        //target.localEulerAngles = new Vector3(0, 180+ BaseSlider.value, 0);
        x = -(target_.localPosition.z - base_.localPosition.z);
        y = (target_.localPosition.y - base_.localPosition.y);
        //print(Mathf.Acos(y/x));
        Debug.Log("X:" + x);
        Debug.Log("Y:" + y);
        theta[1] = Mathf.PI-Mathf.Acos((x * x + y * y - 108.34f * 108.34f - 235.6f * 235.6f) / 2 / 108.34f / 235.6f);
        //print("(x * x + y * y - 108.34f * 108.34f - 235.6f * 235.6f) / 2 / 108.34f / 235.6f" + (x * x + y * y - 108.34f * 108.34f - 235.6f * 235.6f) / 2 / 108.34f / 235.6f);
        Debug.Log("theta[1]:" + theta[1]);
        theta[2] = Mathf.PI/2- Mathf.Atan(y/ x) - Mathf.Atan(235.6f*Mathf.Sin(theta[1])/(108.34f+235.6f*Mathf.Cos(theta[1])));
        //Debug.Log("theta[2]=:" + theta[2]);
        print("Mathf.Atan(y/ x)" + Mathf.Atan(y / x));
        print(" Mathf.Atan(235.6f*Mathf.Sin(theta[1])/(108.34f+235.6f*Mathf.Cos(theta[1]))" + Mathf.Atan(235.6f * Mathf.Sin(theta[1]) / (108.34f + 235.6f * Mathf.Cos(theta[1]))));
        //angle[0] = b;
        angle[0] = 0;
        angle[1] = Mathf.Round(theta[2] * Mathf.Rad2Deg);
        angle[2] = Mathf.Round(theta[1] * Mathf.Rad2Deg);


        //-----------------
        //float gradient = 0;
        //float f_x = Vector3.Distance(target_R.position, now.position);
        //if (f_x > 10)
        //{
        //    for (int i = 1; i <= 2; i++)
        //    {
        //        float new_angle = angle[i];
        //        angle[i] += 1f;
        //        switch (i)
        //        {
        //            case 1:
        //                Joints[1].transform.localEulerAngles = new Vector3(angle[1], 90, 0);
        //                break;
        //            case 2:
        //                Joints[2].transform.localEulerAngles = new Vector3(angle[2], -90, 0);
        //                break;
        //        }
        //        float f_x_plus_d = Vector3.Distance(target_R.position, now.position);
        //        gradient = (f_x_plus_d - f_x) / 1f;

        //        angle[i] = new_angle;
        //        if (Mathf.Abs(gradient) < 0.01)
        //            angle[i] -= gradient > 0 ? 0.1f : -0.1f;
        //        else
        //            angle[i] -= 0.1f * gradient;

        //        if (i == 1)
        //            angle[i] = Mathf.Clamp(angle[i], -90, 90);
        //        else if (i == 2)
        //            angle[i] = Mathf.Clamp(angle[i], 0, 135);
        //        else
        //            angle[i] = Mathf.Clamp(angle[i], -90, 90);
        //        // Restores
        //        if (f_x_plus_d < 2)
        //            break;
        //    }
        //}
        //angle[1] = Mathf.Clamp(angle[1], -90, 90);
        //angle[2] = Mathf.Clamp(angle[2], 0, 135);
        angle[3] = 90;
        angle[4] = 0;
        angle[5] = 0;
        //------------------
        if (!double.IsNaN(angle[0]))
            Joints[0].transform.localEulerAngles = new Vector3(b+angle[0], -90, -90);
        if (!double.IsNaN(angle[1]))
        {
            Joints[1].transform.localEulerAngles = new Vector3(angle[1], 90, 0);
        }

        if (!double.IsNaN(angle[2]))
        {
            Joints[2].transform.localEulerAngles = new Vector3(angle[2], -90, 0);
        }

        if (!double.IsNaN(angle[3]))
        {
            Joints[3].transform.localEulerAngles = new Vector3(0, -angle[3] - 90, 180);
        }
        if (!double.IsNaN(angle[4]))
        {
            Joints[4].transform.localEulerAngles = new Vector3(angle[4], 0, 0);
        }

        if (!double.IsNaN(angle[5]))
        {
            //if (angle[5] < 0) angle[5] = 0;
            //if (angle[5] > 180) angle[5] = 180;
            Joints[5].transform.localEulerAngles = new Vector3(-180, -angle[5], 0);/**/
        }

        Gripper[0].transform.localEulerAngles = new Vector3(0, 0, -g);
        Gripper[1].transform.localEulerAngles = new Vector3(0, 0, g);
        Gripper[2].transform.localEulerAngles = new Vector3(0, 0, -g);
        Gripper[3].transform.localEulerAngles = new Vector3(0, 0, g);

        Catcher[0].transform.localEulerAngles = new Vector3(0, 0, -g);
        Catcher[1].transform.localEulerAngles = new Vector3(0, 0, g);
        txtArr[0].text = (angle[0]).ToString();
        txtArr[1].text = (angle[1]).ToString();
        txtArr[2].text = (angle[2]).ToString();
        txtArr[3].text = (angle[3]).ToString();
        txtArr[4].text = (angle[4]).ToString();
        txtArr[5].text = (angle[5]).ToString();
        txtArr[6].text = g.ToString();




    }
}
