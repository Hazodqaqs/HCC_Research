using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine.UI; 
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
//using UnityEngine.Assertions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

public class ToArduino4Dof : MonoBehaviour
{
    

    /// <summary>
    /// Parameters
    /// </summary>
    #region Parameters

    /// motor data
    public static float[] theta_offset = new float[6];
    public static float[] theta = new float[6];
    /// Start
    public Transform car;
    static float[] angle = new float[6];
    static float g;
    float ti = 0;
    private bool C_move = false;
    public Button Connect;
    public Button Disconnect;
    static TcpClient client;
    static NetworkStream stream;
    static StreamWriter sw;
    static StreamReader sr;
    public static bool con = false;
    Int32 Port = 26;
    string  laststr;
    string str;
    public static string str1;
    int scale = 1;
    bool ch = false;
    bool forward = false, stop = false, left = false, right = false;
    
    /// Update
    int leftD = 0, rightD = 0;

    int lastleftD = 0, lastrightD = 0;

    int leftdef = 0, rightdef = 0;
    bool reset = false;
    public bool VR_mode = false;
    float L = 245;//直徑
    int count = 0;
    float tr = 0;
    public bool sendOnce = true;
    bool finish = false;
    #endregion
    
    
    /// <summary>
    /// Start
    /// </summary>
    void Start()
    {
        theta_offset[0] = 90;
        theta_offset[1] = 45;
        theta_offset[2] = 5;
        theta_offset[3] = 90;
        theta_offset[4] = 125;
        theta_offset[5] = 170;

        ///
        angle[0] = IK5DOF.angle[0] + 1;
        angle[1] = IK5DOF.angle[1] + 1;
        angle[2] = IK5DOF.angle[2] + 1;
        angle[3] = IK5DOF.angle[3] + 1;
        angle[4] = IK5DOF.angle[4] + 1;
        angle[5] = IK5DOF.angle[5] + 1;

        g = IK5DOF.g ;
    }
    
