
#include "stdafx.h"
#pragma warning( disable : 4996 )
#include "Orb_SLAM.h"


Orb_SLAM::Orb_SLAM( MyCallback cb, const int input_type)
{
	input_t = input_type;
	mbFinishRequested = false;
	mbFinished = false;
	cbCpp = cb; 
}
void Orb_SLAM::Run()
{
	while (true)
	{
		if (checkFinish())
			break;


		if (bigchangeID == map->GetLastBigChangeIdx())
		{
			if (lastKeyframeSize != map->GetMaxKFid())
			{
		#ifdef COMPILEDWITHC11
						std::chrono::steady_clock::time_point t1 = std::chrono::steady_clock::now();
		#else
						std::chrono::high_resolution_clock::time_point t0 = std::chrono::high_resolution_clock::now();
		#endif
				keyframes = map->GetAllKeyFrames();
				size_t N = 0;
				{
					N = keyframes.size();
				}
		#ifdef COMPILEDWITHC11
						std::chrono::steady_clock::time_point t1 = std::chrono::steady_clock::now();
		#else
						std::chrono::high_resolution_clock::time_point t1 = std::chrono::high_resolution_clock::now();
		#endif
						double ttrack0 = std::chrono::duration_cast<std::chrono::duration<double>>(t1 - t0).count();


				for (size_t i = lastKeyframeSize; i < N; i++)
				{
					cbCpp(i);
				}

		#ifdef COMPILEDWITHC11
						std::chrono::steady_clock::time_point t2 = std::chrono::steady_clock::now();
		#else
						std::chrono::high_resolution_clock::time_point t2 = std::chrono::high_resolution_clock::now();
		#endif
				double ttrack = std::chrono::duration_cast<std::chrono::duration<double>>(t2 - t1).count();
				lastKeyframeSize = N;
				if (checkFinish())
					break;
				else
					waitKey(30);
			}
		}
		else {
			generateMap();
			lastKeyframeSize = map->GetMaxKFid();
			bigchangeID = map->GetLastBigChangeIdx();
			waitKey(30);
		}
	}


}
void Orb_SLAM::generateMap()
{
	keyframes = map->GetAllKeyFrames();
	cout << "generate Map" << endl;

	size_t N = 0;
	{
		N = keyframes.size();
	}
	cout << "N:" << N << endl;
	for (size_t i = 0; i < N; i++)
	{
		cout << "i:" << i << endl;
		cbCpp(i);
	}
}
Orb_SLAM::~Orb_SLAM()
{
	delete SLAM_system_ptr;
}
ORB_SLAM2::KeyFrame * Orb_SLAM::get_keyFrameByID(int ID) {
	return keyframes[ID];
}

Mat Orb_SLAM::get_keyFrameByID_RGB(int ID) {
	return keyframes[ID]->img0;
}
Mat Orb_SLAM::get_keyFrameByID_Depth(int ID) {
	return keyframes[ID]->img1;
}

bool Orb_SLAM::init()
{
	SLAM_system_ptr = new ORB_SLAM2::System("./ORBvoc.txt", "./TUM1.yaml", ORB_SLAM2::System::RGBD, 1);
	mModelview_matrix[0] = 1.0f;
	mModelview_matrix[1] = 0.0f;
	mModelview_matrix[2] = 0.0f;
	mModelview_matrix[3] = 0.0f;

	mModelview_matrix[4] = 0.0f;
	mModelview_matrix[5] = 1.0f;
	mModelview_matrix[6] = 0.0f;
	mModelview_matrix[7] = 0.0f;

	mModelview_matrix[8] = 0.0f;
	mModelview_matrix[9] = 0.0f;
	mModelview_matrix[10] = 1.0f;
	mModelview_matrix[11] = 0.0;

	mModelview_matrix[12] = 0.0f;
	mModelview_matrix[13] = 0.0f;
	mModelview_matrix[14] = 0.0f;
	mModelview_matrix[15] = 1.0;
	M_arr[0] = 1.0f;
	M_arr[1] = 0.0f;
	M_arr[2] = 0.0f;
	M_arr[3] = 0.0f;

	M_arr[4] = 0.0f;
	M_arr[5] = 1.0f;
	M_arr[6] = 0.0f;
	M_arr[7] = 0.0f;

	M_arr[8] = 0.0f;
	M_arr[9] = 0.0f;
	M_arr[10] = 1.0f;
	M_arr[11] = 0.0;

	M_arr[12] = 0.0f;
	M_arr[13] = 0.0f;
	M_arr[14] = 0.0f;
	M_arr[15] = 1.0;
	map = SLAM_system_ptr->GetMap();
	
	return true;
}

