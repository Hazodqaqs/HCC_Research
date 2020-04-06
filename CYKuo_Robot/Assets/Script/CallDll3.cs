using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Threading;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CallDll3 : MonoBehaviour
{
    /// <summary>
    /// Parameters
    /// </summary>
    public GameObject kf;
    public GameObject KFRoot;
    public ComputeShader shader;
    private Mesh mesh;
    [NonSerialized]
    private Vector3[] vertices;
    public GameObject car_pos;
    public GameObject car_rot;
    public GameObject D435;
    public GameObject Robot_;
    
    public GameObject Cam1;
    public GameObject Cam2;
    public GameObject Cam3;
    public GameObject Cam4;
    public GameObject Cam5;
    public GameObject Cam6;
    public GameObject Cam7;
    public GameObject Cam8;
    public GameObject Cam9;
    public GameObject Cam10;
    public GameObject Cam11;
    public GameObject Cam12;
    public GameObject Cam13;
    public GameObject Cam14;
    public GameObject Cam15;
    public GameObject Cam16;
    public GameObject Cam17;
    public GameObject Cam18;
    public GameObject Cam19;

    /// <summary>
    /// Import DLL
    /// </summary>
    #region Import DLL
    //the name of the DLL you want to load stuff from
    private const string pluginName = "MyDLL";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(string str);

    static void CallBackFunction(string str) { Debug.Log(str); }
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate void ProgressCallback(int value);

    //ROS
    // [DllImport(pluginName)]
    // private static extern bool Create_ROS(bool show_img);
    // [DllImport(pluginName)]
    // private static extern bool Run_ROS();
    //
    // [DllImport(pluginName)]
    // private static extern bool DestroyROS();
    
    [DllImport(pluginName)]
    private static extern void DestroyWebCam();


    // ORBSLAM
    [DllImport(pluginName)]
    static extern void SetCallBack(ProgressCallback callback);

    [DllImport(pluginName)]
    static extern int GetTrackingState();
    [DllImport(pluginName)]
    static extern bool init_Orb_SLAM();
    
    [DllImport(pluginName, EntryPoint = "Update")]
    private static extern void Update1();
    
    [DllImport(pluginName)]
    static extern double reset_slam();
    
    /// <summary>
    /// 
    /// </summary>
    [DllImport(pluginName)]
    private static extern void set_type(int _type);
    [DllImport(pluginName)]
    private static extern IntPtr get_modelview_matrix();
    [DllImport(pluginName)]
    private static extern IntPtr get_imRGB_byte2(out int width, out int height);
    [DllImport(pluginName)]
    private static extern IntPtr get_kf_rgb(int ID);
    [DllImport(pluginName)]
    private static extern IntPtr get_kf_pose(int ID);

    [DllImport(pluginName)]
    public static extern bool check_kf_rgb(int ID);
    [DllImport(pluginName)]
    public static extern bool check_kf_depth(int ID);



    [DllImport(pluginName)]
    public static extern int check_kf_ID(int ID);
    [DllImport(pluginName)]
    private static extern int get_kf_size();


    [DllImport(pluginName)]
    private static extern int get_kf_ID(int ID);
    [DllImport(pluginName)]
    private static extern int get_kf_MaxID(int ID);
    [DllImport(pluginName)]
    private static extern int get_big_change();
    [DllImport(pluginName)]
    private static extern void set_Localozation();
    [DllImport(pluginName)]
    private static extern void set_NLocalozation();
    [DllImport(pluginName)]
    private static extern IntPtr get_kf_rgb2(int ID);
    

    [DllImport(pluginName)]
    private static extern IntPtr get_kf_depth2(int ID);

    [DllImport(pluginName)]
    private static extern IntPtr get_kf_depth(int ID);
    [DllImport(pluginName)]
    private static extern IntPtr get_imD_byte2(out int width, out int height);

    /// <summary>
    /// 
    /// </summary>
    [DllImport(pluginName)]
    public static extern void SetDebugFunction(IntPtr fp);

    [DllImport(pluginName)]
    private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);


    


    #endregion

    /// <summary>
    /// Position transform matrix
    /// </summary>
    #region Position transform matrix
    private double[] mModelViewMatrix = new double[16];
    private double[] mModelViewMatrix2 = new double[16];
    private Matrix4x4 W2C_matrix;
    private Matrix4x4 W2C_matrix2;

    //Vector3 last_cam_position = new Vector3(0, 0, 0);
    ///a record of the last world 2 the newly reset world.
    //Matrix4x4 last_to_reset = Matrix4x4.identity;
    ///the imu quaterion in last lost;
    //Quaternion last_lost_q;
    ///the roatation from last lost to current reset;
    //Quaternion last_lost_to_current_q;
    
    public static Matrix4x4 LHMatrixFromRHMatrix(Matrix4x4 rhm)
    {
        Matrix4x4 lhm = new Matrix4x4(); ;

        // Column 0.
        lhm[0, 0] = rhm[0, 0];
        lhm[1, 0] = -rhm[1, 0];
        lhm[2, 0] = rhm[2, 0];
        lhm[3, 0] = rhm[3, 0];

        // Column 1.
        lhm[0, 1] = -rhm[0, 1];
        lhm[1, 1] = rhm[1, 1];
        lhm[2, 1] = -rhm[2, 1];
        lhm[3, 1] = rhm[3, 1];

        // Column 2.
        lhm[0, 2] = rhm[0, 2];
        lhm[1, 2] = -rhm[1, 2];
        lhm[2, 2] = rhm[2, 2];
        lhm[3, 2] = rhm[3, 2];

        // Column 3.
        lhm[0, 3] = rhm[0, 3];
        lhm[1, 3] = -rhm[1, 3];
        lhm[2, 3] = rhm[2, 3];
        lhm[3, 3] = rhm[3, 3];

        return lhm;
    }
    public static Vector3 PositionFromMatrix(Matrix4x4 m)
    {
        return m.GetColumn(3);
    }

    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        // Trap the case where the matrix passed in has an invalid rotation submatrix.
        if (m.GetColumn(2) == Vector4.zero)
        {
            Debug.Log("QuaternionFromMatrix got zero matrix.");
            return Quaternion.identity;
        }
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }
    #endregion

    /// <summary>
    /// Create point cloud
    /// </summary>
    private int width = 640;
    private int height = 480;
    Texture2D tex, uvmap;
    private void CreateTextureAndPassToPlugin()
    {
        // Create a texture
        tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            // Set point filtering just so we can see the pixels clearly
            filterMode = FilterMode.Bilinear,
        };

        //GetComponent<MeshRenderer>().material.mainTexture = tex;
        //RGB.GetComponent<RawImage>().texture = tex;


        // Set texture onto our matrial

        uvmap = new Texture2D(width, height, TextureFormat.RFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        //Depth.GetComponent<RawImage>().material.mainTexture = uvmap;
    }

    /// <summary>
    /// Callback Function
    /// </summary>
    /// <param name="value"></param>
    void add1(int value)
    {
        if (value >= 0 && value <= 1000)
        {
            Debug.Log("EQ" + value);
        }
    }
    
    ComputeBuffer outputbuffer;
    int kernelIndex = -1;
    #region Reset Mesh Function
    
    public void ResetMesh(out Mesh m, out Vector3[] v, int width, int height)
    {
        m = new Mesh()
        {
            indexFormat = IndexFormat.UInt32,
        };

        v = new Vector3[width * height];

        var indices = new int[v.Length];
        for (int i = 0; i < v.Length; i++)
            indices[i] = i;

        m.MarkDynamic();
        m.vertices = v;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        m.uv = uvs;

        m.SetIndices(indices, MeshTopology.Points, 0, false);
        m.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
    }
    private void ResetMesh(int width, int height)
    {

        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
            };

        vertices = new Vector3[width * height];

        var indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;

        mesh.MarkDynamic();
        mesh.vertices = vertices;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.uv = uvs;
        
            
        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    #endregion
    
    /// <summary>
    /// Find rotation matrix of car should be
    /// </summary>
    #region Quaternion Difference
    private Matrix4x4 Quaternion2RotMatrix(Quaternion from)
    {
        Matrix4x4 matrix_tmp = new Matrix4x4();
        matrix_tmp[0, 0] = Convert.ToSingle(Math.Pow(from.w, 2) + Math.Pow(from.x, 2) - Math.Pow(from.y, 2) - Math.Pow(from.z, 2));//Convert.ToSingle(1 - 2 * Math.Pow(from.y, 2) - 2 * Math.Pow(from.z, 2));
        matrix_tmp[1, 1] = Convert.ToSingle(Math.Pow(from.w, 2) - Math.Pow(from.x, 2) + Math.Pow(from.y, 2) - Math.Pow(from.z, 2));//Convert.ToSingle(1 - 2 * Math.Pow(from.x, 2) - 2 * Math.Pow(from.z, 2));
        matrix_tmp[2, 2] = Convert.ToSingle(Math.Pow(from.w, 2) - Math.Pow(from.x, 2) - Math.Pow(from.y, 2) + Math.Pow(from.z, 2));//Convert.ToSingle(1 - 2 * Math.Pow(from.x, 2) - 2 * Math.Pow(from.y, 2));
        matrix_tmp[0, 1] = 2 * from.x * from.y - 2 * from.z * from.w;
        matrix_tmp[0, 2] = 2 * from.x * from.z + 2 * from.y * from.w;
        matrix_tmp[1, 0] = 2 * from.x * from.y + 2 * from.z * from.w;
        matrix_tmp[1, 2] = 2 * from.y * from.z - 2 * from.x * from.w;
        matrix_tmp[2, 0] = 2 * from.x * from.z - 2 * from.y * from.w;
        matrix_tmp[2, 1] = 2 * from.y * from.z + 2 * from.y * from.w;

        matrix_tmp[0, 3] = 0;
        matrix_tmp[1, 3] = 0;
        matrix_tmp[2, 3] = 0;
        matrix_tmp[3, 3] = 1;
        matrix_tmp[3, 0] = 0;
        matrix_tmp[3, 1] = 0;
        matrix_tmp[3, 2] = 0;
        
        return matrix_tmp;
    }
    private Quaternion QuaternionAToB(Quaternion from, Quaternion to,int positive)
    {
        Quaternion Result = new Quaternion();
        Matrix4x4 matrix_from = Quaternion2RotMatrix(from);
        Matrix4x4 matrix_to = Quaternion2RotMatrix(to);
        Matrix4x4 Rot = Matrix4x4.Inverse(matrix_from) * matrix_to;
        
        Result.w = Convert.ToSingle(positive * 0.5 * Math.Pow(1 + Rot[0, 0] + Rot[1, 1] + Rot[2, 2], 0.5));
        Result.x = Convert.ToSingle((Rot[2, 1]-Rot[1, 2])/4/Result.w);
        Result.y = Convert.ToSingle((Rot[0, 2]-Rot[2, 0])/4/Result.w);
        Result.z = Convert.ToSingle((Rot[1, 0]-Rot[0, 1])/4/Result.w);
        
        
        return Result;
    }
    #endregion

    private Vector3 Change_coordinate(GameObject obj_,int level, int positive_x, int positive_y, int positive_z)
    {
        Vector3 tmp = new Vector3();
        switch (level)
        {
            case 0:
                tmp[0] = obj_.transform.localPosition.x*positive_x;
                tmp[1] = obj_.transform.localPosition.y*positive_y;
                tmp[2] = obj_.transform.localPosition.z*positive_z;
                break;
            case 1:
                tmp[0] = obj_.transform.localPosition.x*positive_x;
                tmp[1] = obj_.transform.localPosition.z*positive_y;
                tmp[2] = obj_.transform.localPosition.y*positive_z;
                break;
            case 2:
                tmp[0] = obj_.transform.localPosition.y*positive_x;
                tmp[1] = obj_.transform.localPosition.x*positive_y;
                tmp[2] = obj_.transform.localPosition.z*positive_z;
                break;
            case 3:
                tmp[0] = obj_.transform.localPosition.y*positive_x;
                tmp[1] = obj_.transform.localPosition.z*positive_y;
                tmp[2] = obj_.transform.localPosition.x*positive_z;
                break;
            case 4:
                tmp[0] = obj_.transform.localPosition.z*positive_x;
                tmp[1] = obj_.transform.localPosition.x*positive_y;
                tmp[2] = obj_.transform.localPosition.y*positive_z;
                break;
            case 5:
                tmp[0] = obj_.transform.localPosition.z*positive_x;
                tmp[1] = obj_.transform.localPosition.y*positive_y;
                tmp[2] = obj_.transform.localPosition.x*positive_z;
                break;
        }

        return tmp;
    }

    /// <summary>
    /// Start
    /// </summary>
    #region Start
    static bool init_ros = false;
    bool init_ORBSLAM = false;
    Thread Thread_ROS;

    public bool Start_With_ORB = true;
    public bool Start_With_ROS = false;
    // static void MyBackgroundTask()
    // {
    //     // Create_ROS(bool show RGB & Depth view)
    //     Create_ROS(false);
    //     init_ros = true;
    //     Run_ROS();
    // }

    void Start()
    {
        DebugDelegate callback_delegate = new DebugDelegate(CallBackFunction);
        // Convert callback_delegate into a function pointer that can be
        // used in unmanaged code.
        IntPtr intptr_delegate = Marshal.GetFunctionPointerForDelegate(callback_delegate);

        // Call the API passing along the function pointer.
        SetDebugFunction(intptr_delegate);

        /// ORBSLAM
        set_type(1);
        SetCallBack(add1);
        init_Orb_SLAM();
        Debug.Log("Orb_SLAM init success!");
        init_ORBSLAM = true;

        /// ROS (4 comment)
        // if (Start_With_ROS)
        // {
        //     Thread_ROS = new Thread(MyBackgroundTask);
        //     Thread_ROS.Start();
        //     System.Threading.Thread.Sleep(2000); /// Wait for ROs init
        //     init_ros = true;
        //     Debug.Log("ROS init success!");
        // }

        /// MESH
        SetUnityStreamingAssetsPath(Application.streamingAssetsPath);
        CreateTextureAndPassToPlugin();
        ResetMesh(width, height);


        outputbuffer = new ComputeBuffer(640 * 480, 12);
        kernelIndex = shader.FindKernel("CSMain");
    }

    #endregion


    /// <summary>
    /// Update
    /// </summary>
    #region Update
    int kfnum = 0;
    int nowkf = 0;
    bool nowlocal = false;
    int count = 0;
    public bool check = true;
    int kfID = -1;
    int bigIndex = -1;
    bool bigChange = false;
    int chageId = 0;
    int updatekf = 0;
    public int axis1 = 0;
    public int axis2 = 0;
    public bool op_ = true;

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (ToArduino4Dof.con)
        {
            if (VR_controller_4dof.carMode)
            {
                if (nowlocal)
                {
                    nowlocal = false;
                    set_NLocalozation();
                }
            }
            else
            {
                if (!nowlocal)
                {
                    nowlocal = true;
                    set_Localozation();
                }
            }
        }

        //we jump into native to be able to generate the culling mimaps
        //GL.IssuePluginEvent(GetRenderEventFunc(), 0);
        int width2, height2;
        Update1();
        Debug.Log("nowlocal" + nowlocal);
        if (init_ORBSLAM)
        {
            kfnum = get_kf_size();
            Debug.Log("kfnum:" + kfnum); /// Debug

            if (bigIndex == -1)
            {
                bigIndex = get_big_change();
            }

            //var elapsed = Environment.TickCount - startTick;
            if (!check)
            {

            }
            else
            {
                if (!nowlocal)
                {
                    if (chageId != get_big_change())
                    {
                        chageId = get_big_change();
                        Debug.Log("chageId" + chageId); /// Debug
                        updatekf = 0;
                    }
                    else
                    {
                        if (kfnum < 5)
                        {
                            for (int k = 0; k < kfnum; k++)
                            {
                                Debug.Log("check_kf_rgb(k):" + check_kf_rgb(k)); /// Debug
                                Debug.Log("check_kf_depth(k):" + check_kf_rgb(k)); /// Debug
                                if (check_kf_rgb(k) && check_kf_depth(k))
                                {
                                    int myID = get_kf_ID(k);
                                    string kfname = myID + "KF";
                                    Debug.Log("ID: " + myID);
                                    if (GameObject.Find(kfname) == null)
                                    {

                                        Debug.Log("new kf" + myID);
                                        IntPtr Depthptr = get_kf_depth2(k);
                                        IntPtr RGBptr = get_kf_rgb2(k);
                                        GameObject NewKF = Instantiate(kf);
                                        NewKF.GetComponent<KF>().ID = myID;
                                        NewKF.name = kfname;
                                        NewKF.GetComponent<KF>().rgb =
                                            new Texture2D(width, height, TextureFormat.RGBA32, false)
                                            {
                                                wrapMode = TextureWrapMode.Clamp,
                                                // Set point filtering just so we can see the pixels clearly
                                                filterMode = FilterMode.Bilinear,
                                            };

                                        // Set texture onto our matrial

                                        NewKF.GetComponent<KF>().depth = new Texture2D(width, height,
                                            TextureFormat.RFloat, false, true)
                                        {
                                            wrapMode = TextureWrapMode.Clamp,
                                            filterMode = FilterMode.Point,
                                        };


                                        //NewKF.GetComponent<MeshRenderer>().material.mainTexture = tex1;
                                        NewKF.GetComponent<KF>().rgb.LoadRawTextureData(RGBptr, 640 * 480 * 4);
                                        NewKF.GetComponent<KF>().depth.LoadRawTextureData(Depthptr, 640 * 480 * 4);
                                        NewKF.GetComponent<KF>().rgb.Apply();
                                        NewKF.GetComponent<KF>().depth.Apply();
                                        Vector3[] v;
                                        Mesh m = new Mesh()
                                        {
                                            indexFormat = IndexFormat.UInt32,
                                        };
                                        v = new Vector3[width * height];
                                        var indices = new int[vertices.Length];
                                        for (int i = 0; i < vertices.Length; i++)
                                            indices[i] = i;

                                        m.MarkDynamic();
                                        m.vertices = v;
                                        m.colors32 = NewKF.GetComponent<KF>().rgb.GetPixels32();
                                        m.SetIndices(indices, MeshTopology.Points, 0, false);
                                        m.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

                                        NewKF.GetComponent<MeshFilter>().sharedMesh = m;
                                        IntPtr Pose = get_kf_pose(k);
                                        Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                        W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                        W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                        W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                        W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                        W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                        W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                        W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                        W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                        W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                        W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                        W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                        W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                        W2C_matrix.m30 = 0;
                                        W2C_matrix.m31 = 0;
                                        W2C_matrix.m32 = 0;
                                        W2C_matrix.m33 = 1;


                                        Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                        Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                        Vector3 camera_position_LH;
                                        camera_position_LH = new Vector3(camera_position_RH.x, -camera_position_RH.y,
                                            camera_position_RH.z);
                                        NewKF.transform.SetParent(KFRoot.transform);
                                        NewKF.transform.localPosition = camera_position_LH;
                                        NewKF.transform.localRotation =
                                            QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));


                                        shader.SetTexture(kernelIndex, "InputTexture", NewKF.GetComponent<KF>().depth);
                                        outputbuffer.SetData(v);
                                        shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                                        shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                                        outputbuffer.GetData(v);

                                        m.vertices = v;
                                        m.UploadMeshData(false);
                                    }
                                    else if (op_)
                                    {
                                        Debug.Log("old kf" + myID);
                                        GameObject nkf = GameObject.Find(kfname);

                                        IntPtr Pose = get_kf_pose(k);
                                        Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                        W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                        W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                        W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                        W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                        W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                        W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                        W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                        W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                        W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                        W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                        W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                        W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                        W2C_matrix.m30 = 0;
                                        W2C_matrix.m31 = 0;
                                        W2C_matrix.m32 = 0;
                                        W2C_matrix.m33 = 1;


                                        Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                        Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                        Vector3 camera_position_LH;
                                        camera_position_LH = new Vector3(camera_position_RH.x, -camera_position_RH.y,
                                            camera_position_RH.z);

                                        nkf.transform.localPosition = camera_position_LH;
                                        nkf.transform.localRotation =
                                            QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));

                                    }
                                }
                            }

                            updatekf = kfnum - 1;
                        }
                        else
                        {
                            if (updatekf < kfnum - 3)
                            {
                                for (int k = updatekf; k < updatekf + 3; k++)
                                {
                                    if (check_kf_rgb(k) && check_kf_depth(k))
                                    {
                                        int myID = get_kf_ID(k);
                                        string kfname = myID + "KF";
                                        Debug.Log("ID: " + myID);
                                        if (GameObject.Find(kfname) == null)
                                        {

                                            Debug.Log("new kf" + myID);
                                            IntPtr Depthptr = get_kf_depth2(k);
                                            IntPtr RGBptr = get_kf_rgb2(k);
                                            GameObject NewKF = Instantiate(kf);
                                            NewKF.GetComponent<KF>().ID = myID;
                                            NewKF.name = kfname;
                                            NewKF.GetComponent<KF>().rgb = new Texture2D(width, height,
                                                TextureFormat.RGBA32, false)
                                            {
                                                wrapMode = TextureWrapMode.Clamp,
                                                // Set point filtering just so we can see the pixels clearly
                                                filterMode = FilterMode.Bilinear,
                                            };

                                            // Set texture onto our matrial

                                            NewKF.GetComponent<KF>().depth = new Texture2D(width, height,
                                                TextureFormat.RFloat, false, true)
                                            {
                                                wrapMode = TextureWrapMode.Clamp,
                                                filterMode = FilterMode.Point,
                                            };


                                            //NewKF.GetComponent<MeshRenderer>().material.mainTexture = tex1;
                                            NewKF.GetComponent<KF>().rgb.LoadRawTextureData(RGBptr, 640 * 480 * 4);
                                            NewKF.GetComponent<KF>().depth.LoadRawTextureData(Depthptr, 640 * 480 * 4);
                                            NewKF.GetComponent<KF>().rgb.Apply();
                                            NewKF.GetComponent<KF>().depth.Apply();
                                            Vector3[] v;
                                            Mesh m = new Mesh()
                                            {
                                                indexFormat = IndexFormat.UInt32,
                                            };
                                            v = new Vector3[width * height];
                                            var indices = new int[vertices.Length];
                                            for (int i = 0; i < vertices.Length; i++)
                                                indices[i] = i;

                                            //m.MarkDynamic();
                                            m.vertices = v;
                                            m.colors32 = NewKF.GetComponent<KF>().rgb.GetPixels32();

                                            //var uvs = new Vector2[width * height];
                                            //Array.Clear(uvs, 0, uvs.Length);
                                            //for (int j = 0; j < height; j++)
                                            //{
                                            //    for (int i = 0; i < width; i++)
                                            //    {
                                            //        uvs[i + j * width].x = i / (float)width;
                                            //        uvs[i + j * width].y = j / (float)height;
                                            //    }
                                            //}

                                            //m.uv = uvs;

                                            m.SetIndices(indices, MeshTopology.Points, 0, false);
                                            m.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

                                            NewKF.GetComponent<MeshFilter>().sharedMesh = m;
                                            IntPtr Pose = get_kf_pose(k);
                                            Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                            W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                            W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                            W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                            W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                            W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                            W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                            W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                            W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                            W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                            W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                            W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                            W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                            W2C_matrix.m30 = 0;
                                            W2C_matrix.m31 = 0;
                                            W2C_matrix.m32 = 0;
                                            W2C_matrix.m33 = 1;


                                            Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                            Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                            Vector3 camera_position_LH;
                                            camera_position_LH = new Vector3(camera_position_RH.x,
                                                -camera_position_RH.y, camera_position_RH.z);
                                            NewKF.transform.SetParent(KFRoot.transform);
                                            NewKF.transform.localPosition = camera_position_LH;
                                            NewKF.transform.localRotation =
                                                QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));





                                            shader.SetTexture(kernelIndex, "InputTexture",
                                                NewKF.GetComponent<KF>().depth);
                                            outputbuffer.SetData(v);
                                            shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                                            shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                                            outputbuffer.GetData(v);

                                            m.vertices = v;
                                            m.UploadMeshData(false);
                                        }
                                        else if (op_)
                                        {
                                            Debug.Log("old kf" + myID);
                                            GameObject nkf = GameObject.Find(kfname);

                                            IntPtr Pose = get_kf_pose(k);
                                            Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                            W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                            W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                            W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                            W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                            W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                            W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                            W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                            W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                            W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                            W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                            W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                            W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                            W2C_matrix.m30 = 0;
                                            W2C_matrix.m31 = 0;
                                            W2C_matrix.m32 = 0;
                                            W2C_matrix.m33 = 1;


                                            Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                            Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                            Vector3 camera_position_LH;
                                            camera_position_LH = new Vector3(camera_position_RH.x,
                                                -camera_position_RH.y, camera_position_RH.z);

                                            nkf.transform.localPosition = camera_position_LH;
                                            nkf.transform.localRotation =
                                                QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));

                                        }
                                    }
                                }

                                updatekf += 3;
                            }
                            else
                            {
                                for (int k = updatekf; k < kfnum; k++)
                                {
                                    if (check_kf_rgb(k) && check_kf_depth(k))
                                    {
                                        int myID = get_kf_ID(k);
                                        string kfname = myID + "KF";
                                        Debug.Log("ID: " + myID);
                                        if (GameObject.Find(kfname) == null)
                                        {

                                            Debug.Log("new kf" + myID);
                                            IntPtr Depthptr = get_kf_depth2(k);
                                            IntPtr RGBptr = get_kf_rgb2(k);
                                            GameObject NewKF = Instantiate(kf);
                                            NewKF.GetComponent<KF>().ID = myID;
                                            NewKF.name = kfname;
                                            NewKF.GetComponent<KF>().rgb = new Texture2D(width, height,
                                                TextureFormat.RGBA32, false)
                                            {
                                                wrapMode = TextureWrapMode.Clamp,
                                                // Set point filtering just so we can see the pixels clearly
                                                filterMode = FilterMode.Bilinear,
                                            };

                                            // Set texture onto our matrial

                                            NewKF.GetComponent<KF>().depth = new Texture2D(width, height,
                                                TextureFormat.RFloat, false, true)
                                            {
                                                wrapMode = TextureWrapMode.Clamp,
                                                filterMode = FilterMode.Point,
                                            };


                                            //NewKF.GetComponent<MeshRenderer>().material.mainTexture = tex1;
                                            NewKF.GetComponent<KF>().rgb.LoadRawTextureData(RGBptr, 640 * 480 * 4);
                                            NewKF.GetComponent<KF>().depth.LoadRawTextureData(Depthptr, 640 * 480 * 4);
                                            NewKF.GetComponent<KF>().rgb.Apply();
                                            NewKF.GetComponent<KF>().depth.Apply();
                                            Vector3[] v;
                                            Mesh m = new Mesh()
                                            {
                                                indexFormat = IndexFormat.UInt32,
                                            };
                                            v = new Vector3[width * height];
                                            var indices = new int[vertices.Length];
                                            for (int i = 0; i < vertices.Length; i++)
                                                indices[i] = i;

                                            //m.MarkDynamic();
                                            m.vertices = v;
                                            m.colors32 = NewKF.GetComponent<KF>().rgb.GetPixels32();

                                            //var uvs = new Vector2[width * height];
                                            //Array.Clear(uvs, 0, uvs.Length);
                                            //for (int j = 0; j < height; j++)
                                            //{
                                            //    for (int i = 0; i < width; i++)
                                            //    {
                                            //        uvs[i + j * width].x = i / (float)width;
                                            //        uvs[i + j * width].y = j / (float)height;
                                            //    }
                                            //}

                                            //m.uv = uvs;

                                            m.SetIndices(indices, MeshTopology.Points, 0, false);
                                            m.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

                                            NewKF.GetComponent<MeshFilter>().sharedMesh = m;
                                            IntPtr Pose = get_kf_pose(k);
                                            Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                            W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                            W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                            W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                            W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                            W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                            W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                            W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                            W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                            W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                            W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                            W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                            W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                            W2C_matrix.m30 = 0;
                                            W2C_matrix.m31 = 0;
                                            W2C_matrix.m32 = 0;
                                            W2C_matrix.m33 = 1;


                                            Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                            Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                            Vector3 camera_position_LH;
                                            camera_position_LH = new Vector3(camera_position_RH.x,
                                                -camera_position_RH.y, camera_position_RH.z);
                                            NewKF.transform.SetParent(KFRoot.transform);
                                            NewKF.transform.localPosition = camera_position_LH;
                                            NewKF.transform.localRotation =
                                                QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));


                                            shader.SetTexture(kernelIndex, "InputTexture",
                                                NewKF.GetComponent<KF>().depth);
                                            outputbuffer.SetData(v);
                                            shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                                            shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                                            outputbuffer.GetData(v);

                                            m.vertices = v;
                                            m.UploadMeshData(false);
                                        }
                                        else if (op_)
                                        {
                                            Debug.Log("old kf" + myID);
                                            GameObject nkf = GameObject.Find(kfname);

                                            IntPtr Pose = get_kf_pose(k);
                                            Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                                            W2C_matrix.m00 = (float) mModelViewMatrix[0];
                                            W2C_matrix.m01 = (float) mModelViewMatrix[4];
                                            W2C_matrix.m02 = (float) mModelViewMatrix[8];
                                            W2C_matrix.m03 = (float) mModelViewMatrix[12];
                                            W2C_matrix.m10 = (float) mModelViewMatrix[1];
                                            W2C_matrix.m11 = (float) mModelViewMatrix[5];
                                            W2C_matrix.m12 = (float) mModelViewMatrix[9];
                                            W2C_matrix.m13 = (float) mModelViewMatrix[13];
                                            W2C_matrix.m20 = (float) mModelViewMatrix[2];
                                            W2C_matrix.m21 = (float) mModelViewMatrix[6];
                                            W2C_matrix.m22 = (float) mModelViewMatrix[10];
                                            W2C_matrix.m23 = (float) mModelViewMatrix[14];
                                            W2C_matrix.m30 = 0;
                                            W2C_matrix.m31 = 0;
                                            W2C_matrix.m32 = 0;
                                            W2C_matrix.m33 = 1;


                                            Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                                            Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                                            Vector3 camera_position_LH;
                                            camera_position_LH = new Vector3(camera_position_RH.x,
                                                -camera_position_RH.y, camera_position_RH.z);

                                            nkf.transform.localPosition = camera_position_LH;
                                            nkf.transform.localRotation =
                                                QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));

                                        }
                                    }
                                }

                                updatekf = 0;
                            }
                        }
                    }
                }
            }

            if (!nowlocal)
            {
                IntPtr Depthptr2 = get_imD_byte2(out width2, out height2);
                IntPtr RGBptr2 = get_imRGB_byte2(out width2, out height2);
                tex.LoadRawTextureData(RGBptr2, 640 * 480 * 4);
                uvmap.LoadRawTextureData(Depthptr2, 640 * 480 * 4);
                tex.Apply();
                uvmap.Apply();

                shader.SetTexture(kernelIndex, "InputTexture", uvmap);
                outputbuffer.SetData(vertices);
                shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                outputbuffer.GetData(vertices);
                mesh.vertices = vertices;
                mesh.colors32 = tex.GetPixels32();
                mesh.UploadMeshData(false);

                if (GetTrackingState() != 3)
                {
                    IntPtr nowP = get_modelview_matrix();
                    Marshal.Copy(nowP, mModelViewMatrix2, 0, 16);

                    W2C_matrix2.m00 = (float) mModelViewMatrix2[0];
                    W2C_matrix2.m01 = (float) mModelViewMatrix2[4];
                    W2C_matrix2.m02 = (float) mModelViewMatrix2[8];
                    W2C_matrix2.m03 = (float) mModelViewMatrix2[12];
                    W2C_matrix2.m10 = (float) mModelViewMatrix2[1];
                    W2C_matrix2.m11 = (float) mModelViewMatrix2[5];
                    W2C_matrix2.m12 = (float) mModelViewMatrix2[9];
                    W2C_matrix2.m13 = (float) mModelViewMatrix2[13];
                    W2C_matrix2.m20 = (float) mModelViewMatrix2[2];
                    W2C_matrix2.m21 = (float) mModelViewMatrix2[6];
                    W2C_matrix2.m22 = (float) mModelViewMatrix2[10];
                    W2C_matrix2.m23 = (float) mModelViewMatrix2[14];
                    W2C_matrix2.m30 = 0;
                    W2C_matrix2.m31 = 0;
                    W2C_matrix2.m32 = 0;
                    W2C_matrix2.m33 = 1;

                    Matrix4x4 C2W_matrix_RH1 = W2C_matrix2.inverse;
                    Vector3 camera_position_RH1 = PositionFromMatrix(C2W_matrix_RH1);

                    // 改坐標系
                    Vector3 camera_position_LH1 = new Vector3(camera_position_RH1.x, -camera_position_RH1.y,
                        camera_position_RH1.z);
                    Quaternion SLAM_cam = QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH1));

                    /// 求車子位置&旋轉
                    // 位置：
                    Vector3 ARM_Pos = D435.transform.position - Robot_.transform.position;
                    Vector3 camera_position_a = camera_position_LH1;
                    Vector3 camera_position = camera_position_a - ARM_Pos;
                    //Debug.Log("ARM_Pos:= " + ARM_Pos);
                    //Debug.Log("camera_position:= "+camera_position);

                    // 旋轉:
                    Quaternion ARM_rot = Cam1.transform.localRotation
                                         * Cam2.transform.localRotation
                                         * Cam3.transform.localRotation
                                         * Cam4.transform.localRotation
                                         * Cam5.transform.localRotation
                                         * Cam6.transform.localRotation
                                         * Cam7.transform.localRotation
                                         * Cam8.transform.localRotation
                                         * Cam9.transform.localRotation
                                         * Cam10.transform.localRotation
                                         * Cam11.transform.localRotation
                                         * Cam12.transform.localRotation
                                         * Cam13.transform.localRotation
                                         * Cam14.transform.localRotation
                                         * Cam15.transform.localRotation
                                         * Cam16.transform.localRotation
                                         * Cam17.transform.localRotation
                                         * Cam18.transform.localRotation
                                         * Cam19.transform.localRotation
                                         * D435.transform.localRotation;
                    Quaternion diff_rot = SLAM_cam * Quaternion.Inverse(ARM_rot);

                    // 設置車子位置&旋轉
                    car_pos.transform.position = Vector3.Slerp(car_pos.transform.position, camera_position,
                        20 * Time.deltaTime);
                    car_rot.transform.localRotation =
                        Quaternion.Slerp(car_rot.transform.localRotation, diff_rot, 20 * Time.deltaTime); //*
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Exit
    /// </summary>
    void OnApplicationQuit()
    {
        Debug.Log("init_ORBSLAM: "+init_ORBSLAM);
        if (init_ORBSLAM)
            DestroyWebCam();
        if (mesh != null)
            Destroy(null);
        // if (init_ros)
        // {
        //     DestroyROS();
        //     Thread_ROS.Join();
        // }
        outputbuffer.Dispose();
        Debug.Log("quit");
    }
}
