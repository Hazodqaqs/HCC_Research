using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine.UI;
public class ToArduino : MonoBehaviour
{
    public Transform car;
    //SerialPort sp = new SerialPort("COM3", 9600);
    float[] angle = new float[6];
    float g;
    float ti = 0;
    private bool C_move = false;
    public Button Connect;
    public Button Disconnect;
    static TcpClient client;
    static NetworkStream stream;
    static StreamWriter sw;
    static StreamReader sr;
    static bool con = false;
    Int32 Port = 26;
    string str, laststr;
    int scale = 1;
    bool ch = false;
    bool forward = false, stop = false, left = false, right = false;
    private Vector3 car_pos;
    // Use this for initialization
    void Start()
    {
        //sp.Open();
        Connect.onClick.AddListener(setupSocket);
        Disconnect.onClick.AddListener(closeSocket);

        angle[0] = IK.angle[0] + 1;
        angle[1] = IK.angle[1] + 1;
        angle[2] = IK.angle[2] + 1;
        angle[3] = IK.angle[3] + 1;
        angle[4] = IK.angle[4] + 1;
        angle[5] = IK.angle[5] + 1;
        //angle[0] = forward.angle[0] + 1;
        //angle[1] = forward.angle[1] + 1;
        //angle[2] = forward.angle[2] + 1;
        //angle[3] = forward.angle[3] + 1;
        //angle[4] = forward.angle[4] + 1;
        //angle[5] = forward.angle[5] + 1;
        g = IK.g + 30;
    }
    int leftD = 0, rightD = 0;

    int lastleftD = 0, lastrightD = 0;

