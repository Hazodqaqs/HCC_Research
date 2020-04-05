#include "stdafx.h"
#include "ROS.h"

using namespace cv;

/// Parameters
cv::Mat rgb_img, depth_img;
bool rgb_status = false, d_status = false;
//OPption
bool show_img = false;


/// Depth transform
Dep_Instrinct D_intrin;
Color_Instrinct C_instrin;
extrinct Extrin;
void deproject_pixel_to_point(float point[3], Dep_Instrinct intrin, float pixel[2], float depth)
{
	float x = (pixel[0] - intrin.ppx) / intrin.fx;
	float y = (pixel[1] - intrin.ppy) / intrin.fy;

	float r2 = x * x + y * y;
	float f = 1 + intrin.coeffs[0] * r2 + intrin.coeffs[1] * r2*r2 + intrin.coeffs[4] * r2*r2*r2;
	float ux = x * f + 2 * intrin.coeffs[2] * x*y + intrin.coeffs[3] * (r2 + 2 * x*x);
	float uy = y * f + 2 * intrin.coeffs[3] * x*y + intrin.coeffs[2] * (r2 + 2 * y*y);
	x = ux;
	y = uy;

	point[0] = depth * x;
	point[1] = depth * y;
	point[2] = depth;
}
void transform_point_to_point(float to_point[3], extrinct extrin, const float from_point[3])
{
	to_point[0] = extrin.rotation[0] * from_point[0] + extrin.rotation[3] * from_point[1] + extrin.rotation[6] * from_point[2] + extrin.translation[0];
	to_point[1] = extrin.rotation[1] * from_point[0] + extrin.rotation[4] * from_point[1] + extrin.rotation[7] * from_point[2] + extrin.translation[1];
	to_point[2] = extrin.rotation[2] * from_point[0] + extrin.rotation[5] * from_point[1] + extrin.rotation[8] * from_point[2] + extrin.translation[2];
}
void project_point_to_pixel(float pixel[2], Color_Instrinct intrin, const float point[3])
{
	float x = point[0] / point[2], y = point[1] / point[2];

	float r2 = x * x + y * y;
	float f = 1 + intrin.coeffs[0] * r2 + intrin.coeffs[1] * r2*r2 + intrin.coeffs[4] * r2*r2*r2;
	x *= f;
	y *= f;
	float dx = x + 2 * intrin.coeffs[2] * x*y + intrin.coeffs[3] * (r2 + 2 * x*x);
	float dy = y + 2 * intrin.coeffs[3] * x*y + intrin.coeffs[2] * (r2 + 2 * y*y);
	x = dx;
	y = dy;


	pixel[0] = x * intrin.fx + intrin.ppx;
	pixel[1] = y * intrin.fy + intrin.ppy;
}
//Mat align_Depth2Color(Mat depth, Mat color) {
//	//平面點定義
//	float pd_uv[2], pc_uv[2];
//	//空間點定義
//	float Pdc3[3], Pcc3[3];
//
//	//獲取深度像素與現實單位比例（D435默認1毫米）
//	float depth_scale = 0.00100;
//	int y = 0, x = 0;
//	float max_distance = 2;
//
//	//初始化結果
//	Mat result = Mat::zeros(color.rows, color.cols, CV_8UC3);
//	//對深度圖像遍歷
//	for (int row = 0; row < depth.rows; row++) {
//		for (int col = 0; col < depth.cols; col++) {
//			//將當前的(x,y)放入數組pd_uv，表示當前深度圖的點
//			pd_uv[0] = col;
//			pd_uv[1] = row;
//			//取當前點對應的深度值
//			uint16_t depth_value = depth.at<uint16_t>(row, col);
//			//換算到米
//			float depth_m = depth_value * depth_scale;
//			//std::cout << depth_value << std::endl;
//
//			//將深度圖的像素點根據內參轉換到深度攝像頭座標系下的三維點
//			//rs2_deproject_pixel_to_point(Pdc3, &intrinDepth, pd_uv, depth_m);
//			deproject_pixel_to_point(Pdc3, D_intrin, pd_uv, depth_m);
//
//			//將深度攝像頭座標系的三維點轉化到彩色攝像頭座標系下
//			//rs2_transform_point_to_point(Pcc3, &extrinDepth2Color, Pdc3);
//			transform_point_to_point(Pcc3, Extrin, Pdc3);
//
//			//將彩色攝像頭座標系下的深度三維點映射到二維平面上
//			project_point_to_pixel(pc_uv, C_instrin, Pcc3);
//
//			//取得映射後的（u,v)
//			x = (int)pc_uv[0];
//			y = (int)pc_uv[1];
//
//			//最值限定
//			x = x < 0 ? 0 : x;
//			x = x > depth.cols - max_distance ? depth.cols - max_distance : x;
//			y = y < 0 ? 0 : y;
//			y = y > depth.rows - max_distance ? depth.rows - max_distance : y;
//
//			//將成功映射的點用彩色圖對應點的RGB數據覆蓋
//			for (int k = 0; k < 3; k++) {
//				//這裏設置了只顯示1米距離內的東西
//				if (depth_m < max_distance)
//					result.at<cv::Vec3b>(y, x)[k] =color.at<cv::Vec3b>(y, x)[k];
//			}
//		}
//	}
//	return result;
//}
Mat align_Depth2Color(Mat depth) {
	//平面點定義
	float pd_uv[2], pc_uv[2];
	//空間點定義
	float Pdc3[3], Pcc3[3];

	//獲取深度像素與現實單位比例（D435默認1毫米）
	float depth_scale = 0.00100;
	int y = 0, x = 0;
	float max_distance = 2;

	//初始化結果
	Mat result = Mat::zeros(depth.rows, depth.cols, CV_16U);
	//對深度圖像遍歷
	for (int row = 0; row < depth.rows; row++) {
		for (int col = 0; col < depth.cols; col++) {
			//將當前的(x,y)放入數組pd_uv，表示當前深度圖的點
			pd_uv[0] = col;
			pd_uv[1] = row;
			//取當前點對應的深度值
			uint16_t depth_value = depth.at<uint16_t>(row, col);
			//換算到米
			float depth_m = depth_value * depth_scale;
			//std::cout << depth_value << std::endl;

			//將深度圖的像素點根據內參轉換到深度攝像頭座標系下的三維點
			deproject_pixel_to_point(Pdc3, D_intrin, pd_uv, depth_m);

			//將深度攝像頭座標系的三維點轉化到彩色攝像頭座標系下
			transform_point_to_point(Pcc3, Extrin, Pdc3);

			//將彩色攝像頭座標系下的深度三維點映射到二維平面上
			project_point_to_pixel(pc_uv, C_instrin, Pcc3);

			//取得映射後的（u,v)
			x = (int)pc_uv[0];
			y = (int)pc_uv[1];

			//最值限定
			x = x < 0 ? 0 : x;
			x = x > depth.cols - 1 ? depth.cols - 1 : x;
			y = y < 0 ? 0 : y;
			y = y > depth.rows - 1 ? depth.rows - 1 : y;

			/// 寫入深度值
			if (depth_m < max_distance)
				result.at<uint16_t>(y, x) = depth_value;
			else
				result.at<uint16_t>(y, x) = 0.000;
		}
	}
	return result;
}

