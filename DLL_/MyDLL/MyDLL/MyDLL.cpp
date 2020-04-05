// MyDLL.cpp : 定義 DLL 應用程式的匯出函式。
//

#include "stdafx.h"
#include "MyDLL.h"

namespace MyDLL 
{
	typedef void(*FuncPtr)(const char *);

	/// Pamrameters
	FuncPtr Debug;
	int count = 0;

	/// Init System
	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API set_type(int _type)
	{
		type = _type;
	}
	
	/// ROS
	//extern "C" ROSCallback UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ROS_D_Callback();
	extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Create_ROS(bool show_img)
	{
		ROS_ptr = new ROS(show_img);
		init_ros = true;
		return init_ros;
	}
	extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Run_ROS()
	{
		ROS_thread_ptr = new thread(&ROS::ROS_run, ROS_ptr);
		return true;
	}
	
	/// ORBSLAM
	extern "C" bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API init_Orb_SLAM()
	{
		if (setcb2)
		{
			SLAM_ptr = new Orb_SLAM(cb2, type);

			if (SLAM_ptr->init())
			{
				SLAM_thread_ptr = new thread(&Orb_SLAM::run_with_ROS, SLAM_ptr);
				init_orb_ = true;
			}
		}
		
		return false;
	}
	// Reset the slam system
	extern "C" void  UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API reset_slam()
	{
		SLAM_ptr->SLAM_system_ptr->Reset();
	}
	// Update
	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Update()
	{
		if (Start_calibration)
		{
			Mat rgb = cv::imread("rgb_init.jpg",CV_8UC3);
			Mat depth = cv::imread("depth_init.jpg", CV_32F);
			rgb.copyTo(RGB);
			depth.copyTo(Depth);

			count++;
			if (count == 10)
				Start_calibration = false;
		}
		else
		{
			ROS_ptr->Update(RGB, Depth);
		}

		///
		if (init_orb_)
		{
			SLAM_ptr->update(RGB, Depth);
		}
		keyframes = SLAM_ptr->map->GetAllKeyFrames();

		if (ROS_ptr->Rgb_status)
			cv::flip(RGB, RGB, 0);
		if (ROS_ptr->D_status)
			cv::flip(Depth, Depth, 0);
	};
	/// Robot arm calibration
	void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API arm_calibration()
	{
		reset_slam();
		count = 0;
		Start_calibration = true;
	}
	// Delete
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API DestroyWebCam()
	{
		SLAM_ptr->requestFinish();
		SLAM_thread_ptr->join();

		delete SLAM_thread_ptr;
	}
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API DestroyROS()
	{
		ROS_ptr->~ROS();
		ROS_thread_ptr->join();
		delete ROS_thread_ptr;
		delete ROS_ptr;
	}

