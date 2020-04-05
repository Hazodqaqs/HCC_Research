#ifndef __VIDEOCAP_H
#define __VIDEOCAP_H

#include <opencv2\opencv.hpp>
#include <opencv2\core\core.hpp>
#include <opencv2\highgui\highgui.hpp>
#include <opencv2\imgproc\imgproc.hpp>

#include <librealsense2/rs.hpp>
//image size for ar mod rendering
//current PTAM-demo machine camera setup is flipped 90.
#define OPENCV_VIDEO_W 640
#define OPENCV_VIDEO_H 480

//image size for tracking, usually smaller for faster frame rate.
#define TRACK_IMAGE_W 640
#define TRACK_IMAGE_H 480


class VideoCap
{
public:
	VideoCap();
	rs2::pipeline pipe;
	rs2::config pipe_config;
	rs2::pipeline_profile profile;
	rs2::stream_profile depth_stream;
	rs2::stream_profile color_stream;
	rs2::align* align_to_color;
	cv::Mat RGB;
	cv::Mat Depth;
	void Update(cv::Mat& rgb, cv::Mat& depth);
	void Update();

	~VideoCap();
	cv::Mat get_rgb();
	cv::Mat get_depth();


};


#endif
