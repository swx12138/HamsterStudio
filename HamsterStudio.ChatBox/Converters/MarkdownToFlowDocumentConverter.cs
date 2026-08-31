using System.Globalization;
using System.Windows.Data;
using HamsterStudio.ChatBox.Services;

namespace HamsterStudio.ChatBox.Converters;

/// <summary>把 Markdown 文本转换为 FlowDocument。</summary>
public class MarkdownToFlowDocumentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string markdown)
            return null;
        return MarkdownRenderer.Render(markdown);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
