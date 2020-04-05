using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour {

    //追踪的手柄
    SteamVR_TrackedObject trackedObj;
    //获取输入事件
    SteamVR_Controller.Device device;
    public Transform sphere;

    // Use this for initialization
    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        Debug.Log("Start....................................." + trackedObj.name);
    }

    // Update is called once per frame
    void Update()
    {
        //获取扳机键类型
        device = SteamVR_Controller.Input((int)trackedObj.index);

    }



    private void OnTriggerStay(Collider collider)
    {
        Debug.Log("OnTriggerStay.....................................");

        //抓取物体
        //触摸中
        if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger))
        {
            // 不受重力影响
            collider.attachedRigidbody.isKinematic = true;
            //将物体设置到父级上
            collider.gameObject.transform.SetParent(gameObject.transform);
            Debug.Log("GetTouch.....................................");
        }


        //松开物体
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            // 受重力影响
            collider.attachedRigidbody.isKinematic = false;
            //将父级置为空
            collider.gameObject.transform.SetParent(null);
        }
    }

    //物体投掷
    void tossObject(Rigidbody rigidbody)
    {
        //如果是起始位置，则设置为起始位置，否则设置为父类
        Transform origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;

        //如果起始位置部位空
        if (origin != null)
        {
            //将局部坐标转换成世界坐标
            rigidbody.velocity = origin.TransformVector((device.velocity));
            rigidbody.angularVelocity = origin.TransformVector((device.angularVelocity));
        }
        else
        {
            //设置速度
            rigidbody.velocity = device.velocity;
            //设置角速度
            rigidbody.angularVelocity = device.angularVelocity;
        }
    }


    //重置小球
    private void Reset()
    {
        //扣动扳机键
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            //重置位置
            sphere.transform.position = Vector3.zero;
            //重置速度
            sphere.GetComponent<Rigidbody>().velocity = Vector3.zero;
            //重置角速度
            sphere.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
 
}
