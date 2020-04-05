// Tensorflow_DLL_O.cpp : 定義 DLL 的匯出函式。
//

#include "pch.h"
#include "framework.h"
#include "Tensorflow_DLL.h"


namespace Tensorflow_DLL
{
	/// Get list patameters 
		/// Use list_elements(begin(data),columns, rows)  for 2D matrix
		/// Use list_elements(begin(data),rows) for 1D matrix
	template<typename Iter>
	double list_elements(Iter start, int r)
	{
		for (int x = 0; x < r; x++)
			*start++;
		return (double)*start;
	}
	template<typename Iter>
	double list_elements(Iter start, int c, int r)
	{
		int x = 0;
		for (; x < c; x++)
			*start++;
		list<double> start_i_list = *start;
		return list_elements(begin(start_i_list), r);
	}
	void listToVector_Float(PyObject* incoming, list<double> &data)
	{

		if (PyList_Check(incoming))
		{
			int r = PyList_Size(incoming);
			for (int i = 0; i < PyList_Size(incoming); i++)
			{
				PyObject *value = PyList_GetItem(incoming, i);

				double *tmp = new double(1);
				tmp[0] = PyFloat_AsDouble(value);
				data.push_back(*tmp);
			}
		}
		else
		{
			throw logic_error("Passed PyObject pointer was not a list or tuple!");
		}
	}
	void listandlistToVector_Float(PyObject* incoming, list< list<double>> &data)
	{
		if (PyList_Check(incoming))
		{
			for (int i = 0; i < PyList_Size(incoming); i++)
			{
				PyObject *value = PyList_GetItem(incoming, i);

				list<double> *tmp = new list<double>(1);
				listToVector_Float(value, tmp[0]);
				data.push_back(*tmp);

			}

			//cout << "Get data: " << endl;
			//for (int i = 0; i < data.size(); i++)
			//{
			//	for (int j = 1; j < 5; j++)
			//	{
			//		std::cout << list_elements(begin(data), i, j) << " ";
			//	}
			//	cout << endl;
			//}

		}
		else
		{
			throw logic_error("Passed PyObject pointer was not a list or tuple!");
		}


	}
	
