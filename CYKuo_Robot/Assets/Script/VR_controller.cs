using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class VR_controller : MonoBehaviour {
    private SteamVR_Controller.Device device;
    private SteamVR_TrackedObject trackedObject;
    public GameObject object1;
    public static bool carMode = false;
    public static int state = 5;
    private Transform parent;
    // Use this for initialization
    void Start () {
        trackedObject = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () {
        device = SteamVR_Controller.Input((int)trackedObject.index);
        
        #region Grip
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            carMode = !carMode;
            device.TriggerHapticPulse(1000);
            Debug.Log(carMode);
        }
        Vector2 GripValue = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Grip);
        #endregion
        #region Touchpad
        if (carMode)
        { if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                if (device.GetAxis().x != 0 || device.GetAxis().y != 0)
                {
                    float deg = Mathf.Atan2(device.GetAxis().y, device.GetAxis().x) * Mathf.Rad2Deg;
                    if (deg > -45 && deg < 45)
                        state = 2;
                    else if (deg > 45 && deg < 135)
                        state = 1;
                    else if (deg > 135 || deg < -135)
                        state = 3;
                    else if (deg > -135 && deg < -45)
                        state = 4;
                }
                else
                {
                    state = 5;
                }
                device.TriggerHapticPulse(300);
            }
            else
                state = 5;
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                ToArduino.setupSocket();
        }
        else
        {
            if (device.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
            {
                if (device.GetAxis().x != 0 || device.GetAxis().y != 0)
                {
                    float deg = Mathf.Atan2(device.GetAxis().y, device.GetAxis().x) * Mathf.Rad2Deg;
                    if (deg > 45 && deg < 135)
                        object1.GetComponent<Grab_the_object>().OPClick();
                    else if (deg > -135 && deg < -45)
                        object1.GetComponent<Grab_the_object>().CLClick();
                }
                device.TriggerHapticPulse(300);
            }
        }
        #endregion
    }
    private void OnTriggerStay(Collider collider)
    {
        if (!carMode)
        {
            if (collider.name == "target")
            {
                //抓起
                if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
                {
                    collider.attachedRigidbody.isKinematic = true;
                    if (parent == null)
                        parent = collider.gameObject.transform.parent;
                    collider.gameObject.transform.SetParent(gameObject.transform);
                }

                //放開
                if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
                {
                    collider.gameObject.transform.SetParent(parent);
                }
            }
        }
    }
    
}
