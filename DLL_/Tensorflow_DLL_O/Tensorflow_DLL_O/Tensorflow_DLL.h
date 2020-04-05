#pragma once
#ifndef TENSORFLOWDLLO_H 
#define TENSORFLOWDLLO_H

#if _MSC_VER
#define UNITY_WIN 1
#endif

#if UNITY_WIN
#define SUPPORT_D3D11 1 // comment this out if you don't have D3D11 header/library files
#define SUPPORT_OPENGL 1
#endif

// Unity
#include "Unity/IUnityInterface.h"
#include "Unity/IUnityGraphics.h"
#include <d3d11.h>
#include "Unity/IUnityGraphicsD3D11.h"
#include <math.h>
#include <stdio.h>
#include <vector>
#include <string>
#include <thread>

/// 
#include <numpy/arrayobject.h>
#include <list>
#include <string>
#include <functional>

/// Tensorflow
#include <iostream>
#include <Python.h>
#include <windows.h>
#include "opencv2/opencv.hpp"
#include <numpy/arrayobject.h>

#include<opencv2/opencv.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/core/core.hpp>
#include <filesystem>

#pragma comment( lib, "opencv_world344.lib")
#pragma comment( lib, "opencv_world344d.lib")

#pragma warning(disable  : 4996)

///
using namespace std;

// 這個類別是從 dll 匯出的
namespace Tensorflow_DLL
{
	extern "C"
	{
		/// Parqameters
		PyObject* pModule = NULL;
		PyObject* model = NULL;
		PyObject* default_data_x = NULL;
		PyObject* default_data_y = NULL;
		PyObject* data_x = NULL;
		PyObject* data_y = NULL;
		list<list<double>> data_2D;
		list<double> data_1D;

		/// Transform Function
	    wchar_t *GetWC(char *c);
		// Make Input (First 4 is input, last 4 is output)
		void Add_data_to_array(double* Input_list);
		void Set_input(double** data_list, int count);
		//
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Get_default_data();
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Print_list();

		/// Main Function 
		// Initialize
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Initialize_ALL();
		// Load weight
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Model_load_weight();
		// Train
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Train_data(int epoch, int count);
		// Predict
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Predict_model();
		// Reset
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Reset_data();

		/// Get Function
		double UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Get_pridict(int columns, int rows);

		/// Set_train_data
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API __stdcall Set_train_data(double** data, int count);
	}
}


#endif