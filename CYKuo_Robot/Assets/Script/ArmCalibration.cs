using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Threading;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ArmCalibration : MonoBehaviour
{
    /// <summary>
    ///  Parameters
    /// </summary>
    public Button btn_Calibration;

    public Text btn_text;
    private static float px, py, pz , ry;
    private float theta;
    public static bool StartCalibration = false;
    public bool Start_Calibration = false;

    ///
    void Change_status()
    {
        StartCalibration = !StartCalibration;
        if (StartCalibration)
        {
            btn_text.text = "Stop Calibration";
        }
        else
        {
            btn_text.text = "Calibration";
        }

        Debug.Log("btn_text:" + btn_text.text);
        Debug.Log("StartCalibration:" + StartCalibration);

        IK5DOF.VR_mode = false;
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    Thread Thread_Calibration;

    void Start()
    {
        btn_text.text = "Calibration";
        btn_Calibration.onClick.AddListener(Change_status);

        px = 230f;
        py = 0;
        pz = 200f;
    }


    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private bool working = false;
    private bool scan_finish = false;
    
    private float random_theta = 0;
    private float random_radius = 0;
    private float random_z = 0;
    private float random_ry = 0;
    void Update()
    {
        // Parameter
        random_theta = Random.Range(0.0f, 2* Mathf.PI);
        random_radius = Random.Range(150, 230);
        random_z = Random.Range(90, 250);
        random_ry = Random.Range(-60.0f, 10.0f);
            
        // Status
        StartCalibration = Start_Calibration;
        
        // Working thread
        if (StartCalibration)
        {
            if (!working)
            {
                working = true;
                Thread_Calibration = new Thread(Calibration);
                Thread_Calibration.Start();
            }
        }
        else
        {
            if(Thread_Calibration.IsAlive)
                Thread_Calibration.Join();
        }
    }

    /// <summary>
    /// Calibration
    /// </summary>
    private int count = 0;
    void Calibration()
    {
        Debug.Log("Start Calibration...");
        while (StartCalibration)
        {
            /// Wait for robot arm move to target position.
            System.Threading.Thread.Sleep(500);

            ///
            if (!scan_finish)
            {
                if (theta <= 2* Mathf.PI) // 先掃描地圖 2* Mathf.PI
                {
                    theta += 20.0f * Mathf.PI / 180.0f;//Mathf.Deg2Rad;
                    px = 230 * Mathf.Cos(theta);
                    py = 230 * Mathf.Sin(theta);
                }    
                else
                {
                    scan_finish = true;
                }
            }
            else // 隨機產生位置
            {
                float radius = random_radius;
                count++;
                theta = random_theta;
                px = radius * Mathf.Cos(theta);
                py = radius * Mathf.Sin(theta);
                pz = random_z;
                ry = random_ry;
            }
            
            /// Send to endeffect
            IK5DOF.px = px;
            IK5DOF.py = py;
            IK5DOF.pz = pz;
            IK5DOF.rx = 0;
            IK5DOF.ry = ry;
            IK5DOF.rz = 0;
        }

        
    }
}