    /// <summary>
    /// Update
    /// </summary>
    void Update()
    {
        angle[0] = IK5DOF.angle[0];
        angle[1] = IK5DOF.angle[1];
        angle[2] = IK5DOF.angle[2];
        angle[3] = IK5DOF.angle[3];
        angle[4] = IK5DOF.angle[4];
        angle[5] = IK5DOF.angle[5];
        
        g = IK5DOF.g;


        ///
        string str = "";
        laststr = "";
        if (VR_mode)
        {
            if (VR_controller_4dof.carMode)
            {
                if (!reset)
                {
                    reset = true;
                    forward = false;
                    stop = false;
                    left = false;
                    right = false;
                    str += "A,A,5,";
                    writeSocket(str);
                }
                else
                {
                    if (VR_controller_4dof.state == 1)
                    {
                        if (forward == false)
                        {
                            ch = true;
                            forward = true;
                        }
                        stop = false;
                        left = false;
                        right = false;
                    }
                    else if (VR_controller_4dof.state == 4)
                    {
                        if (stop == false)
                        {
                            ch = true;
                            stop = true;
                        }
                        forward = false;
                        left = false;
                        right = false;
                    }
                    else if (VR_controller_4dof.state == 3)
                    {
                        if (left == false)
                        {
                            ch = true;
                            left = true;
                        }
                        forward = false;
                        stop = false;
                        right = false;
                    }
                    else if (VR_controller_4dof.state == 2)
                    {
                        if (right == false)
                        {
                            ch = true;
                            right = true;
                        }
                        forward = false;
                        stop = false;
                        left = false;
                    }
                    else
                    {
                        if (forward)
                        {
                            ch = true;
                            forward = false;
                        }
                        else if (stop)
                        {
                            ch = true;
                            stop = false;
                        }
                        else if (left)
                        {
                            ch = true;
                            left = false;
                        }
                        else if (right)
                        {
                            ch = true;
                            right = false;
                        }
                    }
                    if (forward)
                    {
                        str += "A,A,4,";
                    }
                    else if (stop)
                    {
                        str += "A,A,1,";
                    }
                    else if (left)
                    {
                        str += "A,A,3,";
                    }
                    else if (right)
                    {
                        str += "A,A,2,";
                    }
                    else { str += "A,A,5,"; }
                    if (ch)
                    {
                        writeSocket(str);
                        Debug.Log("str:   " + str);
                        ch = false;
                    }
                    else
                    {
                    }
                }
            }
            else
            {
                if (ti > 0.1)
                {
                    
                    theta[0] = theta_offset[0] + angle[0];
                    Debug.Log("angle0:" + theta);

                    theta[1] = (theta_offset[1] - 90 + angle[1]);
                    Debug.Log("angle1:" + theta);

                    theta[2] = (theta_offset[2] + angle[2]);
                    Debug.Log("angle2:" + theta);

                    theta[3] = theta_offset[3];
                    Debug.Log("angle3:" + theta);

                    theta[4] = theta_offset[4] + angle[4];
                    Debug.Log("angle4:" + theta);

                    str = "B,B,";
                    if (-90 <= angle[0] && angle[0] <= 90)
                    {
                        str += ((int)(365 - (90 + angle[0]) * 130 / 90)).ToString() + "1,";
                    }
                    //else
                    //{
                    //    str += "2351,";
                    //}
                    
                    if (90 <= angle[1] && angle[1] <= 180)
                    {
                        str += ((int)(270 - (angle[1] - 90) * 170 / 90)).ToString() + "2,";
                    }
                    else if (45 <= angle[1] && angle[1] < 90)
                    {
                        str += ((int)(352 - (angle[1] - 45) * 82 / 45)).ToString() + "2,";
                    }
                    else if (0 <= angle[1] && angle[1] < 45)
                    {
                        str += ((int)(434 - (angle[1]) * 82 / 45)).ToString() + "2,";
                    }
                    //else
                    //{
                    //    str += "2702,";
                    //}
                    
                    
                    if (0 <= angle[2] && angle[2] <= 90)
                    {
                        str += ((int)(95 + (angle[2]) * 185 / 90)).ToString() + "3,";
                    }
                    else if (90 < angle[2] && angle[2] <= 135)
                    {
                        str += ((int)(280 + (angle[2] - 90) * 90 / 45)).ToString() + "3,";
                    }
                    //else
                    //{
                    //    str += "953,";
                    //}
                    if (angle[4] > 180)
                        angle[4] -= 360;
                    if (angle[4] < -180)
                        angle[4] += 360;
                    
                    
                    str += "2754,";
                    
                    if (-90 <= angle[4] && angle[4] <= 0)
                    {
                        str += ((int)(95 + (angle[4] + 90) * 185 / 90)).ToString() + "5,";
                    }
                    else if (0 < angle[4] && angle[4] <= 90)
                    {
                        str += ((int)(280 + (angle[4]) * 180 / 90)).ToString() + "5,";
                    }
                    //else
                    //{
                    //    str += "2805,";
                    //}
                    
                    
                    
                    if (-35 <= g && g <= 20)
                    {
                        str += ((int)(465 - (35 + g) / 55 * 155)).ToString() + "6,";
                    }
                    if (laststr == str)
                    {
                    }
                    else
                    {
                        laststr = str;
                        writeSocket(str);
                        Debug.Log(str);
                        //Debug.Log("4-2:" + angle[4]);
                        ti = 0;
                    }

                }
                else
                    ti += Time.deltaTime;
            }
        }
        else
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                C_move = !C_move;
                if (C_move)
                {
                    str += "A,A,5,";
                }
                else
                {
                    forward = false;
                    stop = false;
                    left = false;
                    right = false;
                    str += "A,A,5,";
                    writeSocket(str);
                }
            }
            if (C_move)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    if (forward == false)
                    {
                        ch = true;
                        forward = true;
                    }
                    stop = false;
                    left = false;
                    right = false;
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    if (stop == false)
                    {
                        ch = true;
                        stop = true;
                    }
                    forward = false;
                    left = false;
                    right = false;
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    if (left == false)
                    {
                        ch = true;
                        left = true;
                    }
                    forward = false;
                    stop = false;
                    right = false;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    if (right == false)
                    {
                        ch = true;
                        right = true;
                    }
                    forward = false;
                    stop = false;
                    left = false;
                }
                else
                {
                    if (forward)
                    {
                        ch = true;
                        forward = false;
                    }
                    else if (stop)
                    {
                        ch = true;
                        stop = false;
                    }
                    else if (left)
                    {
                        ch = true;
                        left = false;
                    }
                    else if (right)
                    {
                        ch = true;
                        right = false;
                    }
                }
                if (forward)
                {
                    str += "A,A,4,";
                }
                else if (stop)
                {
                    str += "A,A,1,";
                }
                else if (left)
                {
                    str += "A,A,3,";
                }
                else if (right)
                {
                    str += "A,A,2,";
                }
                else
                {
                    str += "A,A,5,";
                    count = 0; tr = 0;
                }
                if (ch)
                {
                    writeSocket(str);
                    Debug.Log("str:   " + str);
                    ch = false;
                }
                else
                {
                    if (ti > 0.3)
                    {
                        str = "";
                        ti = 0;
                    }
                    else
                        ti += Time.deltaTime;
                }
            }
            else
            {
                str += "B,B,";
                if (-90 <= angle[0] && angle[0] <= 90)
                {
                    str += ((int)(70 - angle[0] * 65 / 90)).ToString() + "1,";
                }
                if (0 <= angle[1] && angle[1] <= 180)
                {
                    str += (180 - angle[1]).ToString() + "2,";
                }
                if (0 <= angle[2] && angle[2] <= 180)
                {
                    str += (angle[2]).ToString() + "3,";
                }
                if (-90 <= angle[4] && angle[4] <= 90)
                {
                    str += (90 + angle[4]).ToString() + "5,";
                }

                //str += (180 - angle[5]).ToString() + "7,";
                //str += (90 - angle[5]).ToString() + "7,";

                if (-90 <= angle[4] && angle[4] <= 90)
                {
                    str += ((int)(120 + (40 - g) / 75 * 55)).ToString() + "6,";
                    Debug.Log(g);
                }
                if (ti > 0.3)
                {
                    writeSocket(str);
                    Debug.Log(str);
                    ti = 0;
                }
                else
                    ti += Time.deltaTime;
            }

        }
        string readdata = "";
        readdata = readSocket2();
        if(readdata!="")
            Debug.Log(readdata);

    }
    
    
    
    /// <summary>
    /// 振宇的控制
    /// </summary>
    public static void sendmessage()
    {
        Debug.Log("SendMessage2");
        
    }
    // **********************************************
    public static void setupSocket()
    {

        try
        {
            // 建立 TcpClient連線
            Int32 port = 26;
            //client = new TcpClient("192.168.1.240", port);新數
            //192.168.1.32
            //192.168.0.192

            //192.168.1.56
            client = new TcpClient("192.168.0.183", port);//192.168.0.198 工粽
            con = true;
            stream = client.GetStream();
            sw = new StreamWriter(stream);
            sr = new StreamReader(stream);
        }
        catch (ArgumentNullException A)
        {
            con = false;
            Debug.Log("ArgumentNullException:{0}" + A);
        }
        catch (SocketException S)
        {
            con = false;
            Debug.Log("SocketException:{0}" + S);
        }
    }
    public static void writeSocket(string str)
    {
        if (!con)
            return;
        try
        {
            sw.WriteLine(str);
            sw.Flush();
        }
        catch (Exception)
        {
            Debug.Log("未連線");
        }
    }
    public string readSocket2()
    {
        if (!con)
            return "";
        if (stream.DataAvailable)
            return sr.ReadLine();
        return "NoData";
    }
    public void readSocket(string str)
    {
        // Buffer to store theresponse bytes.
        Byte[] data = new Byte[256];
        Int32 bytes = 0;
        // String to store theresponse Unicode representation.
        String responseData = String.Empty;
        // Read the first batchof the TcpServer response bytes.
        try
        {
            if (stream.DataAvailable)  //如果有收到Server端的資料
            {
                //收到回應的資料顯示在Label中
                bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.Unicode.GetString(data, 0, bytes);
                str = responseData;
                Debug.Log(responseData);
            }
        }
        catch
        {
            str = "no Ack";
            Debug.Log("no Ack");
        }
    }
    public void closeSocket()
    {
        //Byte[] data = System.Text.Encoding.Unicode.GetBytes("disconnect");
        //// 取得client stream.
        //stream = client.GetStream();
        //// 送 disconnect 資料給 TcpServer.
        //stream.Write(data, 0, data.Length);
        //// 關閉串流與連線
        //stream.Close();
        //client.Close();

    }
} // end class s_TCP