	extern "C"
	{
		/// Other Function
		wchar_t *GetWC(char *c)
		{
			const size_t cSize = strlen(c) + 1;
			wchar_t* wc = new wchar_t[cSize];
			mbstowcs(wc, c, cSize);

			return wc;
		}
		// Make Input (First 4 is input, last 4 is output)
		void Add_data_to_array(double* Input_list)
		{
			PyObject *x = PyList_New(4);
			PyObject *y = PyList_New(4);
			for (int Index_i = 0; Index_i < 8; Index_i++)
			{
				if (Index_i < 4)
					PyList_SetItem(x, Index_i, PyFloat_FromDouble(Input_list[Index_i]));
				else
					PyList_SetItem(y, Index_i - 4, PyFloat_FromDouble(Input_list[Index_i]));
			}
			PyList_Append(data_x, x);
			PyList_Append(data_y, y);

		}
		void Set_input(double** data_list, int count)
		{
			double *data = (double*)malloc(sizeof(double) * 8);
			for (int i = 0; i < count; i++)
			{
				data[0] = data_list[i][0];
				data[1] = data_list[i][1];
				data[2] = data_list[i][2];
				data[3] = data_list[i][3];
				data[4] = data_list[i][4];
				data[5] = data_list[i][5];
				data[6] = data_list[i][6];
				data[7] = data_list[i][7];
				Add_data_to_array(data);
			}
		}
		// 
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Get_default_data()
		{
			// default_data
			PyObject* pv_default_data_x = PyObject_GetAttrString(pModule, "default_data_x");
			if (!pv_default_data_x || !PyCallable_Check(pv_default_data_x))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (default_data_x)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (default_data_x) succeed." << endl;

			PyObject* pv_default_data_y = PyObject_GetAttrString(pModule, "default_data_y");
			if (!pv_default_data_y || !PyCallable_Check(pv_default_data_y))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (default_data_y)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (default_data_y) succeed." << endl;

			PyObject* default_args = NULL;

			// 預設資料
			default_data_x = PyObject_CallObject(pv_default_data_x, default_args);
			default_data_y = PyObject_CallObject(pv_default_data_y, default_args);
		}
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Print_list()
		{
			cout << "Print_list..." << endl;
			PyObject* pv_Print_list = PyObject_GetAttrString(pModule, "Print_list");// init_model
			if (!pv_Print_list || !PyCallable_Check(pv_Print_list))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (Print_list)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (Print_list) succeed." << endl;

			PyObject *args = PyTuple_New(2);//定义一个Tuple对象，Tuple对象的长度与Python函数参数个数一直，上面Python参数个数为1，所以这里给的长度为1
			PyTuple_SetItem(args, 0, data_x);
			PyTuple_SetItem(args, 1, data_y);
			PyObject_CallObject(pv_Print_list, args);

		}

		/// Main Function 
		// Initialize
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Initialize_ALL()
		{
			data_x = PyList_New(0);
			data_y = PyList_New(0);

			// Initial python
			cout << "[INFO] Initialize python!" << endl;

			char *python_home = (char *)"D:/Anaconda/envs/tf_gpu";
			wchar_t* python_home_wc = GetWC(python_home);

			Py_SetPythonHome(python_home_wc);
			Py_Initialize();

			PyEval_InitThreads();
			import_array();

			// 將Python工作路徑切換到待呼叫模組所在目錄，一定要保證路徑名的正確性
			string path = "E:/R07522616_HCC/ORBSLAM Program/Tensorflow_test/x64/Release";
			string chdir_cmd = string("sys.path.append(\"") + path + "\")";
			const char* cstr_cmd = chdir_cmd.c_str();
			PyRun_SimpleString("import sys");
			PyRun_SimpleString("import os");
			PyRun_SimpleString(cstr_cmd);
			PyRun_SimpleString("sys.path.append(os.getcwd())");

			// Import library
			PyRun_SimpleString("import tensorflow as tf");
			PyRun_SimpleString("from keras.models import Sequential, Model");
			PyRun_SimpleString("from keras.layers import Dense, Activation, Input");
			PyRun_SimpleString("from keras.optimizers import SGD, Adam");
			PyRun_SimpleString("from keras.layers import concatenate, add");
			PyRun_SimpleString("from keras.layers.core import Lambda");
			PyRun_SimpleString("from keras import backend as K");
			PyRun_SimpleString("import csv");
			PyRun_SimpleString("import time");

			// 創建 tensflow GPU 計算核心
			PyRun_SimpleString("config = tf.ConfigProto()");
			PyRun_SimpleString("config.gpu_options.allow_growth = True ");
			PyRun_SimpleString("config.log_device_placement = True  ");
			PyRun_SimpleString("sess = tf.Session(config = config)");
			//PyRun_SimpleString("set_session(sess) ");

			/// 載入模組
			pModule = PyImport_Import(PyUnicode_FromString("My_function"));
			if (!pModule) // 載入模組失敗
			{
				cout << "[ERROR] Python get module failed." << endl;
				return 0;
			}
			cout << "[INFO] Python get module succeed." << endl;
		}
		// Load weight
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Model_load_weight()
		{
			cout << "loading weight..." << endl;
			PyObject* pv_model_with_pretrained_weight = PyObject_GetAttrString(pModule, "model_with_pretrained_weight");// init_model
			if (!pv_model_with_pretrained_weight || !PyCallable_Check(pv_model_with_pretrained_weight))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (model_with_pretrained_weight)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (model_with_pretrained_weight) succeed." << endl;

			//預測MODEL
			PyObject* args = PyTuple_New(1);
			PyObject* dir = Py_BuildValue("s", "model.pb");
			PyTuple_SetItem(args, 0, dir);
			model = PyObject_CallObject(pv_model_with_pretrained_weight, args);

		}
		// Train
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Train_data(int epoch, int count)
		{
			/// 載入函式
			// Train_model
			PyObject* pv_train = PyObject_GetAttrString(pModule, "Train_model");// init_model
			if (!pv_train || !PyCallable_Check(pv_train))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (Train_model)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (Train_model) succeed." << endl;

			///
			// 設定引數
			PyObject* args = PyTuple_New(3);   // 2個引數
			PyObject* epochs = Py_BuildValue("i", epoch);
			if (count == 0)
			{
				PyTuple_SetItem(args, 0, default_data_x);
				PyTuple_SetItem(args, 1, default_data_y);
				PyTuple_SetItem(args, 2, epochs);
			}
			else
			{
				PyTuple_SetItem(args, 0, data_x);
				PyTuple_SetItem(args, 1, data_y);
				PyTuple_SetItem(args, 2, epochs);
			}

			model = PyObject_CallObject(pv_train, args);
		}
		// Predict (要再將data_y轉成float)
		int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Predict_model()
		{
			cout << "loading weight..." << endl;
			PyObject* pv_model_with_pretrained_weight = PyObject_GetAttrString(pModule, "model_with_pretrained_weight");// init_model
			if (!pv_model_with_pretrained_weight || !PyCallable_Check(pv_model_with_pretrained_weight))  // 驗證是否載入成功
			{
				cout << "[ERROR] Can't find funftion (model_with_pretrained_weight)" << endl;
				return 0;
			}
			cout << "[INFO] Get function (model_with_pretrained_weight) succeed." << endl;

			//預測MODEL
			PyObject* args = PyTuple_New(1);
			PyObject* dir = Py_BuildValue("s", "model.pb");
			PyTuple_SetItem(args, 0, dir);
			model = PyObject_CallObject(pv_model_with_pretrained_weight, args);

			//
			if (model != NULL)
			{
				PyObject* pv_predict = PyObject_GetAttrString(pModule, "model_predict");// init_model
				if (!pv_predict || !PyCallable_Check(pv_predict))  // 驗證是否載入成功
				{
					cout << "[ERROR] Can't find funftion (model_predict)" << endl;
					return 0;
				}
				cout << "[INFO] Get function (model_predict) succeed." << endl;

				// 預測MODEL
				PyObject* args = PyTuple_New(2);
				PyTuple_SetItem(args, 0, model);
				PyTuple_SetItem(args, 1, data_x); // default_data_x data_x
				PyObject* predict_anw = PyObject_CallObject(pv_predict, args);

				// Convert python data to c++
				if (PyList_Check(predict_anw))
				{
					PyObject *value = PyList_GetItem(predict_anw, 0);
					if (PyList_Check(value))
					{
						data_2D.clear();
						listandlistToVector_Float(predict_anw, data_2D);
					}
					else
					{
						data_1D.clear();
						listToVector_Float(predict_anw, data_1D);
					}
				}

			}
			else
				cout << "還沒建立Model" << endl;
		}
		// Reset data
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Reset_data()
		{
			data_x = PyList_New(0);
			data_y = PyList_New(0);
			data_2D.clear();
			data_1D.clear();
			cout << "Reset data succesfully!" << endl;
		}

		/// Get Function
		double UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Get_pridict(int columns, int rows)
		{
			if (!data_1D.empty())
			{
				cout << "only 1D..." << endl;
				return list_elements(begin(data_1D), rows + 1);
			}
			else if (!data_2D.empty())
				return list_elements(begin(data_2D), columns, rows + 1);
			else
			{
				cout << "No predict data..." << endl;
				return 0;
			}
		}


		/// Main 
		void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API __stdcall Set_train_data(double** data, int count)
		{
			Set_input(data, count);
		}
	}
}
