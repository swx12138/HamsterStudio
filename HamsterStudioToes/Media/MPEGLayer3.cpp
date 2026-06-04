#include "MPEGLayer3.h"

#pragma pack(push, 1)

typedef struct tagID3v1
{
	char tag[3];
	char title[30];
	char artist[30];
	char album[30];
	char year[4];
	char comment[30];
	uint8_t type;
} ID3v1, * PID3v1;

typedef struct tagID3V2
{
	uint8_t header[3]; // must be 'ID3'
	uint8_t version;
	uint8_t reversion;
	union {
		uint8_t flag;
		struct {
			uint8_t unsynchronisation : 1;
			uint8_t extended : 1;
			uint8_t test : 1;
		} flags;
	};
	uint8_t size[4];
}ID3V2, * PID3V2;

auto get_size_number(ID3V2 const& header)
{
	int total_size;
	total_size = (header.size[0] & 0x7F) * 0x200000
		+ (header.size[1] & 0x7F) * 0x4000
		+ (header.size[2] & 0x7F) * 0x80
		+ (header.size[3] & 0x7F);
	return total_size;
}

struct TagFrame {
	uint32_t fid;
	uint32_t size;
	union {
		uint16_t flag;
		struct
		{
			uint8_t tag_procted : 1;
			uint8_t file_procted : 1;
			uint8_t readonly : 1;
			uint8_t reversed : 5;
			uint8_t compressed : 1;
			uint8_t encrypted : 1;
			uint8_t groupped : 1;
			uint8_t reversed2 : 5;
		} flags;
	};
};

constexpr uint32_t TagFrame_Id_TIT2 = 'TIT2';
constexpr uint32_t TagFrame_Id_TPE1 = 'TPE1';
constexpr uint32_t TagFrame_Id_TALB = 'TALB';
constexpr uint32_t TagFrame_Id_TRCK = 'TRCK'; // 格式:N/M 其中 N 为专集中的第 N 首,M 为专集中共 M 首,N 和 M 为ASCII 码表示的数字
constexpr uint32_t TagFrame_Id_TYER = 'TYER';
constexpr uint32_t TagFrame_Id_TCON = 'TCON';
constexpr uint32_t TagFrame_Id_COMM = 'COMM'; // 格式:"eng\0 备注内容",其中 eng 表示备注所使用的自然语言

constexpr uint32_t VBR_Id_INFO = 'Info';
constexpr uint32_t VBR_Id_XING = 'Xing';

/*
The following frames are declared in this draft.AENC Audio encryption
APIC Attached picture
COMM Comments
COMR Commercial frame
ENCR Encryption method registration
EQUA Equalization
ETCO Event timing codes
GEOB General encapsulated object
GRID Group identification registration
IPLS Involved people list
LINK Linked information
MCDI Music CD identifier
MLLT MPEG location lookup table
OWNE Ownership frame
PRIV Private frame
PCNT Play counter
POPM Popularimeter
POSS Position synchronisation frame
RBUF Recommended buffer size
RVAD Relative volume adjustment
RVRB Reverb
SYLT Synchronized lyric/text
SYTC Synchronized tempo codes
TALB Album/Movie/Show title
TBPM BPM (beats per minute)
TCOM Composer
TCON Content type
TCOP Copyright message
TDAT Date
TDLY Playlist delay
TENC Encoded by
TEXT Lyricist/Text writer
TFLT File type
TIME Time
TIT1 Content group description
TIT2 Title/songname/content description
TIT3 Subtitle/Description refinement
TKEY Initial key
TLAN Language(s)
TLEN Length
TMED Media type
TOAL Original album/movie/show title
TOFN Original filename
TOLY Original lyricist(s)/text writer(s)TOPE Original artist(s)/performer(s)
TORY Original release year
TOWN File owner/licensee
TPE1 Lead performer(s)/Soloist(s)
TPE2 Band/orchestra/accompaniment
TPE3 Conductor/performer refinement
TPE4 Interpreted, remixed, or otherwise modified by
TPOS Part of a set
TPUB Publisher
TRCK Track number/Position in set
TRDA Recording dates
TRSN Internet radio station name
TRSO Internet radio station owner
TSIZ Size
TSRC ISRC (international standard recording code)
TSSE Software/Hardware and settings used for encoding
TYER Year
TXXX User defined text information frame
UFID Unique file identifier
USER Terms of use
USLT Unsychronized lyric/text transcription
WCOM Commercial information
WCOP Copyright/Legal information
WOAF Official audio file webpage
WOAR Official artist/performer webpage
WOAS Official audio source webpage
WORS Official internet radio station homepage
WPAY Payment
WPUB Publishers official webpage
WXXX User defined URL link frame
*/

