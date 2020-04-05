using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine.UI;

public class TEST_ToArduino : MonoBehaviour
{
    //SerialPort sp = new SerialPort("COM3", 9600);
    float[] angle = new float[6];
    float g;
    float ti = 0;

    public Button Connect;
    public Button Disconnect;
    TcpClient client;
    NetworkStream stream;
    StreamWriter sw;
    StreamReader sr;
    bool con = false;
    Int32 Port = 26;
    string str;
    int scale = 1;
    bool ch = false;
    // Use this for initialization
    void Start()
    {
        //sp.Open();
        Connect.onClick.AddListener(setupSocket);
        Disconnect.onClick.AddListener(closeSocket);

        angle[0] = TEST_IK.angle[0];
        angle[1] = TEST_IK.angle[1];
        angle[2] = TEST_IK.angle[2];
        angle[3] = TEST_IK.angle[3];
        angle[4] = TEST_IK.angle[4];
        angle[5] = TEST_IK.angle[5];
        g = TEST_IK.g + 30;
    }

    // Update is called once per frame
    void Update()
    {
        angle[0] = (int)TEST_IK.angle[0];
        angle[1] = (int)TEST_IK.angle[1];
        angle[2] = (int)TEST_IK.angle[2];
        angle[3] = (int)TEST_IK.angle[3];
        angle[4] = (int)TEST_IK.angle[4];
        angle[5] = (int)TEST_IK.angle[5];
        
        g = TEST_IK.g + 30;
        string str = "";
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
        str += (90 - (int)(angle[1])).ToString() + "3,";
        //writeSocket((90 - (int)(angle[1] * 160 / 180) + 10).ToString() + "3");
        str += (90 + angle[2]).ToString() + "4,";
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
        str += (90 - angle[3]).ToString() + "5,";
        str += (90 + angle[4]).ToString() + "6,";
        //if (angle[3] < 0)
        //{
        //    str += (90 + angle[4]).ToString() + "6,";
        //    //writeSocket((90 + angle[4]).ToString() + "6");
        //}
        //else
        //{
        //    str += (90 - angle[4]).ToString() + "6,";
        //    //writeSocket((90 - angle[4]).ToString() + "6");
        //}
        str += (90 - angle[5]).ToString() + "7,";
        //if (0 <= angle[5] && angle[5] <= 90)
        //{
        //    str += (90 - angle[5]).ToString() + "7,";
        //    //writeSocket((90 - angle[5]).ToString() + "7");
        //}
        //else if (90 <= angle[5] && angle[5] <= 180)
        //{
        //    str += (270 - angle[5]).ToString() + "7,";
        //    //writeSocket((270 - angle[5]).ToString() + "7");
        //}
        //else if (-90 <= angle[5] && angle[5] <= 0)
        //{
        //    str += (90 - angle[5]).ToString() + "7,";
        //    //writeSocket((90 - angle[5]).ToString() + "7");
        //}
        //else if (-180 <= angle[5] && angle[5] <= -90)
        //{
        //    str += (-90 - angle[5]).ToString() + "7,";
        //    //writeSocket((-90 - angle[5]).ToString() + "7");
        //}
        str += g.ToString() + "8,";
        //writeSocket(g.ToString() + "8");
        if (ti > 1)
        {
            writeSocket(str);
            ti = 0;
        }
        else
            ti += Time.deltaTime;
        //if (con)
        //{
        //    if (angle[0] != IK.angle[0] && ti[0] >= 0.8 * scale)
        //    {
        //        if (0 <= angle[0] && angle[0] < 180)
        //        {
        //            str = (160 - angle[0]).ToString() + "1,";
        //            //writeSocket((160 - angle[0]).ToString() + "1");
        //        }
        //        else if (-180 <= angle[0] && angle[0] < 0)
        //        {
        //            str = "1601,";
        //            //writeSocket("1601");
        //        }
        //        angle[0] = IK.angle[0];
        //        ch = true;
        //    }
        //    else
        //    {
        //        ti[0] += Time.deltaTime;
        //        if (ch && ti[1] >= 0.8 * scale)
        //        {
        //            if (0 <= angle[0] && angle[0] < 180)
        //            {
        //                //writeSocket("02");
        //            }
        //            else if (-180 <= angle[0] && angle[0] < 0)
        //            {
        //                writeSocket((-(int)(angle[0] * 160 / 180)).ToString() + "2");
        //            }
        //            ch = false;
        //        }
        //        else
        //        {
        //            ti[1] += Time.deltaTime;
        //            if (angle[1] != IK.angle[1] && ti[2] >= 0.8 * scale)
        //            {
        //                writeSocket((90 - (int)(angle[1] * 160 / 180) + 10).ToString() + "3");
        //                angle[1] = IK.angle[1];
        //            }
        //            else
        //            {
        //                ti[2] += Time.deltaTime;
        //                if (angle[2] != IK.angle[2] && ti[3] >= 0.8 * scale)
        //                {
        //                    writeSocket((90 + angle[2] - 10).ToString() + "4");
        //                    angle[2] = IK.angle[2];
        //                }
        //                else
        //                {
        //                    ti[3] += Time.deltaTime;
        //                    if (angle[3] != IK.angle[3] && ti[4] >= 0.8 * scale)
        //                    {
        //                        if (angle[3] < 0)
        //                        {
        //                            writeSocket((-angle[3]).ToString() + "5");
        //                        }
        //                        else
        //                        {
        //                            writeSocket((180 - angle[3]).ToString() + "5");
        //                        }
        //                        angle[3] = IK.angle[3];
        //                    }
        //                    else
        //                    {
        //                        ti[4] += Time.deltaTime;
        //                        if (angle[4] != IK.angle[4] && ti[5] >= 0.8 * scale)
        //                        {
        //                            if (angle[3] < 0)
        //                            {
        //                                writeSocket((90 + angle[4]).ToString() + "6");
        //                            }
        //                            else
        //                            {
        //                                writeSocket((90 - angle[4]).ToString() + "6");
        //                            }
        //                            angle[4] = IK.angle[4];
        //                        }
        //                        else
        //                        {
        //                            ti[5] += Time.deltaTime;
        //                            if (angle[5] != IK.angle[5] && ti[6] >= 0.8 * scale)
        //                            {

        //                                if (0 <= angle[5] && angle[5] <= 90)
        //                                {
        //                                    writeSocket((90 - angle[5]).ToString() + "7");
        //                                }
        //                                else if (90 <= angle[5] && angle[5] <= 180)
        //                                {
        //                                    writeSocket((270 - angle[5]).ToString() + "7");
        //                                }
        //                                else if (-90 <= angle[5] && angle[5] <= 0)
        //                                {
        //                                    writeSocket((90 - angle[5]).ToString() + "7");
        //                                }
        //                                else if (-180 <= angle[5] && angle[5] <= -90)
        //                                {
        //                                    writeSocket((-90 - angle[5]).ToString() + "7");
        //                                }
        //                                angle[5] = IK.angle[5];
        //                            }
        //                            else
        //                            {
        //                                ti[6] += Time.deltaTime;
        //                                if (g != IK.g && ti[7] >= 0.8 * scale)
        //                                {
        //                                    writeSocket(g.ToString() + "8");
        //                                    g = IK.g;
        //                                }
        //                                else
        //                                {
        //                                    ti[7] += Time.deltaTime;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
    // **********************************************
    public void setupSocket()
    {

        try
        {
            // 建立 TcpClient連線
            Int32 port = 26;
            client = new TcpClient("192.168.1.240", port);
            con = true;
            stream = client.GetStream();
            sw = new StreamWriter(stream);
            sw = new StreamWriter(stream);
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
        Byte[] data = System.Text.Encoding.Unicode.GetBytes("disconnect");
        // 取得client stream.
        stream = client.GetStream();
        // 送 disconnect 資料給 TcpServer.
        stream.Write(data, 0, data.Length);
        // 關閉串流與連線
        stream.Close();
        client.Close();
    }
} // end class s_TCP