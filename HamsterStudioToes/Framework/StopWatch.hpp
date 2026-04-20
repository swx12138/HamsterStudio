#pragma once

#include <chrono>

namespace HamsterStudioToes
{
	/// <summary>
	/// ¼ÆÊ±Æ÷
	/// </summary>
	class StopWatch {
		using Clock = std::chrono::steady_clock;
		using TimePoint = Clock::time_point;

		TimePoint start_time;
	public:
		explicit StopWatch() {
			start_time = Clock::now();
		}

		void reset() {
			start_time = Clock::now();
		}

		template<typename Duration = std::chrono::microseconds>
		auto elapsed() const {
			auto now = Clock::now();
			return std::chrono::duration_cast<Duration>(now - start_time);
		}
	};

}