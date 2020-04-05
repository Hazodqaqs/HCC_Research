#ifndef __VIDEOCAP_H
#define __VIDEOCAP_H

/// ROS
#define WIN32
#include "ros/ros.h"
#include "std_msgs/String.h"
#include <iostream>
//#include <System.h>
#include <string>
#include <image_transport/image_transport.h>
#include <cv_bridge/cv_bridge.h>
#pragma comment(lib, "Ws2_32.lib")
using std::string;
using namespace ros;

// Opencv
#include <opencv2\opencv.hpp>
#include <opencv2\core\core.hpp>
#include <opencv2\highgui\highgui.hpp>
#include <opencv2\imgproc\imgproc.hpp>

#include <librealsense2/rs.hpp>

//image size for ar mod rendering
//current PTAM-demo machine camera setup is flipped 90.
#define OPENCV_VIDEO_W 640
#define OPENCV_VIDEO_H 480

//image size for tracking, usually smaller for faster frame rate.
#define TRACK_IMAGE_W 640
#define TRACK_IMAGE_H 480


class VideoCap
{
public:
	int Read_type;
	VideoCap(int read_type);
	/// ROS Parameters & Function
	NodeHandle image_node;
	Subscriber RGB_image_node_sub;
	Subscriber Depth_image_node_sub;
	/// ROS Function
	bool Ros_status = false;
	void ROS_init(string master, string Ip);
	bool ROS_connect();

	// rs
	rs2::pipeline pipe;
	rs2::config pipe_config;
	rs2::pipeline_profile profile;
	rs2::stream_profile depth_stream;
	rs2::stream_profile color_stream;
	rs2::align* align_to_color;

	
	// Init
	cv::Mat V_RGB;
	cv::Mat V_Depth;
	void Update(cv::Mat& rgb, cv::Mat& depth);
	void Update();

	

	~VideoCap();

	cv::Mat get_rgb();
	cv::Mat get_depth();


};


#endif
