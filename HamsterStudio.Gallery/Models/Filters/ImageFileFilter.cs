namespace HamsterStudio.Gallery.Models.Filters;

class ImageFileFilter : IFileManagerFilter
{
    public static readonly List<string> Extensions =
    [
        ".jpeg", ".jpg", ".png", ".bmp", ".webp"
    ];

    public bool Test(string filename)
    {
        return Extensions.Any(ext => filename.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase));
    }

}