///
// ROS Callback
void ROS_RGB_Callback(const sensor_msgs::ImageConstPtr& msg)
{
	cv_bridge::CvImageConstPtr cv_ptr;
	try
	{
		cv_ptr = cv_bridge::toCvCopy(msg, sensor_msgs::image_encodings::TYPE_8UC3);
		cv::cvtColor(cv_ptr->image, rgb_img, CV_BGR2RGBA);
		rgb_status = true;

		cv::waitKey(30);

		// Option
		if (show_img)
			cv::imshow("view_RGB", rgb_img);
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
		cv::Mat d_img = cv_ptr->image;
		d_img.convertTo(d_img, CV_16U);

		depth_img = align_Depth2Color(d_img);
		depth_img.convertTo(depth_img, CV_32F, 0.001);
		d_status = true;

		cv::waitKey(30);

		// Option
		if (show_img)
			cv::imshow("view_Depth", depth_img);
	}
	catch (cv_bridge::Exception& e)
	{
		ROS_ERROR("Could not convert from '%s' to 'TYPE_16UC1'.", msg->encoding.c_str());
	}
}

/// Constructor
// ROS initialize
ROS::ROS(bool Show_img)
{
	show_img = Show_img;

}
void ROS::ROS_run()
{
	// Option
	if (show_img)
	{
		cv::namedWindow("view_RGB");
		cv::startWindowThread();
		cv::namedWindow("view_Depth");
		cv::startWindowThread();
	}

	// ROS
	ROS_init("http://raspberry:11311", "172.25.171.215");//172.25.250.214
}
void ROS::ROS_init(string master, string Ip)
{
	/// Initialize ROS node
	std::cout << "Start Node(Get_Image)..." << std::endl;

	std::map<std::string, std::string> params;
	params.insert(std::pair<std::string, std::string>("__master", master));
	params.insert(std::pair<std::string, std::string>("__ip", Ip));

	std::cout << "Init Node(Get_Image)..." << std::endl;

	ros::init(params, "Get_Image");
	NodeHandle image_node;
	Subscriber RGB_image_node_sub;
	Subscriber Depth_image_node_sub;
	RGB_image_node_sub = image_node.subscribe<sensor_msgs::Image>("/camera/color/image_raw", 1000, ROS_RGB_Callback);
	Depth_image_node_sub = image_node.subscribe<sensor_msgs::Image>("/camera/depth/image_rect_raw", 1000, ROS_Depth_Callback);
	ros::spin();
}

// Destructor
ROS::~ROS()
{
	ros::shutdown();
}


/// Connect
bool ROS::ROS_connect()
{
	if (!rgb_status && !d_status)
		Ros_status = true;
	return Ros_status;
}

/// Update
void ROS::Update(cv::Mat& rgb, cv::Mat& depth)
{
	D_status = false;
	Rgb_status = false;
	if (d_status && rgb_status)
	{
		rgb_img.copyTo(V_RGB);
		depth_img.copyTo(V_Depth);
		
		rgb_status = false;
		d_status = false;

		D_status = true;
		Rgb_status = true;
	}

	rgb = V_RGB;
	depth = V_Depth;
}
void ROS::ros_request_finish()
{
	ros::shutdown();
}


/// Get image
Mat ROS::get_rgb()
{
	return V_RGB;
}
Mat ROS::get_depth()
{
	return V_Depth;
}


