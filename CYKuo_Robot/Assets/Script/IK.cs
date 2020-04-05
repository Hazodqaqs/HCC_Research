using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IK : MonoBehaviour {

    public Transform[] Joints;
    public Transform[] Gripper;
    public Transform[] Catcher;
    public Transform target;
    public Transform target_;
    public Transform target_R;
    public Slider S_Slider;    //PositionX min=4 max=12
    public Slider L_Slider;    //PositionY min=-4 max=4
    public Slider U_Slider;    //PositionZ min=4 max=12
    public Slider R_Slider;    //RotationX min=-90 max=90
    public Slider B_Slider;    //RotationY min=-90 max=90
    public Slider T_Slider;    //RotationZ min=-90 max=90
    public Slider BaseSlider;    //RotationZ min=0 max=360
    public Slider CatchSlider;    //RotationZ min=0 max=90
    public Text[] txtArr;
    public bool VR_mode;
    public static double[] theta = new double[6];    //angle of the joints

    public static float[] angle = new float[6];    //angle of the joints

    private float L1, L2, L3, L4, L5, L6;    //arm length in order from base
    private float C3;

    public float px = 230f, py = 350f, pz = 200f;
    public float rx = 0f, ry = 0f, rz = 0f;
    public static float b, g;
    private Vector3 initial_pos;
    private Vector3 initial_def;
    private Transform par;
    private Vector3 las_pos;
    float value1, value2, value3;
    float save3 = 0;
    // Use this for initialization
    void Start () {
        //initial_pos = new Vector3(target_R.localPosition.x, target_R.localPosition.y, target_R.localPosition.z);

        target_.position = target_R.position;
        initial_pos = new Vector3(target_.localPosition.x, target_.localPosition.y, target_.localPosition.z);
        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0;
        L1 = 102.3f;
        L2 = 108.5f;
        L3 = 103.23f;
        L4 = 28.0f;
        L5 = 26.0f;
        L6 = 50.0f;
        C3 = 0.0f;
        
        px = 230f; py = 0; pz = 90f;
        rx = 0f; ry = 0f; rz = 0f;
        par = target_R.parent;
        
    }
	
	// Update is called once per frame
	void Update () {
        float ax, ay, az, bx, by, bz;
        float asx, asy, asz, bsx, bsy, bsz;
        float p5x, p5y, p5z;
        float C1, C23, S1, S23;
        if (!VR_mode)
        {
            target_R.localPosition = new Vector3(initial_pos.x + (-L_Slider.value) , initial_pos.y + (U_Slider.value - 200) , initial_pos.z + (-S_Slider.value + 230) );
            target_R.localEulerAngles = new Vector3(-B_Slider.value, T_Slider.value, -R_Slider.value);
            target.localEulerAngles = new Vector3(0, BaseSlider.value, 0);
            px = S_Slider.value;
            py = L_Slider.value;
            pz = U_Slider.value;
            rx = R_Slider.value;
            ry = B_Slider.value;
            rz = T_Slider.value;
            b = (int)BaseSlider.value;
            g = (int)CatchSlider.value;
            
        }
        else
        {
            if (!VR_controller.carMode)
            {
                if (target_R.parent != par)
                {
                    //target_.rotation= target_R.rotation;
                    //target_.position = target_R.position;
                    //px = -(target_.localPosition.z - initial_pos.z) + 230;
                    //Debug.Log("px: " + px);
                    //py = -(target_.localPosition.x - initial_pos.x);
                    //Debug.Log("py: " + py);
                    //pz = (target_.localPosition.y - initial_pos.y) + 200;
                    //Debug.Log("pz: " + pz);
                    //rx = -target_.localEulerAngles.z;
                    //ry = -target_.localEulerAngles.x;
                    //rz = target_.localEulerAngles.y;
                }
                else
                {

                }
                target_.rotation = target_R.rotation;
                target_.position = target_R.position;
                px = -(target_.localPosition.z - initial_pos.z) + 230;
                //Debug.Log("px: " + px);
                py = -(target_.localPosition.x - initial_pos.x);
                //Debug.Log("py: " + py);
                pz = (target_.localPosition.y - initial_pos.y) + 200;
                //Debug.Log("pz: " + pz);
                rx = 0;
                ry = -target_.localEulerAngles.x;
                rz = 0;
                g = (int)CatchSlider.value;
            }
            //px = (target_R.position.z - initial_pos.z) + 230;
            //Debug.Log("px: " + px);
            //py = (target_R.position.x - initial_pos.x);
            //Debug.Log("py: " + py);
            //pz = (target_R.position.y - initial_pos.y) + 200;
            //Debug.Log("pz: " + pz);
        }
        //px = keyboard.px;
        //py = keyboard.py;
        //pz = keyboard.pz;
        //rx = keyboard.rx;
        //ry = keyboard.ry;
        //rz = keyboard.rz;
        //b = keyboard.b;
        //g = keyboard.g;/**/

        ax = Mathf.Cos(rz / Mathf.Rad2Deg) * Mathf.Cos(ry / Mathf.Rad2Deg);
        ay = Mathf.Sin(rz / Mathf.Rad2Deg) * Mathf.Cos(ry / Mathf.Rad2Deg);
        az = -Mathf.Sin(ry / Mathf.Rad2Deg);

        p5x = px - (L5 + L6) * ax;
        p5y = py - (L5 + L6) * ay;
        p5z = pz - (L5 + L6) * az;

        theta[0] = Mathf.Atan2(p5y, p5x);

        C3 = (Mathf.Pow(p5x, 2) + Mathf.Pow(p5y, 2) + Mathf.Pow(p5z - L1, 2) - Mathf.Pow(L2, 2) - Mathf.Pow(L3 + L4, 2)) / (2 * L2 * (L3 + L4));
        theta[2] = Mathf.Atan2(Mathf.Pow(1 - Mathf.Pow(C3, 2), 0.5f), C3);

        float M = L2 + (L3 + L4) * C3;
        float N = (L3 + L4) * Mathf.Sin((float)theta[2]);
        float A = Mathf.Pow(p5x * p5x + p5y * p5y, 0.5f);
        float B = p5z - L1;
        theta[1] = Mathf.Atan2(M * A - N * B, N * A + M * B);

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

        angle[0] = Mathf.Round(b + (float)theta[0] * Mathf.Rad2Deg);
        angle[1] = Mathf.Round((float)theta[1] * Mathf.Rad2Deg);
        angle[2] = Mathf.Round((float)theta[2] * Mathf.Rad2Deg);
        angle[3] = Mathf.Round((float)theta[3] * Mathf.Rad2Deg);
        angle[4] = Mathf.Round((float)theta[4] * Mathf.Rad2Deg);
        angle[5] = Mathf.Round((float)theta[5] * Mathf.Rad2Deg);
        if (angle[3] == -180)
        {
            if(save3 == 180)
                angle[3] = 180;
            else
            {
                save3 = -180;
            }
        }
        else if(angle[3] ==180)
        {
            if (save3 == -180)
                angle[3] = -180;
            else
            {
                save3 = 180;
            }
        }
        else
        {
            save3 = angle[3];
        }
        angle[0] = (int)Mathf.Clamp(angle[0], -180, 180);
        angle[1] = (int)Mathf.Clamp(angle[1], -90, 90);
        angle[2] = (int)Mathf.Clamp(angle[2], 0, 135);
        //Debug.Log(angle[0] + " , "+angle[1] + " , "+angle[2] + " , "+angle[3] + " , " + angle[4] + " , " + angle[5] + " , ");
        //Debug.Log("angle(3):"+angle[3]);
        //Debug.Log("angle(5):" + angle[5]);
        //angle[3] = (int)Mathf.Clamp(angle[3], -180, 180);
        angle[4] = (int)Mathf.Clamp(angle[4], -90, 90);
        //angle[5] = (int)Mathf.Clamp(angle[5], -180, 180);
        if (!double.IsNaN(theta[0]))
            Joints[0].transform.localEulerAngles = new Vector3(angle[0], -90, -90);
        if (!double.IsNaN(theta[1]))
        {
            Joints[1].transform.localEulerAngles = new Vector3(angle[1], 90, 0);
        }

        if (!double.IsNaN(theta[2]))
        {
            Joints[2].transform.localEulerAngles = new Vector3(angle[2], -90, 0);
        }

        if (!double.IsNaN(theta[3]))
        {
            Joints[3].transform.localEulerAngles = new Vector3(0, -angle[3] - 90, 180);
        }
        if (!double.IsNaN(theta[4]))
        {
            Joints[4].transform.localEulerAngles = new Vector3(angle[4], 0, 0);
        }

        if (!double.IsNaN(theta[5]))
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
