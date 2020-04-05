
#pragma warning( disable : 4996 )
#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <mutex>
#include <thread>
#include <condition_variable>
#include <iostream>
#include <chrono>

#include <librealsense2/rs.hpp>
#include <System.h>

//#define OPENCV_VIDEO_W 480
//#define OPENCV_VIDEO_H 540

//#define DEBUG_LOG

using namespace cv;
using namespace std;

typedef void(__stdcall * MyCallback)(int);
extern "C"
{
	class Orb_SLAM {
	public:
		MyCallback cbCpp;
		Orb_SLAM(MyCallback cb, const int input_type = 0);
		int input_t = 0;
		bool new_input_RGB = false;
		bool new_input_D = false;
		~Orb_SLAM();
		vector<float> vTimesTrack;
		//SLAM system
		ORB_SLAM2::System* SLAM_system_ptr;
		double tframe = 0.0f;
		ORB_SLAM2::Map* map;
		vector<ORB_SLAM2::KeyFrame *> keyframes;
		//model view matrix
		cv::Mat mTcw;
		double mModelview_matrix[16];
		cv::Mat imRGB, imD;
		//mutex and locks
		mutex mMutex;
		mutex mMutexFinish;
		bool mbFinishRequested;
		bool mbFinished;

		uint16_t lastKeyframeSize = 0;
		void Run();
		void generateMap();
		bool shutDownFlag = false;
		condition_variable keyFrameUpdated;
		mutex shutDownMutex;
		mutex keyframeMutex;
		//init function
		bool init();
		void update2();
		void update(Mat& rgb, Mat& depth);
		void run();
		void requestFinish();
		bool checkFinish();
		void setFinish();
		bool isFinished();
		double M_arr[16];
		ORB_SLAM2::KeyFrame *get_keyFrameByID(int ID);
		cv::Mat get_keyFrameByID_RGB(int ID);
		cv::Mat get_keyFrameByID_Depth(int ID);
		////return the 4*4 gl model view matrix
		cv::Mat get_imRGB();

		////return the domain plane mean and normal in the world coord
		cv::Mat get_imD();
		cv::Mat get_pose();
		double* get_double_matrix(Mat mTcw);
		double* get_modelview_matrix();
		int bigchangeID = 0;
	};
}