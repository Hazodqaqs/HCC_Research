
// MyDLL.cpp : 定義 DLL 應用程式的匯出函式。
//

#include "stdafx.h"
#include "MyDLL.h"
#include "VideoCap.h"

namespace MyDLL {
	void call_update_Mat(uchar* iRGB, uchar* iDepth) {
		if (NULL == iRGB)
		{
			return;
		}
		cv::Mat opencvImage1(480, 640, CV_8UC3);
		memcpy(opencvImage1.data, iRGB, 640 * 480 * 3);

		cv::imshow("Get the Image1", opencvImage1);

		if (NULL == iDepth)
		{
			return;
		}
		cv::Mat opencvImage(480, 640, CV_16U);
		memcpy(opencvImage.data, iDepth, 640 * 480 * 2);
		cv::imshow("Get the Image", opencvImage);


	}
	void call_update() {

		SLAM_ptr->update2();

	}
	typedef void(*FuncPtr)(const char *);
	FuncPtr Debug;
	VideoCap* video_cap_ptr;
	bool cam_init = false;
	extern "C" void  UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API  init_Cam()
	{
		video_cap_ptr = new VideoCap();
		cam_init = true;
		Debug("init_Cam");

	};

	struct MyVertex {
		float x, y, z;
	};
	MyVertex* vertexs;

	extern "C" void  UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API  destroy_Cam()
	{
		delete video_cap_ptr;
		cam_init = false;
		Debug("destroy_Cam");
	};
	bool  init_Orb_SLAM()
	{
		if (setcb2)
		{
			SLAM_ptr = new Orb_SLAM(cb2, type);

			if (SLAM_ptr->init())
			{
				init_Cam();
				SLAM_thread_ptr = new thread(&Orb_SLAM::run, SLAM_ptr);
				//SLAM_thread_ptr2 = new thread(&Orb_SLAM::Run, SLAM_ptr);
				init_orb_ = true;
				if(cam_init)
					return true;
			}
		}
		
		return false;
	}

	void  set_type(int _type) {
		type = _type;
	}
	//	//reset the slam system
	void reset_slam()
	{
		SLAM_ptr->SLAM_system_ptr->Reset();
	}
	
	//return false when depth is null
	extern "C" UNITY_INTERFACE_EXPORT bool  UNITY_INTERFACE_API check_kf_depth(int ID) {
		if (keyframes[ID]->img1.total() <= 0)
			return false;
		return true;
	}

	//return false when rgb is null
	extern "C" UNITY_INTERFACE_EXPORT bool  UNITY_INTERFACE_API check_kf_rgb(int ID) {
		if (keyframes[ID]->img0.total() <= 0)
			return false;
		return true;
	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_kf_depth2(int ID) {

		cv::flip(keyframes[ID]->img1, kfDepth, 0);
		//kfDepth.convertTo(kfDepth, CV_16U, 1000);

		return kfDepth.data;

	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_kf_rgb2(int ID) {
		cv::cvtColor(keyframes[ID]->img0, kfRGB, CV_BGR2RGBA);
		cv::flip(kfRGB, kfRGB, 0);
		return kfRGB.data;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_ID(int ID) {
		return keyframes[ID]->mnId;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_MaxID(int ID) {
		return keyframes[ID]->nNextId;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API check_kf_ID(int mnID) {
		for (int i = 0; i < keyframes.size(); i++)
		{
			if (keyframes[i]->mnId == mnID)
				return i;
		}
		return -1;
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_kf_size() {
		return keyframes.size();
		

	}
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API set_Localozation() {
		SLAM_ptr->SLAM_system_ptr->ActivateLocalizationMode();

	}
	extern "C" UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API set_NLocalozation() {
		SLAM_ptr->SLAM_system_ptr->DeactivateLocalizationMode();
	}
	extern "C" UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API get_big_change() {
		
		return SLAM_ptr->map->GetLastBigChangeIdx();

	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imRGB_byte2(int &width, int &height) {

		width = RGB.size().width;
		height = RGB.size().height;
		return RGB.data;

	}
	extern "C" UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imD_byte2(int &width, int &height) {

		width = Depth.size().width;
		height = Depth.size().height;
		return Depth.data;

	}

	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Update() {
		video_cap_ptr->Update(RGB, Depth);
		if (init_orb_)
			SLAM_ptr->update(RGB, Depth);

		keyframes = SLAM_ptr->map->GetAllKeyFrames();
		cv::cvtColor(RGB, RGB, CV_BGR2RGBA);
		cv::flip(RGB, RGB, 0);
		cv::flip(Depth, Depth, 0);
		Depth.convertTo(Depth, CV_32F, 0.001);
	};

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
	Mat get_imRGB() {
		return (SLAM_ptr->imRGB).clone();
	}
	//
	Mat get_imD()
	{
		return (SLAM_ptr->imD).clone();
	}

	Mat get_pose()
	{
		return (SLAM_ptr->get_pose()).clone();
	}

	int GetTrackingState()
	{
		return SLAM_ptr->SLAM_system_ptr->GetTrackingState();
	}
	//
	void DestroyWebCam()
	{
		SLAM_ptr->requestFinish();
		SLAM_thread_ptr->join();

		//SLAM_thread_ptr2->join();
		delete video_cap_ptr;
		delete SLAM_thread_ptr;
		delete SLAM_thread_ptr2;
	}



   /// 

	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetDebugFunction(FuncPtr fp) { Debug = fp; }

	static std::string s_UnityStreamingAssetsPath;
	extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetUnityStreamingAssetsPath(const char* path)
	{
		s_UnityStreamingAssetsPath = path;
	}
}