    int leftdef = 0, rightdef = 0;
    bool reset = false;
    public bool VR_mode = false;
    public bool AR_mode = false;
    float L = 245;//直徑
    int count = 0;
    float tr = 0;
    // Update is called once per frame
    void Update()
    {
        angle[0] = IK.angle[0];
        angle[1] = IK.angle[1];
        angle[2] = IK.angle[2];
        angle[3] = IK.angle[3];
        angle[4] = IK.angle[4];
        angle[5] = IK.angle[5];

        //g = IK.g + 30;
        //angle[0] = forward.angle[0] ;
        //angle[1] = forward.angle[1] ;
        //angle[2] = forward.angle[2] ;
        //angle[3] = forward.angle[3] ;
        //angle[4] = forward.angle[4] ;
        //angle[5] = forward.angle[5] ;
        g = IK.g + 30;
        string str = "";
        laststr = "";
        if (VR_mode)
        {
            if (VR_controller.carMode)
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
                    //switch (VR_controller.state)
                    //{
                    //    case 1:
                    //        car.transform.Translate(0,0, Time.deltaTime*20);//
                    //break;
                    //    case 2:
                    //        car.transform.Rotate(0, Time.deltaTime*10, 0);
                    //        break;
                    //    case 3:
                    //        car.transform.Rotate(0, -Time.deltaTime*10, 0);
                    //        break;
                    //    case 4:
                    //        car.transform.Translate(0, 0, -Time.deltaTime*20);//
                    //        break;
                    //    case 5:

                    //        break;
                    //}  
                    if (VR_controller.state == 1)
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
                    else if (VR_controller.state == 4)
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
                    else if (VR_controller.state == 3)
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
                    else if (VR_controller.state == 2)
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
                        car_pos = new Vector3(car.position.x, car.position.y, car.position.z);
                        writeSocket(str);
                        Debug.Log("str:   " + str);
                        ch = false;
                    }
                    else
                    {
                        //if (ti > 0.3)
                        //{
                        //    str = "CC,";
                        //    //writeSocket(str);
                        //    //Debug.Log(str);
                        //    ti = 0;
                        //}
                        //else
                        //    ti += Time.deltaTime;
                    }
                }
            }
            else
            {
                if (reset == true)
                    reset = false;
                str += "B,B,";
                if (0 <= angle[0] && angle[0] < 180)
                {
                    str += (160 - angle[0]).ToString() + "1,";
                    str += "02,";
                }
                else if (-180 <= angle[0] && angle[0] < 0)
                {
                    str += "1601,";
                    str += (-(int)(angle[0] * 160 / 180)).ToString() + "2,";
                }
                //if (angle[1] <= 90 && angle[1] >= -90)
                //{
                //    if (angle[1] <= 0)
                //    {
                //        str += ((int)((60 + (-angle[1])/3))).ToString() + "3,";
                //    }
                //    else
                //    {
                //        str += ((int)((60 + (-angle[1])*2 / 3))).ToString() + "3,";
                //    }
                //}
                str += (90 - (int)(angle[1])).ToString() + "3,";
                //str += (90 + angle[2]).ToString() + "4,";
                //writeSocket((90 - (int)(angle[1])).ToString() + "3");
                if (angle[2] >= 0 && angle[2] <= 135)
                {
                    str += ((int)Mathf.Clamp(angle[2] - 10, 0, 135)).ToString() + "4,";
                    //if (angle[2] <= 90)
                    //{
                    //    str += ((int)(10 + angle[2] * 60 / 90)).ToString() + "4,";
                    //}
                    //else
                    //{
                    //    str += ((int)(85 + (angle[2] - 90) * 50 / 45)).ToString() + "4,";
                    //}
                }
                //writeSocket((90 + angle[2] ).ToString() + "4");
                //if (angle[3] < 0)
                //{
                //    if (angle[3] >= -360 && angle[3] <= 360)
                //    {
                //        //str += "05,";
                //        str += ((int)Mathf.Clamp((180 - angle[3]), 0, 160)) + "5,";
                //    }
                //    if (angle[4] >= -90 && angle[4] <= 90)
                //    {
                //        str += (90 - angle[4]).ToString() + "6,";
                //    }
                //}
                //else
                //{
                //    if (angle[3] >= -360 && angle[3] <= 360)
                //    {
                //        //str += "05,";
                //        str += ((int)Mathf.Clamp((180 - angle[3]), 0, 160)).ToString() + "5,";
                //    }
                //    if (angle[4] >= -90 && angle[4] <= 90)
                //    {
                //        str += (90 - angle[4]).ToString() + "6,";
                //    }
                //}
                if (angle[3] >= 0 && angle[3] <= 180)
                {
                    str += ((int)(160 - angle[3] * 160 / 180)).ToString() + "5,";
                    str += ((int)(90 - angle[4])).ToString() + "6,";
                }
                else
                {
                    str += ((int)(160 - (180 + angle[3]) * 160 / 180)).ToString() + "5,";
                    str += ((int)(90 + angle[4])).ToString() + "6,";
                }


                //str += (90 - angle[3]).ToString() + "5,";
                //str += (90 + angle[4]).ToString() + "6,";
                if (angle[5] >= -360 && angle[5] <= 360)
                {
                    if (0 <= angle[5])
                    {
                        //str += "1807,";
                        str += ((int)Mathf.Clamp((170 - angle[5] * 170 / 180), -180, 180)).ToString() + "7,";
                    }
                    else if (angle[5] < 0)
                    {
                        //str += "1807,";
                        str += ((int)Mathf.Clamp((-angle[5] * 170 / 180), -180, 180)).ToString() + "7,";
                    }
                }

                //str += (180 - angle[5]).ToString() + "7,";
                //str += (90 - angle[5]).ToString() + "7,";
                str += g.ToString() + "8,";
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
        else if (AR_mode)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                C_move = !C_move;
            }
            if (C_move)
            {
                float h = Input.GetAxis("Horizontal");//獲取水平軸向按鍵
                float v = Input.GetAxis("Vertical");//獲取垂直軸向按鍵
                car.transform.Translate(0, 0, -v);
                car.transform.Rotate(0, h, 0);
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
                    car_pos = new Vector3(car.position.x, car.position.y, car.position.z);
                    writeSocket(str);
                    Debug.Log("str:   " + str);
                    ch = false;
                }
                else
                {
                    if (ti > 0.3)
                    {
                        str = "CC,";
                        //writeSocket(str);
                        //Debug.Log(str);
                        ti = 0;
                    }
                    else
                        ti += Time.deltaTime;
                }
                //if (Input.GetKeyUp(KeyCode.W))
                //{
                //    str += "A,A,1,";
                //}
                //else if (Input.GetKeyUp(KeyCode.S))
                //{
                //    str += "A,A,4,";
                //}
                //else if (Input.GetKeyUp(KeyCode.A))
                //{
                //    str += "A,A,2,"; 
                //}
                //else if (Input.GetKeyUp(KeyCode.D))
                //{ 
                //    str += "A,A,3,";
                //}
                //else
                //{
                //    str += "A,A,5,";
                //}
                //writeSocket(str);
                //str = "";
                //if (Input.GetKey(KeyCode.W))
                //{
                //    str += "A,A,1,";
                //}
                //else if (Input.GetKey(KeyCode.S))
                //{
                //    str += "A,A,4,";
                //}
                //else if (Input.GetKey(KeyCode.A))
                //{
                //    str += "A,A,2,";
                //}
                //else if (Input.GetKey(KeyCode.D))
                //{
                //    str += "A,A,3,";
                //}
                //else
                //{
                //    str += "A,A,5,";
                //}
                //writeSocket(str);
                //str = "";
            }
            else
            {
                str += "B,B,";
                if (0 <= angle[0] && angle[0] < 180)
                {
                    str += (160 - angle[0]).ToString() + "1,";
                    str += "02,";
                }
                else if (-180 <= angle[0] && angle[0] < 0)
                {
                    str += "1601,";
                    str += (-(int)(angle[0] * 160 / 180)).ToString() + "2,";
                }
                if (angle[1] <= 90 && angle[1] >= -90)
                {
                    if (angle[1] <= 0)
                    {
                        str += ((int)((70 + (-angle[1]) / 3))).ToString() + "3,";
                    }
                    else
                    {
                        str += ((int)((70 + (-angle[1]) * 2 / 3))).ToString() + "3,";
                    }
                }
                //writeSocket((90 - (int)(angle[1] * 160 / 180) + 10).ToString() + "3");
                if (angle[2] <= 90)
                {
                    str += ((int)(10 + angle[2] * 60 / 90)).ToString() + "4,";
                }
                else
                {
                    str += ((int)(85 + (angle[2] - 90) * 50 / 45)).ToString() + "4,";
                }
                //writeSocket((90 + angle[2] - 10).ToString() + "4");
                //if (angle[3] < 0)
                //{
                //    str += (-angle[3]).ToString() + "5,";
                //    //writeSocket((-angle[3]).ToString() + "5");
                //}
                //else 
                //{
                //    str += (180 - angle[3]).ToString() + "5,";
                //    //writeSocket((180 - angle[3]).ToString() + "5");
                //}
                if (angle[3] < 0)
                {
                    if (angle[3] >= -360 && angle[3] <= 360)
                    {
                        //str += "05,";
                        str += ((int)Mathf.Clamp((180 - angle[3]), 0, 160)) + "5,";
                    }
                    if (angle[4] >= -90 && angle[4] <= 90)
                    {
                        str += (90 - angle[4]).ToString() + "6,";
                    }
                }
                else
                {
                    if (angle[3] >= -360 && angle[3] <= 360)
                    {
                        //str += "05,";
                        str += ((int)Mathf.Clamp((180 - angle[3]), 0, 160)).ToString() + "5,";
                    }
                    if (angle[4] >= -90 && angle[4] <= 90)
                    {
                        str += (90 + angle[4]).ToString() + "6,";
                    }
                }
                //str += (90 - angle[3]).ToString() + "5,";
                //str += (90 + angle[4]).ToString() + "6,";
                if (angle[5] >= -360 && angle[5] <= 360)
                {
                    if (0 <= angle[5])
                    {
                        //str += "1807,";
                        str += ((int)Mathf.Clamp((180 - angle[5]), 0, 180)).ToString() + "7,";
                    }
                    else if (angle[5] < 0)
                    {
                        //str += "1807,";
                        str += ((int)Mathf.Clamp((-angle[5]), 0, 180)).ToString() + "7,";
                    }
                }
                {
                    str += (-angle[5]).ToString() + "7,";
                }

                //str += (180 - angle[5]).ToString() + "7,";
                //str += (90 - angle[5]).ToString() + "7,";
                str += g.ToString() + "8,";
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
        if(con)
        { if (stream.DataAvailable)
            {
                string readdata = "";
                readdata = readSocket2();
                if (readdata != "NoData" && readdata != "")
                {
                    //Debug.Log(readdata);
                    string[] sarr = readdata.Split(',');

                    if (sarr.Length > 1)
                    {
                        //Debug.Log("right:  " + sarr[0]);
                        //Debug.Log("left: " + sarr[1]);
                        lastleftD = leftD;
                        lastrightD = rightD;
                        if (sarr[0] != "")
                            rightD = -Convert.ToInt16(sarr[0]);
                        if (sarr[0] != "")
                            leftD = Convert.ToInt16(sarr[1]);
                        rightdef = rightD - lastrightD;
                        leftdef = leftD - lastleftD;

                        //if (Mathf.Abs(nowl-leftdef) < 20)
                        //    leftdef = nowl;
                        //if (Mathf.Abs(nowr-rightdef) < 20)
                        //    rightdef = nowr;
                        float rot = 0;

                        //Debug.Log("leftdef: " + leftdef + "rightdef:  " + rightdef);
                        //if (leftdef == 0 && rightdef == 0)
                        //{

                        //}
                        //else
                        //{
                        //    count++;
                        //    Debug.Log("count:" + count);
                        //}
                        if (leftdef * rightdef < 0)
                        {
                            L = 245;
                            float D = 0;
                            D = (Mathf.Abs(leftdef) - Mathf.Abs(rightdef)) / (Mathf.Abs(leftdef) + Mathf.Abs(rightdef)) * L / 2;
                            if (leftdef > 0)
                            {
                                rot = -(Mathf.Abs(leftdef) + Mathf.Abs(rightdef));
                            }
                            else
                            {
                                rot = (Mathf.Abs(leftdef) + Mathf.Abs(rightdef));
                            }
                            float degree = 0;
                            degree = rot / 468.6f * 2 * Mathf.PI * 34 / L;
                            //degree = rot / 468 * 2 * Mathf.PI * 34 / L;
                            car.transform.Translate(D * (1 - Mathf.Cos(degree)), 0, -D * Mathf.Sin((degree)));//
                            car.transform.Rotate(0, -degree * Mathf.Rad2Deg, 0);//
                        }
                        else if (leftdef * rightdef == 0)
                        {
                            L = 245;
                            float degree = 0;
                            if (leftdef == 0)
                            {
                                rot = rightdef;
                                //degree = rot / 468 * 2 * Mathf.PI * 34 / L;
                                degree = rot / 468.6f * 2 * Mathf.PI * 34 / L;
                                car.transform.Translate(L / 2 * (Mathf.Cos(degree) - 1), 0, L / 2 * Mathf.Sin((degree)));//
                                                                                                                         //tr += L / 2 * Mathf.Sin((degree));
                                                                                                                         //Debug.Log(tr);
                                car.transform.Rotate(0, -degree * Mathf.Rad2Deg, 0);//
                            }
                            else
                            {
                                rot = leftdef;
                                //degree = rot / 468 * 2 * Mathf.PI * 34 / L;
                                degree = rot / 468.6f * 2 * Mathf.PI * 34 / L;
                                car.transform.Translate(L / 2 * (1 - Mathf.Cos(degree)), 0, L / 2 * Mathf.Sin((degree)));
                                //tr += L / 2 * Mathf.Sin((degree));
                                //Debug.Log(tr);
                                car.transform.Rotate(0, degree * Mathf.Rad2Deg, 0);
                            }
                        }
                        else
                        {
                            float R = 0;
                            float degree = 0;

                            L = 245;
                            if (leftdef == rightdef)
                            {
                                car.transform.Translate(0, 0, (float)(rightdef) / 468.6f * 2 * Mathf.PI * 34);
                                //car.transform.Translate(0, 0, (float)(rightdef) / 468 * 2 * Mathf.PI * 34);
                            }
                            else
                            {
                                if (Mathf.Abs(leftdef) > Mathf.Abs(rightdef))
                                {
                                    R = rightdef * L / (leftdef - rightdef);
                                    rot = (leftdef - rightdef) / L;
                                    //degree = rot / 468 * 2 * Mathf.PI * 34;
                                    degree = rot / 468.6f * 2 * Mathf.PI * 34;
                                    car.transform.Translate((R + L / 2) * (1 - Mathf.Cos(degree)), 0, (R + L / 2) * Mathf.Sin(degree));
                                    car.transform.Rotate(0, degree * Mathf.Rad2Deg, 0);
                                }
                                else
                                {
                                    R = leftdef * L / (rightdef - leftdef);
                                    rot = (rightdef - leftdef) / L;
                                    degree = rot / 468.6f * 2 * Mathf.PI * 34;
                                    //degree = rot / 468 * 2 * Mathf.PI * 34;
                                    car.transform.Translate((R + L / 2) * (Mathf.Cos(degree) - 1), 0, (R + L / 2) * Mathf.Sin(degree));
                                    car.transform.Rotate(0, -degree * Mathf.Rad2Deg, 0);
                                }
                            }
                        }
                    }
                    //Debug.Log(readdata);


                    //Debug.Log("rightd: " + rightD + "leftd: " + leftD + "lastrightD: " + lastrightD + "    lastleftD: " + lastleftD + "rightdef: " + rightdef + "leftdef: " + leftdef);
                    //if (Mathf.Abs(leftdef) > Mathf.Abs(rightdef))
                    //{
                    //    //if (leftdef * rightdef == 0)
                    //    //{
                    //    //    if (leftdef > 0)
                    //    //    {
                    //    //        move = 0;
                    //    //        rot2 = 0;
                    //    //        rot = -(leftdef + rightdef);
                    //    //        //Debug.Log("1");
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        move = 0;
                    //    //        rot2 = 0;
                    //    //        rot = -(leftdef + rightdef);
                    //    //        //Debug.Log("2");
                    //    //    }
                    //    //}
                    //    //else
                    //    //{
                    //    //    rot2 = 0;
                    //    //    move = rightdef;
                    //    //    rot = leftdef - move;
                    //    //    //Debug.Log("5");
                    //    //}
                    //    rot2 = 0;
                    //    move = rightdef;
                    //    rot = leftdef - move;
                    //    car.transform.Translate(0, 0, move / 468 * 34 * 2 * 3.14159f);//前進或後退
                    //    float degree_rot2 = rot2 / 468 * 34 / L * 360 * 2;
                    //    float degree = rot / 468 * 34 / L * 360;
                    //    //Debug.Log("move: " + L * Mathf.Sin((180 - degree) / Mathf.Rad2Deg) / 2 + "    degree: " + degree + "    degree2: " + L * (Mathf.Cos((180 - degree) / Mathf.Rad2Deg) + 1) / 2);
                    //    car.transform.Rotate(0, -degree_rot2, 0);//旋轉

                    //    car.transform.Translate(L * (Mathf.Cos((180 - degree) / Mathf.Rad2Deg) + 1) / 2, 0, L * Mathf.Sin((180 - degree) / Mathf.Rad2Deg) / 2);//
                    //    car.transform.Rotate(0, degree, 0);//
                    //}
                    //else
                    //{
                    //    rot2 = 0;
                    //    move = leftdef;
                    //    rot = rightdef - move;
                    //    car.transform.Translate(0, 0, move / 468 * 34 * 2 * 3.14159f);//根據水平軸向按鍵來前進或後退
                    //    float degree_rot2 = rot2 / 468 * 34 / L * 360 * 2;
                    //    float degree = rot / 468 * 34 / L * 360;
                    //    //Debug.Log("move: " + L * Mathf.Sin((degree) / Mathf.Rad2Deg) / 2 + "    degree: " + degree + "    degree2: " + L * (Mathf.Cos((degree) / Mathf.Rad2Deg) - 1) / 2);
                    //    car.transform.Rotate(0, degree_rot2, 0);//根據垂直軸向按鍵來旋轉

                    //    car.transform.Translate(-L * (Mathf.Cos((-degree) / Mathf.Rad2Deg) - 1) / 2, 0, -L * Mathf.Sin((-degree) / Mathf.Rad2Deg) / 2);//
                    //    car.transform.Rotate(0, -degree, 0);//
                    //}
                }

            }
        }
    }
    // **********************************************
    public static void setupSocket()
    {

        try
        {
            // 建立 TcpClient連線
            Int32 port = 26;
            //client = new TcpClient("192.168.1.240", port);新數

            client = new TcpClient("192.168.0.182", port);//192.168.0.198 工粽
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
    public void writeSocket(string str)
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