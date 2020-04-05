#ifndef __ROS_H
#define __ROS_H

/// ROS
#define WIN32
#include "ros/ros.h"
#include "std_msgs/String.h"
#include <iostream>
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

typedef void(__stdcall * ROSCallback)(const sensor_msgs::ImageConstPtr& msg);
class ROS
{
public:
	cv::Mat V_RGB;
	cv::Mat V_Depth;
	bool Rgb_status = false, D_status = false;
	bool Show_img = false;

	/// Constructor
	ROS(bool Show_img);
	void ROS_init(string master, string Ip);
	void ROS_run();

	/// ROS Function
	bool Ros_status = false;
	bool ROS_connect();
	cv::Mat get_rgb();
	cv::Mat get_depth();

	/// Update
	void Update(cv::Mat& rgb, cv::Mat& depth);
	void ros_request_finish();

	/// Destructor
	~ROS();

	
};


/// D435 Instrinct class
class Dep_Instrinct
{
	//
public:
	float width = 640;
	float height = 480;
	float ppx = 323.65191650390625;
	float ppy = 241.19386291503906;
	float fx = 384.57;
	float fy = 384.57;
	float coeffs[5] = { 0,0,0,0,0 };
};
class Color_Instrinct
{
public:
	float width = 640;
	float height = 480;
	float ppx = 327.612;
	float ppy = 239.766;
	float fx = 617.823;
	float fy = 618.296;
	float coeffs[5] = { 0,0,0,0,0 };
};
class extrinct
{
public:
	float rotation[9] = { 0.999688, 0.0249597, 0.00110495, -0.0249596, 0.999688, 0.0000560867, -0.00110601, 0.0000284898, 0.9999 };
	float translation[3] = { 0.0148451,0.000529237,0.00030143 };
};
#endif