	/// Function
	// Check Function
	extern "C" UNITY_INTERFACE_EXPORT bool  UNITY_INTERFACE_API check_kf_depth(int ID) {
		if (keyframes[ID]->img1.total() <= 0)
			return false;
		return true;
	}
	extern "C" UNITY_INTERFACE_EXPORT bool  UNITY_INTERFACE_API check_kf_rgb(int ID) {
		if (keyframes[ID]->img0.total() <= 0)
			return false;
		return true;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API check_kf_ID(int mnID) 
	{
		for (int i = 0; i < keyframes.size(); i++)
		{
			if (keyframes[i]->mnId == mnID)
				return i;
		}
		return -1;
	}
	
	// Set Function
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API set_Localozation() 
	{
		SLAM_ptr->SLAM_system_ptr->ActivateLocalizationMode();
	}
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API set_NLocalozation() 
	{
		SLAM_ptr->SLAM_system_ptr->DeactivateLocalizationMode();
	}
	
	
	// Get Function
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_kf_depth2(int ID) {

		cv::flip(keyframes[ID]->img1, kfDepth, 0);
		return kfDepth.data;

	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_kf_rgb2(int ID)
	{
		cv::cvtColor(keyframes[ID]->img0, kfRGB, CV_BGR2RGBA);
		cv::flip(kfRGB, kfRGB, 0);
		return kfRGB.data;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_ID(int ID)
	{
		return keyframes[ID]->mnId;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_MaxID(int ID)
	{
		return keyframes[ID]->nNextId;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_big_change()
	{
		return SLAM_ptr->map->GetLastBigChangeIdx();
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_size()
	{
		return keyframes.size();
	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imRGB_byte2(int &width, int &height)
	{
		width = RGB.size().width;
		height = RGB.size().height;
		return RGB.data;
	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imD_byte2(int &width, int &height)
	{
		width = Depth.size().width;
		height = Depth.size().height;
		return Depth.data;

	}
	extern "C" UNITY_INTERFACE_EXPORT int UNITY_INTERFACE_API GetTrackingState()
	{
		return SLAM_ptr->SLAM_system_ptr->GetTrackingState();
	}



	/// Useless
	uchar * get_imRGB_byte(int&width, int&height) {
		width = SLAM_ptr->imRGB.size().width;
		height = SLAM_ptr->imRGB.size().height;
		return matToBytes((SLAM_ptr->imRGB).clone());
	}
	uchar * get_imD_byte(int&width, int&height) {

		return matToBytes((SLAM_ptr->imD).clone());

	}
	void set_rgb(uchar* imageData, int width, int height, int stride)
	{

		if (NULL == imageData)
		{
			return;
		}
		cv::Mat opencvImage(height, width, CV_8UC3);
		memcpy(opencvImage.data, imageData, width*height * 3);

		cv::imshow("Get the Image", opencvImage);
		cv::waitKey(30);

	}
	void set_depth(uchar* imageData, int width, int height, int stride)
	{

		if (NULL == imageData)
		{
			return;
		}
		cv::Mat opencvImage(height, width, CV_16U);
		memcpy(opencvImage.data, imageData, (SLAM_ptr->imD.elemSize())*(SLAM_ptr->imD.total()));
		cv::imshow("Get the Image", opencvImage);

		cv::waitKey(30);

	}
	uchar * _stdcall get_rgb(int&width, int&height, int&step) {
		uchar * data = new uchar[(SLAM_ptr->imRGB.size().width)*(SLAM_ptr->imRGB.size().height) * 3];
		//memcpy(data, SLAM_ptr->imRGB.data, width*height);
		data = (SLAM_ptr->imRGB).clone().data;
		width = SLAM_ptr->imRGB.size().width;
		height = SLAM_ptr->imRGB.size().height;
		step = SLAM_ptr->imRGB.step;
		imshow("this", (SLAM_ptr->imRGB));
		return data;
	}
	uchar * _stdcall get_pos() {
		uchar * data = new uchar[4 * 4];
		data = (SLAM_ptr->get_pose()).clone().data;
		return data;
	}
	uchar * _stdcall get_depth(int&width, int&height, int&step) {

		imshow("this", (SLAM_ptr->imD));
		uchar * data = new uchar[(SLAM_ptr->imD.elemSize())*(SLAM_ptr->imD.total())];
		memcpy(data, (SLAM_ptr->imD).clone().data, (SLAM_ptr->imD.elemSize())*(SLAM_ptr->imD.total()));

		width = SLAM_ptr->imD.size().width;
		height = SLAM_ptr->imD.size().height;
		step = SLAM_ptr->imD.step;
		return data;
	}
	uchar * get_pose_byte() {
		return matToBytes((SLAM_ptr->get_pose()).clone());
	}
	Mat get_imRGB() 
	{
		return (SLAM_ptr->imRGB).clone();
	}
	Mat get_imD()
	{
		return (SLAM_ptr->imD).clone();
	}
	Mat get_pose()
	{
		return (SLAM_ptr->get_pose()).clone();
	}


	//
	static std::string s_UnityStreamingAssetsPath;
	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetUnityStreamingAssetsPath(const char* path)
	{
		s_UnityStreamingAssetsPath = path;
	}
	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetDebugFunction(FuncPtr fp) { Debug = fp; }

}