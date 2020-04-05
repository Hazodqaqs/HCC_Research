#include "stdafx.h"
#include "VideoCap.h"

using namespace cv;

cv::Mat rgb_img,depth_img;
bool ros_status = false;

void ROS_RGB_Callback(const sensor_msgs::ImageConstPtr& msg)
{
	cv_bridge::CvImageConstPtr cv_ptr;
	try
	{
		cv_ptr = cv_bridge::toCvCopy(msg, sensor_msgs::image_encodings::TYPE_8UC3);
		//cv::imwrite("../x64/Release/image/image.jpg", cv_ptr->image);
		//cout << "Save image..." << endl;
		//cv::imshow("view", cv_ptr->image);
		rgb_img = cv_ptr->image;
		cv::waitKey(20);
		ros_status = true;
	}
	catch (cv_bridge::Exception& e)
	{
		ROS_ERROR("Could not convert from '%s' to 'TYPE_8UC3'.", msg->encoding.c_str());
	}
}
void ROS_Depth_Callback(const sensor_msgs::ImageConstPtr& msg)
{
	cv_bridge::CvImageConstPtr cv_ptr;
	try
	{
		cv_ptr = cv_bridge::toCvCopy(msg, sensor_msgs::image_encodings::TYPE_16UC1);
		//cv::imwrite("../x64/Release/image/image.jpg", cv_ptr->image);
		//cout << "Save image..." << endl;
		//cv::imshow("view", cv_ptr->image);
		depth_img = cv_ptr->image;
		cv::waitKey(10);
		ros_status = true;
	}
	catch (cv_bridge::Exception& e)
	{
		ROS_ERROR("Could not convert from '%s' to 'TYPE_16UC1'.", msg->encoding.c_str());
	}
}

void VideoCap::ROS_init(string master, string Ip)
{
	/// Initialize ROS node
	std::cout << "Start Node(Get_Image)..." << std::endl;

	std::map<std::string, std::string> params;
	params.insert(std::pair<std::string, std::string>("__master", master));
	params.insert(std::pair<std::string, std::string>("__ip", Ip));

	std::cout << "Init Node(Get_Image)..." << std::endl;
	ros::init(params, "Get_Image");
	RGB_image_node_sub = image_node.subscribe<sensor_msgs::Image>("/camera/color/image_raw", 1000, ROS_RGB_Callback);
	Depth_image_node_sub = image_node.subscribe<sensor_msgs::Image>("/camera/depth/image_rect_raw", 1000, ROS_Depth_Callback);
	ros::spin();
}

bool VideoCap::ROS_connect()
{
	Ros_status = ros_status;
	return Ros_status;
}
/// Defalut constructor
// Defalut constructor (Old)
VideoCap::VideoCap(int read_type)
{
	Read_type = read_type;
	if (Read_type == 0)
	{
		pipe_config.enable_stream(RS2_STREAM_DEPTH, 640, 480, RS2_FORMAT_Z16, 60);
		pipe_config.enable_stream(RS2_STREAM_COLOR, 640, 480, RS2_FORMAT_BGR8, 60);
		profile = pipe.start(pipe_config);
		depth_stream = profile.get_stream(RS2_STREAM_DEPTH).as<rs2::video_stream_profile>();
		color_stream = profile.get_stream(RS2_STREAM_COLOR).as<rs2::video_stream_profile>();
		align_to_color = new rs2::align(RS2_STREAM_COLOR);
	}
	else // Defalut constructor (New)
	{
		ROS_init("http://raspberry:11311", "raspberry");//172.25.250.214
	}
}


/// Update
// Update (Old)
void VideoCap::Update(cv::Mat& rgb, cv::Mat& depth) 
{
	if (Read_type == 0)
	{
		rs2::frameset frameset = pipe.wait_for_frames();

		frameset = align_to_color->process(frameset);
		rs2::frame color_frame = frameset.get_color_frame();//processed.first(align_to);
		rs2::frame depth_frame = frameset.get_depth_frame();
		//獲取寬高
		const int depth_w = depth_frame.as<rs2::video_frame>().get_width();
		const int depth_h = depth_frame.as<rs2::video_frame>().get_height();
		const int color_w = color_frame.as<rs2::video_frame>().get_width();
		const int color_h = color_frame.as<rs2::video_frame>().get_height();

		//創建OPENCV類型 並傳入數據
		depth = cv::Mat(Size(depth_w, depth_h), CV_16U, (void*)depth_frame.get_data(), Mat::AUTO_STEP);
		rgb = cv::Mat(Size(color_w, color_h), CV_8UC3, (void*)color_frame.get_data(), Mat::AUTO_STEP);

		//cv::cvtColor(rgb, rgb, CV_BGR2RGBA);
		//cv::flip(rgb, rgb, 1);
		//cv::flip(depth, depth, 1);
	}
	else
	{
		/*V_RGB = rgb_img;
		V_Depth = depth_img;

		rgb = rgb_img;
		depth = depth_img;*/
	}
}

void VideoCap::Update() {
	rs2::frameset frameset = pipe.wait_for_frames();

	rs2::frame color_frame = frameset.get_color_frame();//processed.first(align_to);
	rs2::frame depth_frame = frameset.get_depth_frame();
	//獲取寬高
	const int depth_w = depth_frame.as<rs2::video_frame>().get_width();
	const int depth_h = depth_frame.as<rs2::video_frame>().get_height();
	const int color_w = color_frame.as<rs2::video_frame>().get_width();
	const int color_h = color_frame.as<rs2::video_frame>().get_height();

	//創建OPENCV類型 並傳入數據
	V_Depth = cv::Mat(Size(depth_w, depth_h), CV_16U, (void*)depth_frame.get_data(), cv::Mat::AUTO_STEP);
	V_RGB = cv::Mat(Size(color_w, color_h), CV_8UC3, (void*)color_frame.get_data(), cv::Mat::AUTO_STEP);
}

/// Destructor
VideoCap::~VideoCap()
{
	if (Read_type == 0)
		pipe.stop();
	else
		ros::shutdown();
}


Mat VideoCap::get_rgb()
{
	return V_RGB;
}

Mat VideoCap::get_depth()
{
	return V_Depth;
}


