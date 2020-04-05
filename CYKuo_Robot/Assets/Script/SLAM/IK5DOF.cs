using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IK5DOF : MonoBehaviour
{
    /// <summary>
    /// Parameters
    /// </summary>
    public Transform[] Joints;

    public Transform[] Gripper;
    public Transform[] Catcher;
    //public Transform target;
    public Transform target_item;
    public Transform target_pos;
    public Transform arm_base_coordinate;
    public Slider S_Slider; //PositionX min=4 max=12
    public Slider L_Slider; //PositionY min=-4 max=4
    public Slider U_Slider; //PositionZ min=4 max=12
    public Slider R_Slider; //RotationX min=-90 max=90
    public Slider B_Slider; //RotationY min=-90 max=90
    public Slider T_Slider; //RotationZ min=-90 max=90
    public Slider BaseSlider; //RotationZ min=0 max=360
    public Slider CatchSlider; //RotationZ min=0 max=90
    public Text[] txtArr;
    public static bool VR_mode = true;
    public static double[] theta = new double[6]; //angle of the joints

    public static float[] angle = new float[6]; //angle of the joints

    // Arm Parameters
    private static float L1, L2, L3, L4, L5, L6; //arm length in order from base
    private static float C3;

    public static float px = 230f, py = 350f, pz = 200f;
    public static float rx = 0f, ry = 0f, rz = 0f;
    public static float b, g;
    private Vector3 initial_pos;
    private Vector3 initial_def;
    private Transform par;
    private Vector3 las_pos;
    float value1, value2, value3;
    float save3 = 0;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        //if (VR_mode)
        //{
            //target_item.position = target_pos.position;
            //initial_pos = new Vector3(target_item.localPosition.x, target_item.localPosition.y, target_item.localPosition.z);
        
            //par = target_pos.parent;
        //}

        theta[0] = theta[1] = theta[2] = theta[3] = theta[4] = theta[5] = 0.0;
        L1 = 30.26f; //22.26
        L2 = 103.0f; //104.0
        L3 = 92.5404f;
        L4 = 52.2011f;
        L5 = 0.0f;
        L6 = 136.9981f;
        C3 = 0.0f;

        px = 230f;
        py = 0.0f;
        pz = 200f;
        rx = 0f;
        ry = 0f;
        rz = 0f;
        g = -30;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        float p3xy, p3z;

        //
        if (!ArmCalibration.StartCalibration)
        {
            if (!VR_mode)
            {
                px = S_Slider.value;
                py = L_Slider.value;
                pz = U_Slider.value;
                
                ry = B_Slider.value;
                
                b = (int) BaseSlider.value;
                g = (int) CatchSlider.value;

            }
            else
            {
                ///
                //target_item.rotation = target_pos.rotation;
                //target_item.position = target_pos.position / 1000;
                
                px = (target_item.localPosition.z*2 - arm_base_coordinate.localPosition.z)*1000;
                py = -(target_item.localPosition.x - arm_base_coordinate.localPosition.x)*1000;
                pz = (target_item.localPosition.y - arm_base_coordinate.localPosition.y)*1000;
                
                rx = 0;
                ry = target_item.localEulerAngles.x;
                rz = 0;
                
                Debug.Log("target_item.localPosition: " + target_item.localPosition*1000);
                Debug.Log("arm_base_coordinate.localPosition: " + arm_base_coordinate.localPosition*1000);

                Debug.Log("px: " + px);
                Debug.Log("py: " + py);
                Debug.Log("pz: " + pz);
                
                /// 陣雨
                // target_item.rotation = target_pos.rotation;
                // target_item.position = target_pos.position;
                // px = -(target_item.localPosition.z - initial_pos.z) + 230;
                // Debug.Log("px: " + px);
                // py = -(target_item.localPosition.x - initial_pos.x);
                // Debug.Log("py: " + py);
                // pz = (target_item.localPosition.y - initial_pos.y) + 90;
                // Debug.Log("pz: " + pz);
                // rx = 0;
                // ry = target_item.localEulerAngles.x;
                // rz = 0;
                //g = (int)CatchSlider.value;
                //Debug.Log("pz: " + pz);
            }
        }


        //px = keyboard.px;
        //py = keyboard.py;
        //pz = keyboard.pz;
        //rx = keyboard.rx;
        //ry = keyboard.ry;
        //rz = keyboard.rz;
        //b = keyboard.b;
        //g = keyboard.g;/**/

        ///
        if (false)
        {

            if (!double.IsNaN(theta[0]))
            {
                Joints[0].transform.localEulerAngles = new Vector3(90, 180, -180 + px);
            }

            if (!double.IsNaN(theta[1]))
            {
                Joints[1].transform.localEulerAngles = new Vector3(90, 0, -90 + py);
            }

            if (!double.IsNaN(theta[2]))
            {
                Joints[2].transform.localEulerAngles = new Vector3(0, -90, 90 + pz);
            }

            if (!double.IsNaN(theta[3]))
            {
                Joints[3].transform.localEulerAngles = new Vector3(90, 0, -90 + rx);
            }

            if (!double.IsNaN(theta[4]))
            {
                Joints[4].transform.localEulerAngles = new Vector3(180, 90, -90 + ry);
            }

            if (!double.IsNaN(theta[5]))
            {
                //if (angle[5] < 0) angle[5] = 0;
                //if (angle[5] > 180) angle[5] = 180;
                //Joints[5].transform.localEulerAngles = new Vector3(-180, -angle[5], 0);/**/
            }

            Gripper[0].transform.localEulerAngles = new Vector3(-0.347f, -9.994f, -1.97f - g);
            Gripper[1].transform.localEulerAngles = new Vector3(-0.347f, -9.994f, -1.97f + g);
            Gripper[2].transform.localEulerAngles = new Vector3(0, -10, -g);
            Gripper[3].transform.localEulerAngles = new Vector3(0, -10, g);

            Catcher[0].transform.localEulerAngles = new Vector3(0, 0, g);
            Catcher[1].transform.localEulerAngles = new Vector3(0, 0, -g);
            txtArr[0].text = (angle[0]).ToString();
            txtArr[1].text = (angle[1]).ToString();
            txtArr[2].text = (angle[2]).ToString();
            txtArr[3].text = (angle[3]).ToString();
            txtArr[4].text = (angle[4]).ToString();
            txtArr[5].text = (angle[5]).ToString();
            txtArr[6].text = g.ToString();
        }
        else
        {
            p3z = pz - (L6) * Mathf.Sin(ry / Mathf.Rad2Deg);
            p3xy = Mathf.Sqrt(px * px + py * py) - (L6) * Mathf.Cos(ry / Mathf.Rad2Deg);
            theta[0] = Mathf.Atan2(py, px);


            theta[2] = Mathf.Acos((p3z * p3z + p3xy * p3xy - L2 * L2 - (L3 + L4) * (L3 + L4)) / 2 / L2 / (L3 + L4));
            theta[1] = Mathf.Atan2(p3z, p3xy) + Mathf.Atan2((L3 + L4) * Mathf.Sin((float) theta[2]), 
                L2 + (L3 + L4) * Mathf.Cos((float) theta[2]));

            theta[3] = 0;
            theta[4] = ry / Mathf.Rad2Deg + theta[2] - theta[1];
            angle[0] = Mathf.Round(b + (float) theta[0] * Mathf.Rad2Deg);
            //Debug.Log(" ry:" + ry );
            //Debug.Log(" theta[2]:" + theta[2]);
            Debug.Log("theta[1]:" + theta[1]);
            //Debug.Log("theta[4]:" + theta[4]);
            angle[1] = Mathf.Round((float) theta[1] * Mathf.Rad2Deg);
            angle[2] = Mathf.Round((float) theta[2] * Mathf.Rad2Deg);
            angle[4] = Mathf.Round((float) theta[4] * Mathf.Rad2Deg);

            //Debug.Log(" angle[4]:" + angle[4]);
            if (!double.IsNaN(theta[0]))
                Joints[0].transform.localEulerAngles = new Vector3(90, 180, -90 + angle[0]);
            if (!double.IsNaN(theta[1]))
            {
                Joints[1].transform.localEulerAngles = new Vector3(270, 0, -90 + (90 - angle[1])); // +(90- angle[1])
            }

            if (!double.IsNaN(theta[2]))
            {
                Joints[2].transform.localEulerAngles = new Vector3(0, -90, 90 - (angle[2]));
            }

            //if (!double.IsNaN(theta[3]))
            //{
            //    Joints[3].transform.localEulerAngles = new Vector3(90, 0, -90 +(90 - angle[3]));
            //}
            if (!double.IsNaN(theta[4]))
            {
                Joints[4].transform.localEulerAngles = new Vector3(180, 90, -90 + angle[4]);
            }

            if (!double.IsNaN(theta[5]))
            {
                //if (angle[5] < 0) angle[5] = 0;
                //if (angle[5] > 180) angle[5] = 180;
                //Joints[5].transform.localEulerAngles = new Vector3(-180, -angle[5], 0);/**/
            }

            Gripper[0].transform.localEulerAngles = new Vector3(-0.347f, -9.994f, -1.97f - g);
            Gripper[1].transform.localEulerAngles = new Vector3(-0.347f, -9.994f, -1.97f + g);
            Gripper[2].transform.localEulerAngles = new Vector3(0, -10, -g);
            Gripper[3].transform.localEulerAngles = new Vector3(0, -10, g);

            Catcher[0].transform.localEulerAngles = new Vector3(0, 0, g);
            Catcher[1].transform.localEulerAngles = new Vector3(0, 0, -g);
        }

    }
}
