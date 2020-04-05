
#include "stdafx.h"
#include "VideoCap.h"

using namespace cv;

//defalut constructor
VideoCap::VideoCap()
{
	pipe_config.enable_stream(RS2_STREAM_DEPTH, 640, 480, RS2_FORMAT_Z16, 60);
	pipe_config.enable_stream(RS2_STREAM_COLOR, 640, 480, RS2_FORMAT_BGR8, 60);
	profile = pipe.start(pipe_config);
	depth_stream = profile.get_stream(RS2_STREAM_DEPTH).as<rs2::video_stream_profile>();
	color_stream = profile.get_stream(RS2_STREAM_COLOR).as<rs2::video_stream_profile>();
	align_to_color = new rs2::align(RS2_STREAM_COLOR);
}

void VideoCap::Update(cv::Mat& rgb, cv::Mat& depth) {
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
	Depth = cv::Mat(Size(depth_w, depth_h), CV_16U, (void*)depth_frame.get_data(), Mat::AUTO_STEP);
	RGB = cv::Mat(Size(color_w, color_h), CV_8UC3, (void*)color_frame.get_data(), Mat::AUTO_STEP);


}

//destructor
VideoCap::~VideoCap()
{
	pipe.stop();
}


Mat VideoCap::get_rgb()
{
	return RGB;
}

Mat VideoCap::get_depth()
{
	return Depth;
}


