using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TEST_IK : MonoBehaviour
{

    public Transform[] Joints;
    public Transform[] Gripper;
    public Transform[] Catcher;
    public Transform target;
    public Transform car;
    public Transform target_R;
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
    public static double[] theta = new double[6];    //angle of the joints

    public static float[] angle = new float[6];    //angle of the joints

    private float L1, L2, L3, L4, L5, L6;    //arm length in order from base
    private float C3;

    public float px = 230f, py = 350f, pz = 200f;
    public float rx = 0f, ry = 0f, rz = 0f;
    public int b;
    public static int g;
    private Vector3 initial_pos;
    // Use this for initialization
    void Start()
    {
        initial_pos = new Vector3(target_R.localPosition.x , target_R.localPosition.y, target_R.localPosition.z );
        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0;
        L1 = 102.3f;
        L2 = 108.5f;
        L3 = 103.23f;
        L4 = 28.0f;
        L5 = 26.0f;
        L6 = 50.0f;
        C3 = 0.0f; 
        px = 230f; py = 0; pz = 200f;
        rx = 0f; ry = 0f; rz = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        //target.eulerAngles=new Vector3(car.eulerAngles.x, car.eulerAngles.y, car.eulerAngles.z -R_Slider.value);
        //target_R.position = new Vector3(car.position.x + difference.x - L_Slider.value, car.position.y + difference.y + U_Slider.value - 200, car.position.z + difference.z - S_Slider.value + 230);
        target_R.localPosition = new Vector3(initial_pos.x+( - L_Slider.value)/1000, initial_pos.y + ( U_Slider.value - 200)/1000, initial_pos.z + ( - S_Slider.value + 230)/1000);
        //target_R.localEulerAngles = new Vector3(0, 0, -R_Slider.value);
        //target_B.localEulerAngles = new Vector3(-B_Slider.value, 0, 0);
        target_R.localEulerAngles = new Vector3( -B_Slider.value, T_Slider.value, -R_Slider.value);
        b = (int)BaseSlider.value;
        g = (int)CatchSlider.value;
        float ax, ay, az, bx, by, bz;
        float asx, asy, asz, bsx, bsy, bsz;
        float p5x, p5y, p5z;
        float C1, C23, S1, S23;

        //px = keyboard.px;
        //py = keyboard.py;
        //pz = keyboard.pz;
        //rx = keyboard.rx;
        //ry = keyboard.ry;
        //rz = keyboard.rz;
        //b = keyboard.b;
        //g = keyboard.g;/**/
        rx = R_Slider.value;
        ry = B_Slider.value;
        rz = T_Slider.value;

        ax = Mathf.Cos(rz / Mathf.Rad2Deg) * Mathf.Cos(ry / Mathf.Rad2Deg);
        ay = Mathf.Sin(rz / Mathf.Rad2Deg) * Mathf.Cos(ry / Mathf.Rad2Deg);
        az = -Mathf.Sin(ry / Mathf.Rad2Deg);
        theta[0] = angle[0] / Mathf.Rad2Deg;
        theta[1] = angle[1] / Mathf.Rad2Deg;
        theta[2] = angle[2] / Mathf.Rad2Deg;
        C1 = Mathf.Cos((float)theta[0]);
        C23 = Mathf.Cos((float)theta[1] + (float)theta[2]);
        S1 = Mathf.Sin((float)theta[0]);
        S23 = Mathf.Sin((float)theta[1] + (float)theta[2]);

        bx = Mathf.Cos(rx / Mathf.Rad2Deg) * Mathf.Sin(ry / Mathf.Rad2Deg) * Mathf.Cos(rz / Mathf.Rad2Deg) - Mathf.Sin(rx / Mathf.Rad2Deg) * Mathf.Sin(rz / Mathf.Rad2Deg);
        by = Mathf.Cos(rx / Mathf.Rad2Deg) * Mathf.Sin(ry / Mathf.Rad2Deg) * Mathf.Sin(rz / Mathf.Rad2Deg) - Mathf.Sin(rx / Mathf.Rad2Deg) * Mathf.Cos(rz / Mathf.Rad2Deg);
        bz = Mathf.Cos(rx / Mathf.Rad2Deg) * Mathf.Cos(ry / Mathf.Rad2Deg);

        asx = C23 * (C1 * ax + S1 * ay) - S23 * az;
        asy = -S1 * ax + C1 * ay;
        asz = S23 * (C1 * ax + S1 * ay) + C23 * az;
        bsx = C23 * (C1 * bx + S1 * by) - S23 * bz;
        bsy = -S1 * bx + C1 * by;
        bsz = S23 * (C1 * bx + S1 * by) + C23 * bz;

        theta[3] = Mathf.Atan2(asy, asx);
        theta[4] = Mathf.Atan2(Mathf.Cos((float)theta[3]) * asx + Mathf.Sin((float)theta[3]) * asy, asz);
        theta[5] = Mathf.Atan2(Mathf.Cos((float)theta[3]) * bsy - Mathf.Sin((float)theta[3]) * bsx, -bsz / Mathf.Sin((float)theta[4]));
        
        float gradient = 0;
        float f_x = Vector3.Distance(target_R.position, now.position);
        if (f_x > 10)
        {
            for (int i = 5; i >=0; i--)
            {
                float new_angle = angle[i];
                angle[i] += 1f;
                switch (i)
                {
                    case 0:
                        Joints[0].transform.localEulerAngles = new Vector3(angle[0], -90, -90);
                        break;
                    case 1:
                        Joints[1].transform.localEulerAngles = new Vector3(angle[1], 90, 0);
                        break;
                    case 2:
                        Joints[2].transform.localEulerAngles = new Vector3(angle[2], -90, 0);
                        break;
                    case 3:
                        Joints[3].transform.localEulerAngles = new Vector3(0, -angle[3] - 90, 180);
                        break;
                    case 4:
                        Joints[4].transform.localEulerAngles = new Vector3(angle[4], 0, 0);
                        break;
                    case 5:
                        Joints[5].transform.localEulerAngles = new Vector3(-180, -angle[5], 0);
                        break;
                }
                float f_x_plus_d = Vector3.Distance(target_R.position, now.position);
                gradient = (f_x_plus_d - f_x) / 1f;

                angle[i] = new_angle;
                if(Mathf.Abs(gradient)<0.01)
                    angle[i] -= gradient>0?0.1f:-0.1f;
                 else
                    angle[i] -= 0.1f * gradient;

                if (i == 0) 
                    angle[i] = Mathf.Clamp(angle[i], -180, 180);
                else if(i==2)
                    angle[i] = Mathf.Clamp(angle[i], 0, 135);
                else
                angle[i] = Mathf.Clamp(angle[i], -90, 90); 
                // Restores
                if (f_x_plus_d < 2)
                    break;
            }
        }
        //angle[3] = R_Slider.value;
        //angle[4] = B_Slider.value;
        //angle[5] = T_Slider.value;
        angle[3] = Mathf.Round((float)theta[3] * Mathf.Rad2Deg);
        angle[4] = Mathf.Round((float)theta[4] * Mathf.Rad2Deg);
        angle[5] = Mathf.Round((float)theta[5] * Mathf.Rad2Deg);

        //angle[3] = Mathf.Clamp(angle[3], -90, 90);
        angle[4] = Mathf.Clamp(angle[4], -90, 90);
        Joints[0].transform.localEulerAngles = new Vector3(angle[0], -90, -90);
       Joints[1].transform.localEulerAngles = new Vector3(angle[1], 90, 0);
        Joints[2].transform.localEulerAngles = new Vector3(angle[2], -90, 0);
        Joints[3].transform.localEulerAngles = new Vector3(0, -angle[3] - 90, 180);
        Joints[4].transform.localEulerAngles = new Vector3(angle[4], 0, 0);
        Joints[5].transform.localEulerAngles = new Vector3(-180, -angle[5], 0);

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
