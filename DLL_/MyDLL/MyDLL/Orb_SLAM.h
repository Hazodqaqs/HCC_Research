/// ORBSLAM
#pragma warning( disable : 4996 )
#include <opencv2/core/core.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>

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
	class Orb_SLAM
	{
	public:

		/// Parameters
		MyCallback cbCpp;
		int input_t = 0;
		bool new_input_RGB = false;
		bool new_input_D = false;
		vector<float> vTimesTrack;

		// model view matrix
		double mModelview_matrix[16];
		cv::Mat mTcw;
		cv::Mat imRGB, imD;

		// mutex and locks
		mutex mMutex;
		mutex mMutexFinish;
		bool mbFinishRequested;
		bool mbFinished;

		uint16_t lastKeyframeSize = 0;
		void generateMap();
		bool shutDownFlag = false;
		condition_variable keyFrameUpdated;
		mutex shutDownMutex;
		mutex keyframeMutex;


		/// Constructor
		Orb_SLAM(MyCallback cb, const int input_type = 0);
		~Orb_SLAM();
		

		/// SLAM system
		double tframe = 0.0f;
		vector<ORB_SLAM2::KeyFrame *> keyframes;
		ORB_SLAM2::System* SLAM_system_ptr;
		ORB_SLAM2::Map* map;
		

		/// Execute Function
		bool init();

		// Update
		void update(Mat& rgb, Mat& depth); 
		
		// ROS
		void run_with_ROS();

		// Excute
		double M_arr[16];

		void requestFinish();
		bool checkFinish();
		void setFinish();
		bool isFinished();
		

		/// Get Function
		ORB_SLAM2::KeyFrame *get_keyFrameByID(int ID);
		cv::Mat get_keyFrameByID_RGB(int ID);
		cv::Mat get_keyFrameByID_Depth(int ID);

		//return the 4*4 gl model view matrix
		cv::Mat get_imRGB();

		// return the domain plane mean and normal in the world coord
		cv::Mat get_imD();
		cv::Mat get_pose();
		double* get_double_matrix(Mat mTcw);
		double* get_modelview_matrix();
		int bigchangeID = 0;
	};
}