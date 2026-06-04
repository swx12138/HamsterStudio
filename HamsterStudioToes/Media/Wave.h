#pragma once

#define WaveMediaNamespace HamsterStudioToes::Media

#include <filesystem>
#include <memory>
#include <array>

namespace WaveMediaNamespace
{
	class WaveFormat
	{		
		struct RiffChunk
		{
			uint8_t Id[4];
			uint32_t Size;
			uint8_t Data[4];
		};

		struct FormatChunkData {
			uint16_t audio_format, channels;
			uint32_t sample_rate, bytes_per_sec;
			uint16_t block_align, bits_per_sample;
			uint16_t cbSize;
			uint8_t ExtData[22];
		};

		struct FormatChunk
		{
			uint8_t Id[4];
			uint32_t Size;
			union {
				uint8_t RawData[40];
				FormatChunkData Data;
			};
		};

		struct DataTrunk {
			uint8_t Id[4];
			uint32_t Size;
			uint8_t* Data;
		};

		struct WaveFormatStruct {
			RiffChunk Riff;
			FormatChunk Format;
			DataTrunk Data;
		};

		WaveFormatStruct _wave;
		mutable size_t _wave_p = 0;
	public:
		explicit WaveFormat(std::filesystem::path const& path) noexcept;

		~WaveFormat() noexcept;

		std::vector<uint8_t> Read(int nReadCount) const;
		uint8_t Read8Bit() const;
		uint16_t Read16Bit() const;

		bool EndOfWave() const;

		int GetBytePerSecond() const;

	private:
		static WaveFormatStruct decode(std::filesystem::path const& path);

	};

}
