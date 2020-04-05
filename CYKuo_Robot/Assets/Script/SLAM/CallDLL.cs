using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using UnityEngine.UI;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CallDLL : MonoBehaviour
{
    public GameObject kf;
    public GameObject KFRoot;
    public ComputeShader shader;
    private Mesh mesh;
    [NonSerialized]
    private Vector3[] vertices;
    public GameObject cam;
    //the name of the DLL you want to load stuff from
    private const string pluginName = "MyDLL";
    //public GameObject RGB;
    //public GameObject Depth;
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugDelegate(string str);

    static void CallBackFunction(string str) { Debug.Log(str); }
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate void ProgressCallback(int value);

    [DllImport(pluginName)]
    private static extern void DestroyWebCam();
    [DllImport(pluginName)]
    static extern void SetCallBack(ProgressCallback callback);

    [DllImport(pluginName)]
    static extern int GetTrackingState();
    [DllImport(pluginName)]
    static extern bool init_Orb_SLAM();
    [DllImport(pluginName)]
    static extern double reset_slam();
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
    private static extern int get_big_change();
    [DllImport(pluginName)]
    private static extern void set_Localozation();
    [DllImport(pluginName)]
    private static extern void set_NLocalozation();
    [DllImport(pluginName)]
    private static extern IntPtr get_kf_rgb2(int ID);


    [DllImport(pluginName)]
    private static extern IntPtr get_kf_data(int ID, out IntPtr depth, out IntPtr pose,out int width,out int height);

    [DllImport(pluginName)]
    private static extern IntPtr get_kf_depth2(int ID);

    [DllImport(pluginName)]
    private static extern IntPtr get_kf_depth(int ID);
    [DllImport(pluginName)]
    private static extern IntPtr get_imD_byte2(out int width, out int height);

    [DllImport(pluginName)]
    public static extern void SetDebugFunction(IntPtr fp);

    [DllImport(pluginName)]
    private static extern void SetTextureFromUnity(System.IntPtr texture);

    [DllImport(pluginName)]
    private static extern void SetTextureFromUnity_right(System.IntPtr texture);

    [DllImport(pluginName)]
    private static extern IntPtr GetRenderEventFunc();

    [DllImport(pluginName)]
    private static extern void SetUnityStreamingAssetsPath([MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport(pluginName)]
    private static extern void Nothing();


    [DllImport(pluginName, EntryPoint = "Update")]
    private static extern void Update1();

    [DllImport(pluginName)]
    private static extern void init_Cam();

    [DllImport(pluginName)]
    private static extern void destroy_Cam();

    private double[] mModelViewMatrix = new double[16];
    private double[] mModelViewMatrix2 = new double[16];
    private Matrix4x4 W2C_matrix;
    private Matrix4x4 W2C_matrix2;
    private double[] mPlaneMean = new double[3];
    private double[] mPlaneNormal = new double[3];
    private Vector3 mPlaneMeanVec3;
    private Vector3 mPlaneNormalVec3;

    Vector3 last_cam_position = new Vector3(0, 0, 0);
    //a record of the last world 2 the newly reset world.
    Matrix4x4 last_to_reset = Matrix4x4.identity;
    //the imu quaterion in last lost;
    Quaternion last_lost_q;
    //the roatation from last lost to current reset;
    Quaternion last_lost_to_current_q;
    bool init_ = false;
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
    void add1(int value)
    {
        if (value >= 0 && value <= 1000)
        {
            Debug.Log("EQ" + value);
        }
    }

    ComputeBuffer outputbuffer;
    int kernelIndex = -1;
    // Use this for initialization
    void Start()
    {
        DebugDelegate callback_delegate = new DebugDelegate(CallBackFunction);
        // Convert callback_delegate into a function pointer that can be
        // used in unmanaged code.
        IntPtr intptr_delegate =
            Marshal.GetFunctionPointerForDelegate(callback_delegate);
        // Call the API passing along the function pointer.
        SetDebugFunction(intptr_delegate);

        //Nothing();

        set_type(1);
        SetCallBack(add1);
        if (init_Orb_SLAM())
        {
            init_ = true;
            Debug.Log("init success!");
        }
        SetUnityStreamingAssetsPath(Application.streamingAssetsPath);
        CreateTextureAndPassToPlugin();
        ResetMesh(width, height);


        outputbuffer = new ComputeBuffer(640 * 480, 12);
        kernelIndex = shader.FindKernel("CSMain");
    }



    public void ResetMesh(out Mesh m,out Vector3[] v, int width, int height)
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
    
    // Update is called once per frame
    int kfnum=0;
    int nowkf = 0;
    int BigChange = 0;
    bool nowlocal = false;
    int nowT1 = -1;
    int nowT2 = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(nowlocal)
            {
                nowlocal = false;
                set_NLocalozation();

            }
            else
            {
                nowlocal = true;
                set_Localozation();
            }
        }


        if (nowT1 == -1)
            nowT1 = Environment.TickCount;
        //we jump into native to be able to generate the culling mimaps
        //GL.IssuePluginEvent(GetRenderEventFunc(), 0);
        var startTick = Environment.TickCount;
        var nowTick = startTick;
        int width2, height2;
        Update1();
        kfnum = get_kf_size();
        //var elapsed = Environment.TickCount - startTick;

        while (nowkf < kfnum&& (nowTick-startTick<10))
        {
            if (check_kf_depth(nowkf)&& check_kf_rgb(nowkf))
            {
                int ID=get_kf_ID(nowkf);
                string kfname = "KF" + ID + "AA";
                if (GameObject.Find(kfname)==null)
                {
                    IntPtr Depthptr = get_kf_depth2(nowkf);
                    IntPtr RGBptr = get_kf_rgb2(nowkf);
                    GameObject NewKF = Instantiate(kf);
                    NewKF.GetComponent<KF>().ID = ID;
                    NewKF.name = kfname;
                    NewKF.GetComponent<KF>().rgb= new Texture2D(width, height, TextureFormat.RGBA32, false)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                        // Set point filtering just so we can see the pixels clearly
                        filterMode = FilterMode.Bilinear,
                    };

                    // Set texture onto our matrial

                    NewKF.GetComponent<KF>().depth = new Texture2D(width, height, TextureFormat.RFloat, false, true)
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

                    NewKF.GetComponent<MeshFilter>().sharedMesh = m;
                    IntPtr Pose = get_kf_pose(nowkf);
                    Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                    W2C_matrix.m00 = (float)mModelViewMatrix[0];
                    W2C_matrix.m01 = (float)mModelViewMatrix[4];
                    W2C_matrix.m02 = (float)mModelViewMatrix[8];
                    W2C_matrix.m03 = (float)mModelViewMatrix[12];
                    W2C_matrix.m10 = (float)mModelViewMatrix[1];
                    W2C_matrix.m11 = (float)mModelViewMatrix[5];
                    W2C_matrix.m12 = (float)mModelViewMatrix[9];
                    W2C_matrix.m13 = (float)mModelViewMatrix[13];
                    W2C_matrix.m20 = (float)mModelViewMatrix[2];
                    W2C_matrix.m21 = (float)mModelViewMatrix[6];
                    W2C_matrix.m22 = (float)mModelViewMatrix[10];
                    W2C_matrix.m23 = (float)mModelViewMatrix[14];
                    W2C_matrix.m30 = 0;
                    W2C_matrix.m31 = 0;
                    W2C_matrix.m32 = 0;
                    W2C_matrix.m33 = 1;


                    Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                    Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                    Vector3 camera_position_LH;
                    camera_position_LH = new Vector3(camera_position_RH.x, -camera_position_RH.y, camera_position_RH.z);

                    NewKF.transform.position = camera_position_LH;
                    NewKF.transform.rotation = QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));




                    NewKF.transform.SetParent(KFRoot.transform);
                    shader.SetTexture(kernelIndex, "InputTexture", NewKF.GetComponent<KF>().depth);
                    outputbuffer.SetData(v);
                    shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                    shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                    outputbuffer.GetData(v);

                    m.vertices = v;
                    m.UploadMeshData(false);
                }
                else
                {
                    Debug.Log("find");
                    GameObject nkf = GameObject.Find(kfname);
                    IntPtr Depthptr = get_kf_depth2(nowkf);
                    IntPtr RGBptr = get_kf_rgb2(nowkf);
                    
                    //NewKF.GetComponent<MeshRenderer>().material.mainTexture = tex1;
                    nkf.GetComponent<KF>().rgb.LoadRawTextureData(RGBptr, 640 * 480 * 4);
                    nkf.GetComponent<KF>().depth.LoadRawTextureData(Depthptr, 640 * 480 * 4);
                    nkf.GetComponent<KF>().rgb.Apply();
                    nkf.GetComponent<KF>().depth.Apply();
                    Mesh m = nkf.GetComponent<MeshFilter>().sharedMesh;
                    
                    IntPtr Pose = get_kf_pose(nowkf);
                    Marshal.Copy(Pose, mModelViewMatrix, 0, 16);

                    W2C_matrix.m00 = (float)mModelViewMatrix[0];
                    W2C_matrix.m01 = (float)mModelViewMatrix[4];
                    W2C_matrix.m02 = (float)mModelViewMatrix[8];
                    W2C_matrix.m03 = (float)mModelViewMatrix[12];
                    W2C_matrix.m10 = (float)mModelViewMatrix[1];
                    W2C_matrix.m11 = (float)mModelViewMatrix[5];
                    W2C_matrix.m12 = (float)mModelViewMatrix[9];
                    W2C_matrix.m13 = (float)mModelViewMatrix[13];
                    W2C_matrix.m20 = (float)mModelViewMatrix[2];
                    W2C_matrix.m21 = (float)mModelViewMatrix[6];
                    W2C_matrix.m22 = (float)mModelViewMatrix[10];
                    W2C_matrix.m23 = (float)mModelViewMatrix[14];
                    W2C_matrix.m30 = 0;
                    W2C_matrix.m31 = 0;
                    W2C_matrix.m32 = 0;
                    W2C_matrix.m33 = 1;


                    Matrix4x4 C2W_matrix_RH = W2C_matrix.inverse;
                    Vector3 camera_position_RH = PositionFromMatrix(C2W_matrix_RH);

                    Vector3 camera_position_LH;
                    camera_position_LH = new Vector3(camera_position_RH.x, -camera_position_RH.y, camera_position_RH.z);

                    nkf.transform.position = camera_position_LH;
                    nkf.transform.rotation = QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH));




                    nkf.transform.SetParent(KFRoot.transform);
                    shader.SetTexture(kernelIndex, "InputTexture", nkf.GetComponent<KF>().depth);
                    outputbuffer.SetData(m.vertices);
                    shader.SetBuffer(kernelIndex, "OutputBuffer", outputbuffer);
                    shader.Dispatch(kernelIndex, uvmap.width / 16, uvmap.height / 12, 1);
                    outputbuffer.GetData(m.vertices);
                    m.colors32=nkf.GetComponent<KF>().rgb.GetPixels32(); 
                    m.UploadMeshData(false);

                }

            }
            nowkf++;
            nowTick = Environment.TickCount;


        }
        if (nowkf >= kfnum)
        {
            if (BigChange != get_big_change())
            {
                nowkf = 0;
                BigChange = get_big_change();
                Debug.Log("BigChange: " + BigChange);
                nowT1 = Environment.TickCount;
            }
            else
            {
                nowT2 = Environment.TickCount;
            }

            if (nowT2 - nowT1 > 100000)
            {
                nowT1 = Environment.TickCount;
                nowkf = 0;
                GC.Collect();
            }
        }
        IntPtr Depthptr2 = get_imD_byte2(out width2,out height2);
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
          

        IntPtr nowP = get_modelview_matrix();
        Marshal.Copy(nowP, mModelViewMatrix2, 0, 16);

        W2C_matrix2.m00 = (float)mModelViewMatrix2[0];
        W2C_matrix2.m01 = (float)mModelViewMatrix2[4];
        W2C_matrix2.m02 = (float)mModelViewMatrix2[8];
        W2C_matrix2.m03 = (float)mModelViewMatrix2[12];
        W2C_matrix2.m10 = (float)mModelViewMatrix2[1];
        W2C_matrix2.m11 = (float)mModelViewMatrix2[5];
        W2C_matrix2.m12 = (float)mModelViewMatrix2[9];
        W2C_matrix2.m13 = (float)mModelViewMatrix2[13];
        W2C_matrix2.m20 = (float)mModelViewMatrix2[2];
        W2C_matrix2.m21 = (float)mModelViewMatrix2[6];
        W2C_matrix2.m22 = (float)mModelViewMatrix2[10];
        W2C_matrix2.m23 = (float)mModelViewMatrix2[14];
        W2C_matrix2.m30 = 0;
        W2C_matrix2.m31 = 0;
        W2C_matrix2.m32 = 0;
        W2C_matrix2.m33 = 1;

        Matrix4x4 C2W_matrix_RH1 = W2C_matrix2.inverse;
        Vector3 camera_position_RH1 = PositionFromMatrix(C2W_matrix_RH1);

        Vector3 camera_position_LH1;
        camera_position_LH1 = new Vector3(camera_position_RH1.x, -camera_position_RH1.y, camera_position_RH1.z);

        transform.position = camera_position_LH1;
        transform.rotation = QuaternionFromMatrix(LHMatrixFromRHMatrix(C2W_matrix_RH1));

    }
    void OnApplicationQuit()
    {

        if (init_)
            DestroyWebCam();
        if (mesh != null)
            Destroy(null);
        outputbuffer.Dispose();
        Debug.Log("quit");
    }
}
