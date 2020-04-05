using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class target1 : MonoBehaviour {

    public Slider S_Slider;
    public Slider L_Slider;
    public Slider U_Slider;
    public Slider R_Slider;
    public Slider B_Slider;
    public Slider T_Slider;
    public Slider Base_Slider;
    public Transform target;
    public Transform target_B;
    public Transform target_T;
    private Vector3 initial_pos;
    // Use this for initialization
    void Start () {
        initial_pos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);

    }
	
	// Update is called once per frame
	void Update () {

        target.position = new Vector3(initial_pos.x - L_Slider.value, initial_pos.y + U_Slider.value - 200, initial_pos.z - S_Slider.value + 230);
        target.eulerAngles = new Vector3(0, 0, -R_Slider.value);
        target_B.localEulerAngles = new Vector3(-B_Slider.value, 0, 0);
        target_T.localEulerAngles = new Vector3(0, 0, -T_Slider.value);
    }
}