void Orb_SLAM::update(Mat& rgb, Mat& depth)
{
	unique_lock<mutex> lock(mMutex);
	if (input_t == 0)
	{
	}
	else {
		rgb.copyTo(imRGB);
		depth.copyTo(imD);
		new_input_RGB = true;
		new_input_D = true;
	}


}
void Orb_SLAM::update2() {
	rs2::pipeline pipe;
	rs2::config pipe_config;
	pipe_config.enable_stream(RS2_STREAM_DEPTH, 640, 480, RS2_FORMAT_Z16, 30);
	pipe_config.enable_stream(RS2_STREAM_COLOR, 640, 480, RS2_FORMAT_BGR8, 30);

	//start()函數返回數據管道的profile
	rs2::pipeline_profile profile = pipe.start(pipe_config);

	//定義一個變量去轉換深度到距離
	float depth_clipping_distance = 1.f;
	//聲明數據流
	auto depth_stream = profile.get_stream(RS2_STREAM_DEPTH).as<rs2::video_stream_profile>();
	auto color_stream = profile.get_stream(RS2_STREAM_COLOR).as<rs2::video_stream_profile>();
	rs2::frameset frameset = pipe.wait_for_frames();

	rs2::frame color_frame = frameset.get_color_frame();//processed.first(align_to);
	rs2::frame depth_frame = frameset.get_depth_frame();
	//獲取寬高
	const int depth_w = depth_frame.as<rs2::video_frame>().get_width();
	const int depth_h = depth_frame.as<rs2::video_frame>().get_height();
	const int color_w = color_frame.as<rs2::video_frame>().get_width();
	const int color_h = color_frame.as<rs2::video_frame>().get_height();

	//創建OPENCV類型 並傳入數據
	cv::Mat _imD(Size(depth_w, depth_h),
		CV_16U, (void*)depth_frame.get_data(), Mat::AUTO_STEP);
	cv::Mat _imRGB(Size(color_w, color_h),
		CV_8UC3, (void*)color_frame.get_data(), Mat::AUTO_STEP);
	_imD.copyTo(imD);
	_imRGB.copyTo(imRGB);

	new_input_RGB = true;
	new_input_D = true;

}
void Orb_SLAM::run()
{
	if (input_t == 0) {

		rs2::pipeline pipe;
		rs2::config pipe_config;
		pipe_config.enable_stream(RS2_STREAM_DEPTH, 640, 480, RS2_FORMAT_Z16, 30);
		pipe_config.enable_stream(RS2_STREAM_COLOR, 640, 480, RS2_FORMAT_BGR8, 30);

		//start()函數返回數據管道的profile
		rs2::pipeline_profile profile = pipe.start(pipe_config);

		//定義一個變量去轉換深度到距離
		float depth_clipping_distance = 1.f;
		//聲明數據流
		auto depth_stream = profile.get_stream(RS2_STREAM_DEPTH).as<rs2::video_stream_profile>();
		auto color_stream = profile.get_stream(RS2_STREAM_COLOR).as<rs2::video_stream_profile>();

		while (1)
		{
			rs2::frameset frameset = pipe.wait_for_frames();

			rs2::frame color_frame = frameset.get_color_frame();//processed.first(align_to);
			rs2::frame depth_frame = frameset.get_depth_frame();
			//獲取寬高
			const int depth_w = depth_frame.as<rs2::video_frame>().get_width();
			const int depth_h = depth_frame.as<rs2::video_frame>().get_height();
			const int color_w = color_frame.as<rs2::video_frame>().get_width();
			const int color_h = color_frame.as<rs2::video_frame>().get_height();

			//創建OPENCV類型 並傳入數據
			cv::Mat _imD(Size(depth_w, depth_h),
				CV_16U, (void*)depth_frame.get_data(), Mat::AUTO_STEP);
			cv::Mat _imRGB(Size(color_w, color_h),
				CV_8UC3, (void*)color_frame.get_data(), Mat::AUTO_STEP);
			if (_imRGB.empty() || _imD.empty())
			{
				waitKey(30);
				continue;
			}
			{
				unique_lock<mutex> lock(mMutex);
				_imD.copyTo(imD);
				_imRGB.copyTo(imRGB);
				tframe += 0.0003f;
			}


			//imshow("left", frame_rectify_left);
			//imshow("right", frame_rectify_right);
			//waitKey(30);


#ifdef COMPILEDWITHC11
			std::chrono::steady_clock::time_point t1 = std::chrono::steady_clock::now();
#else
			std::chrono::high_resolution_clock::time_point t1 = std::chrono::high_resolution_clock::now();
#endif

			mTcw = SLAM_system_ptr->TrackRGBD(_imRGB, _imD, tframe);


			//imshow("left", frame_rectify_left);
			//imshow("right", frame_rectify_right);
			//waitKey(30);

#ifdef COMPILEDWITHC11
			std::chrono::steady_clock::time_point t2 = std::chrono::steady_clock::now();
#else
			std::chrono::high_resolution_clock::time_point t2 = std::chrono::high_resolution_clock::now();
#endif

			double ttrack = std::chrono::duration_cast<std::chrono::duration<double>>(t2 - t1).count();

#ifdef DEBUG_LOG
			log_file << "tracking state " << SLAM_system_ptr->mpTracker->mState << endl;
#endif
			vTimesTrack.push_back(ttrack);
			if (!mTcw.empty())
			{
				//std::stringstream sstrMat;
				//sstrMat << mTcw;
				//DebugInUnity(sstrMat.str());

				cv::Mat Rcw(3, 3, CV_32F);
				cv::Mat tcw(3, 1, CV_32F);

				Rcw = mTcw.rowRange(0, 3).colRange(0, 3);
				tcw = mTcw.rowRange(0, 3).col(3);

				mModelview_matrix[0] = Rcw.at<float>(0, 0);
				mModelview_matrix[1] = Rcw.at<float>(1, 0);
				mModelview_matrix[2] = Rcw.at<float>(2, 0);
				mModelview_matrix[3] = 0.0;

				mModelview_matrix[4] = Rcw.at<float>(0, 1);
				mModelview_matrix[5] = Rcw.at<float>(1, 1);
				mModelview_matrix[6] = Rcw.at<float>(2, 1);
				mModelview_matrix[7] = 0.0;

				mModelview_matrix[8] = Rcw.at<float>(0, 2);
				mModelview_matrix[9] = Rcw.at<float>(1, 2);
				mModelview_matrix[10] = Rcw.at<float>(2, 2);
				mModelview_matrix[11] = 0.0;

				mModelview_matrix[12] = tcw.at<float>(0);
				mModelview_matrix[13] = tcw.at<float>(1);
				mModelview_matrix[14] = tcw.at<float>(2);
				mModelview_matrix[15] = 1.0;

			}

			if (checkFinish())
				break;

			//log_file << "after checkfinish()" << endl;
		}

	}
	else {
		while (1)
		{
			if (checkFinish())
				break;
			if (!new_input_RGB || !new_input_D)
			{
				waitKey(30);
				continue;
			}
			new_input_RGB = false;
			new_input_D = false;

			tframe += 0.0003f;


			//imshow("left", frame_rectify_left);
			//imshow("right", frame_rectify_right);
			//waitKey(30);


		#ifdef COMPILEDWITHC11
			std::chrono::steady_clock::time_point t1 = std::chrono::steady_clock::now();
		#else
			std::chrono::high_resolution_clock::time_point t1 = std::chrono::high_resolution_clock::now();
		#endif

			mTcw = SLAM_system_ptr->TrackRGBD(imRGB, imD, tframe);


			//imshow("left", frame_rectify_left);
			//imshow("right", frame_rectify_right);
			//waitKey(30);

		#ifdef COMPILEDWITHC11
			std::chrono::steady_clock::time_point t2 = std::chrono::steady_clock::now();
		#else
			std::chrono::high_resolution_clock::time_point t2 = std::chrono::high_resolution_clock::now();
		#endif

			double ttrack = std::chrono::duration_cast<std::chrono::duration<double>>(t2 - t1).count();

		#ifdef DEBUG_LOG
			log_file << "tracking state " << SLAM_system_ptr->mpTracker->mState << endl;
		#endif
			vTimesTrack.push_back(ttrack);
			
			if (!mTcw.empty())
			{
				//std::stringstream sstrMat;
				//sstrMat << mTcw;
				//DebugInUnity(sstrMat.str());

				cv::Mat Rcw(3, 3, CV_32F);
				cv::Mat tcw(3, 1, CV_32F);

				Rcw = mTcw.rowRange(0, 3).colRange(0, 3);
				tcw = mTcw.rowRange(0, 3).col(3);

				mModelview_matrix[0] = Rcw.at<float>(0, 0);
				mModelview_matrix[1] = Rcw.at<float>(1, 0);
				mModelview_matrix[2] = Rcw.at<float>(2, 0);
				mModelview_matrix[3] = 0.0;

				mModelview_matrix[4] = Rcw.at<float>(0, 1);
				mModelview_matrix[5] = Rcw.at<float>(1, 1);
				mModelview_matrix[6] = Rcw.at<float>(2, 1);
				mModelview_matrix[7] = 0.0;

				mModelview_matrix[8] = Rcw.at<float>(0, 2);
				mModelview_matrix[9] = Rcw.at<float>(1, 2);
				mModelview_matrix[10] = Rcw.at<float>(2, 2);
				mModelview_matrix[11] = 0.0;

				mModelview_matrix[12] = tcw.at<float>(0);
				mModelview_matrix[13] = tcw.at<float>(1);
				mModelview_matrix[14] = tcw.at<float>(2);
				mModelview_matrix[15] = 1.0;

			}

			if (checkFinish())
				break;
			//log_file << "after checkfinish()" << endl;
		}
	}
	setFinish();

	SLAM_system_ptr->Shutdown();
}

