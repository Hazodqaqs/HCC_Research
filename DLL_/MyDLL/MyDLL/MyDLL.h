#pragma once
#ifndef MYDLL_H 
#define MYDLL_H

#if _MSC_VER
	#define UNITY_WIN 1
#endif

#if UNITY_WIN
	#define SUPPORT_D3D11 1 // comment this out if you don't have D3D11 header/library files
	#define SUPPORT_OPENGL 1
#endif

#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"
#include <math.h>
#include <stdio.h>
#include <vector>
#include <string>
#include <thread>
#include "Orb_SLAM.h"
#include "ROS.h"
typedef void(__stdcall * MyCallback)(int);
typedef void(__stdcall * ROSCallback)(const sensor_msgs::ImageConstPtr& msg);
typedef unsigned char byte;

namespace MyDLL 
{
	extern "C" 
	{
		/// Common Parameters
		bool setcb1 = false;
		bool setcb2 = false;
		bool init_orb_ = false;
		bool init_ros = false;

		// Vertex
		struct MyVertex
		{
			float x, y, z;
		};
		MyVertex* vertexs;

		//ROS
		ROS * ROS_ptr;
	    UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API Create_ROS(bool show_img);
		UNITY_INTERFACE_EXPORT bool UNITY_INTERFACE_API Run_ROS();


		//ORBSLAM
		MyCallback cb2;
		Orb_SLAM* SLAM_ptr;
		std::thread* SLAM_thread_ptr;
		std::thread* ROS_thread_ptr;
		vector<ORB_SLAM2::KeyFrame *> keyframes;

		/// Tramsform Function
		uchar * matToBytes(Mat image)
		{
			int size = image.total() * image.elemSize();

			uchar * bytes = new uchar[size];  // you will have to delete[] that later

			std::memcpy(bytes, image.data, size * sizeof(uchar));
			return bytes;
		}
		Mat bytesToMat(byte * bytes, int width, int height)
		{

			Mat image = Mat(height, width, CV_8UC3, bytes).clone(); // make a copy

			return image;

		}
		
		/// ORBSLAM2
		Mat RGB, Depth;
		Mat kfRGB, kfDepth;
		double kfpose[16];
		UNITY_INTERFACE_EXPORT double* get_kf_pose(int ID) 
		{
			Mat mTcw;
			mTcw = keyframes[ID]->GetPose();
			if (!mTcw.empty())
			{
				//std::stringstream sstrMat;
				//sstrMat << mTcw;
				//DebugInUnity(sstrMat.str());

				cv::Mat Rcw(3, 3, CV_32F);
				cv::Mat tcw(3, 1, CV_32F);

				Rcw = mTcw.rowRange(0, 3).colRange(0, 3);
				tcw = mTcw.rowRange(0, 3).col(3);

				kfpose[0] = Rcw.at<float>(0, 0);
				kfpose[1] = Rcw.at<float>(1, 0);
				kfpose[2] = Rcw.at<float>(2, 0);
				kfpose[3] = 0.0;

				kfpose[4] = Rcw.at<float>(0, 1);
				kfpose[5] = Rcw.at<float>(1, 1);
				kfpose[6] = Rcw.at<float>(2, 1);
				kfpose[7] = 0.0;

				kfpose[8] = Rcw.at<float>(0, 2);
				kfpose[9] = Rcw.at<float>(1, 2);
				kfpose[10] = Rcw.at<float>(2, 2);
				kfpose[11] = 0.0;

				kfpose[12] = tcw.at<float>(0);
				kfpose[13] = tcw.at<float>(1);
				kfpose[14] = tcw.at<float>(2);
				kfpose[15] = 1.0;

			}
			return kfpose;
		}
		UNITY_INTERFACE_EXPORT double* get_modelview_matrix()
		{
			return SLAM_ptr->get_modelview_matrix();
		}
		UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API call_update_Mat(uchar* iRGB, uchar* iDepth);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imRGB_byte(int &width, int &height);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_imD_byte(int &width, int &height);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_pose_byte();
		UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API set_rgb(uchar* imageData, int width, int height, int stride);
		UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API set_depth(uchar* imageData, int width, int height, int stride);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_rgb(int&width, int&height, int&step);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_depth(int&width, int&height, int&step);
		UNITY_INTERFACE_EXPORT uchar *  UNITY_INTERFACE_API get_pos();
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API set_type(int _type);
		UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API SetCallBack(MyCallback Y)
		{
			cb2 = Y;
			setcb2 = true;
		};
		

		/// Initial
		UNITY_INTERFACE_EXPORT bool  UNITY_INTERFACE_API init_Orb_SLAM();
		///reset the slam system
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API reset_slam();
		/// Update
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API call_update();
		/// Robot arm calibration
		bool Start_calibration = false;
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API arm_calibration();

		/// Get Function
		UNITY_INTERFACE_EXPORT Mat  UNITY_INTERFACE_API get_imRGB();
		UNITY_INTERFACE_EXPORT Mat  UNITY_INTERFACE_API get_imD();
		UNITY_INTERFACE_EXPORT Mat  UNITY_INTERFACE_API get_pose();
		UNITY_INTERFACE_EXPORT int  UNITY_INTERFACE_API GetTrackingState();
		
		/// delete ROS
		UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API  DestroyWebCam();
		UNITY_INTERFACE_EXPORT void  UNITY_INTERFACE_API DestroyROS();

		/// 創建數據管道
		int type = 0;
	}
}
#endif // MYDLL_H 