/* SamplePerFrameTbl[version][layer]*/
constexpr uint32_t SamplePerFrameTbl[][4] = { 
	/*保留 layer3 layer2 layer1*/
	{ 0,   576,   1152,  384 },		// MPEG2.5
	{ 0,   0,     0,     0 },		// 保留
	{ 0,   576,   1152,  384 },		// MPEG2
	{ 0,   1152,  1152,  384 },		// MPEG1
};

/* 单位kbps，1kb=1000bit */

#define BitRateTable_Reversed_Values { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1 }
#define BitRateTable_Mpeg1_layer1_Values { 0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, -1 }
#define BitRateTable_Mpeg1_layer2_Values { 0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384, -1 }
#define BitRateTable_Mpeg1_layer3_Values { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, -1 }
#define BitRateTable_Mpeg2_layer1_Values { 0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256, -1 }
#define BitRateTable_Mpeg2_layer2_Values { 0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, -1 }
constexpr uint32_t BitRateTable[][4][16] = {
	/* 保留                         layer3                             layer2                             laye1  */
	{ BitRateTable_Reversed_Values, BitRateTable_Mpeg2_layer2_Values,  BitRateTable_Mpeg2_layer2_Values,  BitRateTable_Mpeg2_layer1_Values},		// MPEG2.5
	{ BitRateTable_Reversed_Values, BitRateTable_Reversed_Values,      BitRateTable_Reversed_Values,      BitRateTable_Mpeg2_layer1_Values},		// 保留
	{ BitRateTable_Reversed_Values, BitRateTable_Mpeg2_layer2_Values,  BitRateTable_Mpeg2_layer2_Values,  BitRateTable_Mpeg2_layer1_Values},		// MPEG2
	{ BitRateTable_Reversed_Values, BitRateTable_Mpeg1_layer3_Values,  BitRateTable_Mpeg1_layer2_Values,  BitRateTable_Mpeg1_layer1_Values}			// MPEG1
};

/* 单位Hz */
constexpr uint32_t SamplingRateTable[][4] =
{
	{ 11025, 12000, 8000, -1},		// MPEG2.5
	{ 11025, 12000, 8000, -1},		// 保留
	{ 22050, 24000, 16000, -1},		// MPEG2
	{ 44100, 48000, 32000, -1},		// MPEG1
};

struct FrameHeader
{
	uint32_t sync : 11;
	uint32_t version : 2;
	uint32_t layer : 2;
	uint32_t protection : 1;
	uint32_t bitrate_index : 4;
	uint32_t sampling_rate_index : 2;
	uint32_t padding : 1;
	uint32_t private_bit : 1;
	uint32_t channel_mode : 2;
	uint32_t mode_extension : 2;
	uint32_t copyright : 1;
	uint32_t original : 1;
	uint32_t emphasis : 2;
};

static inline void decode(FrameHeader& header, uint32_t raw)
{
	header.sync = (raw >> 21) & 0x7FF;
	header.version = (raw >> 19) & 0x3;
	header.layer = (raw >> 17) & 0x3;
	header.protection = (raw >> 16) & 0x1;
	header.bitrate_index = (raw >> 12) & 0xF;
	header.sampling_rate_index = (raw >> 10) & 0x3;
	header.padding = (raw >> 9) & 0x1;
	header.private_bit = (raw >> 8) & 0x1;
	header.channel_mode = (raw >> 6) & 0x3;
	header.mode_extension = (raw >> 4) & 0x3;
	header.copyright = (raw >> 3) & 0x1;
	header.original = (raw >> 2) & 0x1;
	header.emphasis = (raw >> 0) & 0x3;
}

uint32_t get_block_size(FrameHeader const& frameHeader)
{
	const auto samples_per_frame = SamplePerFrameTbl[frameHeader.version][frameHeader.layer];
	const auto bitrate = BitRateTable[frameHeader.version][frameHeader.layer][frameHeader.bitrate_index] * 1000;
	const auto sampling_rate = SamplingRateTable[frameHeader.version][frameHeader.sampling_rate_index];
	const auto slot_size = frameHeader.layer == 3 ? 4 : 1;
	const auto block_size = (samples_per_frame / 8.0 * bitrate) / sampling_rate + frameHeader.padding * slot_size;
	return static_cast<uint32_t>(block_size);
}

