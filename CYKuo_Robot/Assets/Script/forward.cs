using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class forward : MonoBehaviour {
    public Transform[] Joints;
    public Transform[] Gripper;
    public Transform[] Catcher;
    public Slider S_Slider;    //PositionX min=4 max=12
    public Slider L_Slider;    //PositionY min=-4 max=4
    public Slider U_Slider;    //PositionZ min=4 max=12
    public Slider R_Slider;    //RotationX min=-90 max=90
    public Slider B_Slider;    //RotationY min=-90 max=90
    public Slider T_Slider;    //RotationZ min=-90 max=90
    public Slider BaseSlider;    //RotationZ min=0 max=360
    public Slider CatchSlider;    //RotationZ min=0 max=90
    public Text[] txtArr;

    public static float[] angle = new float[6];    //angle of the joints
    
    public int b;
    public static int g;


    // Use this for initialization
    void Start()
    {

    }
    // Update is called once per frame
    void Update ()
    {
        b = (int)BaseSlider.value;
        g = (int)CatchSlider.value;


        angle[0] = b;
        angle[1] = S_Slider.value;
        angle[2] = L_Slider.value;
        angle[3] = U_Slider.value;
        angle[4] = R_Slider.value;
        angle[5] = B_Slider.value;
        if (!double.IsNaN(angle[0]))
            Joints[0].transform.localEulerAngles = new Vector3(angle[0], -90, -90);
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
