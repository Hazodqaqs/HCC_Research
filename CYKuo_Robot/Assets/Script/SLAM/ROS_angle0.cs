using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;

public class ROS_angle0 : UnityPublisher<Float64>
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Publish(new Float64(ToArduino4Dof.theta[0]));
    }
}