struct VariableBitRateHeader {
	uint32_t id;
	uint32_t flag;
	uint32_t frames_count;
	uint32_t file_size;
	uint8_t toc_index[100];
	uint32_t quality;
};

#pragma pack(pop)

struct MPEGLayer3MediaNamespace::MPEGLayer3Format::Format {

	VariableBitRateHeader vbr_hdr;
	ID3V2 id3v2;
};

#include <fstream>
#include "../Tools/StreamReader.h"
#include "../Tools/NumberUtils.h"

#include <iostream>

MPEGLayer3MediaNamespace::MPEGLayer3Format::MPEGLayer3Format(std::filesystem::path const& filepath) noexcept
{
	_data = std::make_shared<Format>();

	std::ifstream ifs(filepath, std::ios::binary);
	Tools::StreamReader reader(ifs);

	FrameHeader header = { 0 };
	std::cout << "file size : " << reader.GetSize() << std::endl;
	for (int blockIdx = 0;!ifs.eof();++blockIdx)
	{
		//reader.Read((char*)(&_data->header), sizeof(_data->header));		// 顺序不对，，，
		
		const auto blockHeaderRawValue = reader.Read<uint32_t>();
		if (!blockHeaderRawValue.second)
		{
			std::cout << "reaces end of file." << std::endl;
			break;
		}

		//*(uint32_t*)(&_data->header) = reader.Read<uint32_t>().first;		// 还是不对
		decode(header, Tools::SwitchEndian(blockHeaderRawValue.first));
		if (header.sync != 0x7ff)
		{
			std::cout << "irregular header, sync: " << std::hex << header.sync << std::dec << std::endl;
			break;
		}

		const auto blockSize = get_block_size(header);
		reader.SeekForward(blockSize - 4);

		//std::cout << "block " << blockIdx << " size: " << blockSize << ", stream pos : " << std::hex << reader.GetPostion() << std::endl;
		printf_s("block %4d, size: %8ud, stream pos : %8llx\n", blockIdx, blockSize, reader.GetPostion());
		continue;

		// ？跳过连续0
		while (1) {
			if (auto result = reader.Read<uint8_t>();result.second && result.first == 0x00) {
				std::cout << '.';
				continue;
			}
			else if (result.second) {
				break;
			}
			else {
				std::wcerr << L"意外结束" << std::endl;
				return;
			}
		}
		std::cout << std::endl;
		reader.SeekBackward(1);

		for (auto result = reader.Read<uint32_t>();result.second;result = reader.Read<uint32_t>())
		{
			if (auto actual_magic = Tools::SwitchEndian(result.first);actual_magic == VBR_Id_INFO || actual_magic == VBR_Id_XING)
			{
				_data->vbr_hdr.id = actual_magic;

				auto len = reader.Read<uint32_t>();
				if (!len.second) {
					std::wcerr << L"意外结束2" << std::endl;
					return;
				}
				_data->vbr_hdr.flag = Tools::SwitchEndian(len.first);

				int usedSize = 0;
				if ((_data->vbr_hdr.flag & 0x1) == 0x1) {
					usedSize += 4;
					_data->vbr_hdr.frames_count = Tools::SwitchEndian(reader.Read<uint32_t>().first);
				}
				if ((_data->vbr_hdr.flag & 0x2) == 0x2) {
					usedSize += 4;
					_data->vbr_hdr.file_size = Tools::SwitchEndian(reader.Read<uint32_t>().first);
				}
				if ((_data->vbr_hdr.flag & 0x4) == 0x4) {
					usedSize += 100;
					reader.Read(_data->vbr_hdr.toc_index, 100);
				}
				if ((_data->vbr_hdr.flag & 0x8) == 0x8) {
					usedSize += 4;
					_data->vbr_hdr.quality = Tools::SwitchEndian(reader.Read<uint32_t>().first);
				}

				const auto remaied = blockSize - usedSize;
				ifs.seekg(blockSize, std::ios::beg); // 跳过扩展信息

				std::cout << "read vbr section" << std::endl;
			}
			else if ((result.first >> 8) == 'ID3') {
				reader.SeekBackward(8);
				reader.Read(&_data->id3v2, sizeof(_data->id3v2));
				std::cout << "read ID3 section" << std::endl;
			}
			else {
				printf_s("skpiied %c%c%c%c(%x)\n",
					(char)result.first, (char)(result.first >> 8), (char)(result.first >> 16), (char)(result.first >> 24), result.first);
			}
		}
	}
}

MPEGLayer3MediaNamespace::MPEGLayer3Format::~MPEGLayer3Format() noexcept
{
}