void Orb_SLAM::requestFinish()
{
	unique_lock<mutex> lock(mMutexFinish);
	cout << "requestFinish" << endl;
	mbFinishRequested = true;
}

bool Orb_SLAM::checkFinish()
{
	unique_lock<mutex> lock(mMutexFinish);
	return mbFinishRequested;
}

void Orb_SLAM::setFinish()
{
	unique_lock<mutex> lock(mMutexFinish);
	mbFinished = true;
}

bool Orb_SLAM::isFinished()
{
	unique_lock<mutex> lock(mMutexFinish);
	return mbFinished;
}
double* Orb_SLAM::get_double_matrix(Mat mTcw) {
	if (!mTcw.empty())
	{
		//std::stringstream sstrMat;
		//sstrMat << mTcw;
		//DebugInUnity(sstrMat.str());

		cv::Mat Rcw(3, 3, CV_32F);
		cv::Mat tcw(3, 1, CV_32F);

		Rcw = mTcw.rowRange(0, 3).colRange(0, 3);
		tcw = mTcw.rowRange(0, 3).col(3);

		M_arr[0] = Rcw.at<float>(0, 0);
		M_arr[1] = Rcw.at<float>(1, 0);
		M_arr[2] = Rcw.at<float>(2, 0);
		M_arr[3] = 0.0;

		M_arr[4] = Rcw.at<float>(0, 1);
		M_arr[5] = Rcw.at<float>(1, 1);
		M_arr[6] = Rcw.at<float>(2, 1);
		M_arr[7] = 0.0;

		M_arr[8] = Rcw.at<float>(0, 2);
		M_arr[9] = Rcw.at<float>(1, 2);
		M_arr[10] = Rcw.at<float>(2, 2);
		M_arr[11] = 0.0;

		M_arr[12] = tcw.at<float>(0);
		M_arr[13] = tcw.at<float>(1);
		M_arr[14] = tcw.at<float>(2);
		M_arr[15] = 1.0;

	}
	return M_arr;
}
double* Orb_SLAM::get_modelview_matrix()
{
	unique_lock<mutex> lock(mMutex);
	return mModelview_matrix;
}

Mat Orb_SLAM::get_imRGB()
{
	unique_lock<mutex> lock(mMutex);
	return imRGB;
}

Mat Orb_SLAM::get_imD()
{
	unique_lock<mutex> lock(mMutex);
	return imD;
}

Mat Orb_SLAM::get_pose()
{
	unique_lock<mutex> lock(mMutex);
	return mTcw;
}