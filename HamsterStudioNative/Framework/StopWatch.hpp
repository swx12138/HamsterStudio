#pragma once

#include <chrono>

class StopWatch{
	std::chrono::system_clock::time_point start_time;
public:
	explicit StopWatch() {
		start_time = std::chrono::system_clock::now();
	}

	void reset() {
		start_time = std::chrono::system_clock::now();
	}

	auto elapsed() const {
		auto now = std::chrono::system_clock::now();
		return now - start_time;
	}
};