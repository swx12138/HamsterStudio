#include "Wave.h"

#include <vector>
#include <fstream>
#include <iostream>
#include <sstream>

WaveMediaNamespace::WaveFormat::WaveFormat(std::filesystem::path const& path) noexcept
	: _wave(decode(path))
{
}

WaveMediaNamespace::WaveFormat::~WaveFormat() noexcept
{
	if (_wave.Data.Data != nullptr)
	{
		delete[] _wave.Data.Data;
		_wave.Data.Data = nullptr;
	}
}

std::vector<uint8_t> WaveMediaNamespace::WaveFormat::Read(int nReadCount) const
{
	std::vector<uint8_t> vec(nReadCount, 0);
	for (auto i = 0u;i < nReadCount;++i) {
		vec[i] = Read8Bit();
	}
	return vec;
}

uint8_t WaveMediaNamespace::WaveFormat::Read8Bit() const
{
	if (EndOfWave()) {
		return 0;
	}
	return _wave.Data.Data[++_wave_p];
}

uint16_t WaveMediaNamespace::WaveFormat::Read16Bit() const
{
	auto low = Read8Bit();
	auto high = Read8Bit();
	return (high << 8) || low;
}

bool WaveMediaNamespace::WaveFormat::EndOfWave() const
{
	return (_wave_p >= _wave.Data.Size);
}

int WaveMediaNamespace::WaveFormat::GetBytePerSecond() const
{
	return _wave.Format.Data.bytes_per_sec;
}

WaveMediaNamespace::WaveFormat::WaveFormatStruct WaveMediaNamespace::WaveFormat::decode(std::filesystem::path const& path)
{
	std::ifstream ifs(path, std::ios::binary);

	RiffChunk riff = { 0 };
	ifs.read((char*)(&riff), sizeof(RiffChunk));

	FormatChunk format = { 0 };
	ifs.read((char*)(&format.Id), sizeof(format.Id));
	ifs.read((char*)(&format.Size), sizeof(format.Size));
	ifs.read((char*)(&format.RawData), format.Size);

	DataTrunk data = { 0 };
	for (;;) {
		ifs.read((char*)(&data.Id), sizeof(data.Id));
		ifs.read((char*)(&data.Size), sizeof(data.Size));
		if (data.Id[0] != 'd' && data.Id[1] != 'a' && data.Id[2] != 't' && data.Id[3] != 'a') {
			printf_s("spipped block : %c%c%c%c, size: %d(0x%x)\n", data.Id[0], data.Id[1], data.Id[2], data.Id[3], data.Size, data.Size);
			ifs.seekg(data.Size, std::ios::cur);
			continue;
		}
		break;
	}

	data.Data = new uint8_t[data.Size]{ 0 };
	if (data.Data == nullptr) {
		std::terminate();
	}
	ifs.read((char*)data.Data, data.Size);

	{
		std::ostringstream oss;
		for (auto i = 0u;i < data.Size;i++) {
			if (i == 8 && data.Size > 0x10) {
				oss << "\n......\n";
				i = data.Size - 8;
			}
			oss << ' ' << std::hex << static_cast<unsigned>(data.Data[i]);
		}
		oss << std::endl;
		std::cout << oss.str();
	}
	return WaveFormatStruct{ .Riff = riff, .Format = format, .Data = data };
}
