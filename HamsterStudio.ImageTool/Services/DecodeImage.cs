using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.FileSystem;


namespace HamsterStudio.ImageTool.Services;

public record SonyIFD0Meta(string Make,
                          string Model,
                          string Software,
                          string Time,
                          string Artist,
                          string Copyright,
                          string ExposureTime,
                          string FNumber,
                          string ExposureProgram,
                          string ISOSpeedRatings,
                          string ExifVersion,
                          string LensModel,
                          string WhiteBalanceMode,
                          string ExifImageWidth,
                          string ExifImageHeight)
{ }

public record SonyExifImageMeta(string NewSubfileType,
                                string ImageWidth,
                                string ImageHeight,
                                string BitsPerSample,
                                string PhotometricInterpretation,
                                string ImageDescription,
                                string Orientation,
                                string SamplesPerPixel,
                                string PlanarConfiguration,
                                string YCbCrCoefficients,
                                string YCbCrSubSampling,
                                string YCbCrPositioning,
                                string ReferenceBlackWhite)
{ }

public record SonyFileMeta(string FileName,
                           string FileSize,
                           string FileModifiedDate)
{ }

public class SonyArwMetas
{
    public SonyIFD0Meta IFD0 { get; set; }
    public SonyExifImageMeta ExifImage { get; set; }
    public SonyFileMeta File { get; set; }
}

public class DecodeImage
{
    public SonyArwMetas LoadSonyRaw(string sonyRawFilename)
    {
        try
        {
            // 读取所有元数据目录
            var directories = ImageMetadataReader.ReadMetadata(sonyRawFilename);
            SonyArwMetas arwMetas = new();

            // 解析 Exif IFD0 目录
            var ifd0Dir = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (ifd0Dir != null)
            {
                arwMetas.IFD0 = new SonyIFD0Meta(
                    Make: ifd0Dir.GetString(ExifDirectoryBase.TagMake)!,
                    Model: ifd0Dir.GetString(ExifDirectoryBase.TagModel)!,
                    Software: ifd0Dir.GetString(ExifDirectoryBase.TagSoftware)!,
                    Time: ifd0Dir.GetString(ExifDirectoryBase.TagDateTime)!,
                    Artist: ifd0Dir.GetString(ExifDirectoryBase.TagArtist)!,
                    Copyright: ifd0Dir.GetString(ExifDirectoryBase.TagCopyright)!,
                    ExposureTime: null, // 在SubIFD中解析
                    FNumber: null,     // 在SubIFD中解析
                    ExposureProgram: null, // 在SubIFD中解析
                    ISOSpeedRatings: null, // 在SubIFD中解析
                    ExifVersion: null, // 在SubIFD中解析
                    LensModel: null,   // 在SubIFD中解析
                    WhiteBalanceMode: null, // 在SubIFD中解析
                    ExifImageWidth: null, // 在SubIFD中解析
                    ExifImageHeight: null  // 在SubIFD中解析
                );
            }

            // 解析包含拍摄参数的 Exif SubIFD 目录
            var subIfdDirs = directories.OfType<ExifSubIfdDirectory>().ToList();
            if (subIfdDirs.Count >= 2)
            {
                // 第二个SubIFD包含拍摄参数（根据文件内容）
                var subIfdDir = subIfdDirs[1];

                // 更新IFD0信息（合并拍摄参数）
                arwMetas.IFD0 = arwMetas.IFD0 with
                {
                    ExposureTime = subIfdDir.GetString(ExifDirectoryBase.TagExposureTime)!,
                    FNumber = subIfdDir.GetString(ExifDirectoryBase.TagFNumber)!,
                    ExposureProgram = subIfdDir.GetString(ExifDirectoryBase.TagExposureProgram)!,
                    ISOSpeedRatings = subIfdDir.GetString(ExifDirectoryBase.TagIsoEquivalent)!,
                    ExifVersion = subIfdDir.GetString(ExifDirectoryBase.TagExifVersion)!,
                    LensModel = subIfdDir.GetString(ExifDirectoryBase.TagLensModel)!,
                    WhiteBalanceMode = subIfdDir.GetString(ExifDirectoryBase.TagWhiteBalance)!,
                    ExifImageWidth = subIfdDir.GetString(ExifDirectoryBase.TagExifImageWidth)!,
                    ExifImageHeight = subIfdDir.GetString(ExifDirectoryBase.TagExifImageHeight)!
                };
            }

            // 解析 Exif Image 目录
            var exifImageDir = directories.OfType<ExifImageDirectory>().FirstOrDefault();
            if (exifImageDir != null)
            {
                arwMetas.ExifImage = new SonyExifImageMeta(
                    NewSubfileType: exifImageDir.GetString(ExifDirectoryBase.TagNewSubfileType)!,
                    ImageWidth: exifImageDir.GetString(ExifDirectoryBase.TagImageWidth)!,
                    ImageHeight: exifImageDir.GetString(ExifDirectoryBase.TagImageHeight)!,
                    BitsPerSample: exifImageDir.GetString(ExifDirectoryBase.TagBitsPerSample)!,
                    PhotometricInterpretation: exifImageDir.GetString(ExifDirectoryBase.TagPhotometricInterpretation)!,
                    ImageDescription: exifImageDir.GetString(ExifDirectoryBase.TagImageDescription)!,
                    Orientation: exifImageDir.GetString(ExifDirectoryBase.TagOrientation)!,
                    SamplesPerPixel: exifImageDir.GetString(ExifDirectoryBase.TagSamplesPerPixel)!,
                    PlanarConfiguration: exifImageDir.GetString(ExifDirectoryBase.TagPlanarConfiguration)!,
                    YCbCrCoefficients: exifImageDir.GetString(ExifDirectoryBase.TagYCbCrCoefficients)!,
                    YCbCrSubSampling: exifImageDir.GetString(ExifDirectoryBase.TagYCbCrSubsampling)!,
                    YCbCrPositioning: exifImageDir.GetString(ExifDirectoryBase.TagYCbCrPositioning)!,
                    ReferenceBlackWhite: exifImageDir.GetString(ExifDirectoryBase.TagReferenceBlackWhite)!
                );
            }

            // 解析 File 目录
            var fileDir = directories.OfType<FileMetadataDirectory>().FirstOrDefault();
            if (fileDir != null)
            {
                arwMetas.File = new SonyFileMeta(
                    FileName: fileDir.GetString(FileMetadataDirectory.TagFileName)!,
                    FileSize: fileDir.GetString(FileMetadataDirectory.TagFileSize)!,
                    FileModifiedDate: fileDir.GetString(FileMetadataDirectory.TagFileModifiedDate)!
                );
            }

            return arwMetas;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取EXIF信息出错: {ex.Message}");
            return new SonyArwMetas(); // 返回空对象
        }
    }
}
