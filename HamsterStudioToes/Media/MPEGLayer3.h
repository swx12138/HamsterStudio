#pragma once

#define MPEGLayer3MediaNamespace HamsterStudioToes::Media

#include <memory>
#include <filesystem>

namespace MPEGLayer3MediaNamespace
{
	class MPEGLayer3Format
	{
		struct Format;
		std::shared_ptr<Format> _data;

	public:
		explicit MPEGLayer3Format(std::filesystem::path const& filepath) noexcept;

		~MPEGLayer3Format() noexcept;

	};

}
