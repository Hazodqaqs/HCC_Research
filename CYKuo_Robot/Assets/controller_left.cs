using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controller_left : MonoBehaviour
{
    public GameObject orignin_;
    private SteamVR_Controller.Device device;
    private SteamVR_TrackedObject trackedObject;
    public GameObject CAM_RIG;
    private Vector2 axis = Vector2.zero;
    Vector3 target;
    private float y = 0;
    Collider m_Collider;
    Vector3 m_Center;
    // Start is called before the first frame update
    void Start()
    {
        m_Collider = GetComponent<Collider>();
        y = CAM_RIG.transform.position.y;
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }
    // Update is called once per frame
    void Update()
    {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        #region Touchpad
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
        {
            axis = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
            if (CAM_RIG != null)
            {
                
                CAM_RIG.transform.position += (transform.right * axis.x + transform.forward * axis.y) * Time.deltaTime;
                CAM_RIG.transform.position = new Vector3(CAM_RIG.transform.position.x, y, CAM_RIG.transform.position.z);
            }

        }

        #endregion
        #region Trigger
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (orignin_.GetComponent<LineRenderer>().positionCount > 1)
            {
                m_Center = m_Collider.bounds.center;
                Debug.Log("Collider Center : " + m_Center);
                orignin_.GetComponent<LineRenderer>().SetPosition(1, m_Center);
            }
            else
            {
                m_Center = m_Collider.bounds.center;
                Debug.Log("Collider Center : " + m_Center);
                orignin_.GetComponent<LineRenderer>().positionCount = 2;
                orignin_.GetComponent<LineRenderer>().SetPosition(1, m_Center);

            }
        }
        #endregion

        #region Grip
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            orignin_.GetComponent<LineRenderer>().positionCount=1;
        }
        Vector2 GripValue = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Grip);
        #endregion
    }
}
