using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grab_the_object : MonoBehaviour
{
    public Slider Grasper;
    public Button Op;
    public Button Cl;
    public Transform CAR;
    public Transform Environment;
    bool move = false;
    bool triggerStay = false;
    int sign = 1;
    bool graspObj = false;
    int count = 0;
    int stage = 0;
    bool grip1 = false;
    bool grip2 = false;
    // Use this for initialization
    void Start()
    {
        move = false;
        Op.onClick.AddListener(OPClick);
        Cl.onClick.AddListener(CLClick);
    }
    public void OPClick()
    {
        if (Grasper.value <= 0)
        {
            Grasper.value = 1;
        }
        transform.parent = Environment;
        stage = 1;
        move = true;
        sign = 1;
    }
    public void CLClick()
    {
        if (Grasper.value >= 70)
        {
            Grasper.value = 69;
        }
        stage = 2;
        move = true;
        sign = -1;
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(stage);

        if (move)
        {
            if (Grasper.value < 70 && Grasper.value > 0)
                Grasper.value += sign;
            else
                move = false;
        }
        //print("GP1:"+grip1);
        //print("GP2:"+grip2);
        //print(stage);
        if (grip1 && grip2 && stage != 1)
        {
            transform.parent = CAR;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<BoxCollider>().isTrigger = true;
            move = false;
        }
        else
        {
            transform.parent = Environment;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            gameObject.GetComponent<BoxCollider>().isTrigger = false;
        }
        if (Grasper.value >= 70)
        {
            Grasper.value = 70;
            stage = 0;
        }
        if (Grasper.value <= 0)
        {
            Grasper.value = 0;
            stage = 0;
        }
        //Debug.Log(stage);
    }

    void OnCollisionEnter(Collision aaa)
    {
        Debug.Log("C_ENTER");
        if (stage == 2)
        {
            if (aaa.gameObject.name == "GP1")
            {
                grip1 = true;
            }
            if (aaa.gameObject.name == "GP2")
            {
                grip2 = true;
            }
        }
    }
    void OnCollisionExit(Collision aaa)
    {
        Debug.Log("C_EXIT");
        if (stage == 1)
        {
            if (aaa.gameObject.name == "GP1")
            {
                grip1 = false;
            }
            if (aaa.gameObject.name == "GP2")
            {
                grip2 = false;
            }
        }
    }

    void OnTriggerEnter(Collider aaa)
    {
        Debug.Log("T_ENTER");
        if (stage == 2)
        {
            if (aaa.gameObject.name == "GP1")
            {
                grip1 = true;
            }
            if (aaa.gameObject.name == "GP2")
            {
                grip2 = true;
            }
        }
    }
    void OnTriggerExit(Collider aaa)
    {
        Debug.Log("T_EXIT");
        if (stage == 1)
        {
            if (aaa.gameObject.name == "GP1")
            {
                grip1 = false;
            }
            if (aaa.gameObject.name == "GP2")
            {
                grip2 = false;
            }
        }
    }
}
