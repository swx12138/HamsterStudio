using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MdBlock = Markdig.Syntax.Block;
using MdInline = Markdig.Syntax.Inlines.Inline;

namespace HamsterStudio.ChatBox.Services;

/// <summary>把 Markdown 文本渲染为 WPF FlowDocument。</summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public static FlowDocument Render(string markdown)
    {
        var document = Markdown.Parse(markdown ?? string.Empty, Pipeline);

        var flow = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            Foreground = Brushes.Black,
            PagePadding = new Thickness(0),
            LineHeight = 20,
        };

        foreach (var block in document)
            RenderBlock(flow.Blocks, block);

        return flow;
    }

    private static void RenderBlock(BlockCollection blocks, MdBlock block)
    {
        switch (block)
        {
            case HeadingBlock heading:
            {
                var paragraph = new Paragraph { Margin = new Thickness(0, 8, 0, 2) };
                (paragraph.FontSize, paragraph.FontWeight) = heading.Level switch
                {
                    1 => (24.0, FontWeights.Bold),
                    2 => (20.0, FontWeights.Bold),
                    3 => (17.0, FontWeights.Bold),
                    4 => (15.0, FontWeights.Bold),
                    _ => (14.0, FontWeights.Bold),
                };
                RenderInlines(paragraph.Inlines, heading.Inline);
                blocks.Add(paragraph);
                break;
            }

            case CodeBlock codeBlock:
            {
                var paragraph = new Paragraph
                {
                    FontFamily = new FontFamily("Consolas, Courier New"),
                    FontSize = 13,
                    Background = new SolidColorBrush(Color.FromRgb(0xF4, 0xF4, 0xF4)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0)),
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(8),
                    Margin = new Thickness(0, 4, 0, 4),
                };
                paragraph.Inlines.Add(new Run(codeBlock.Lines.ToString()));
                blocks.Add(paragraph);
                break;
            }

            case ParagraphBlock paragraphBlock:
            {
                var paragraph = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                RenderInlines(paragraph.Inlines, paragraphBlock.Inline);
                blocks.Add(paragraph);
                break;
            }

            case ListBlock listBlock:
            {
                RenderList(blocks, listBlock);
                break;
            }

            case QuoteBlock quote:
            {
                var section = new Section
                {
                    Margin = new Thickness(0, 4, 0, 4),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0xD0, 0xD0)),
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding = new Thickness(8, 0, 0, 0),
                };
                foreach (var child in quote)
                    RenderBlock(section.Blocks, child);
                blocks.Add(section);
                break;
            }

            case ThematicBreakBlock:
            {
                blocks.Add(new Paragraph(new Run("────────"))
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0)),
                    Margin = new Thickness(0, 8, 0, 8),
                });
                break;
            }

            default:
                break;
        }
    }

    private static void RenderList(BlockCollection blocks, ListBlock list)
    {
        var wpfList = new List
        {
            MarkerStyle = list.IsOrdered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc,
            Margin = new Thickness(18, 2, 0, 2),
            Padding = new Thickness(4, 0, 0, 0),
        };

        foreach (var item in list)
        {
            if (item is not ListItemBlock listItem)
                continue;

            var wpfItem = new ListItem();
            foreach (var child in listItem)
            {
                if (child is ListBlock nested)
                    RenderList(wpfItem.Blocks, nested);
                else
                    RenderBlock(wpfItem.Blocks, child);
            }
            wpfList.ListItems.Add(wpfItem);
        }

        blocks.Add(wpfList);
    }

    private static void RenderInlines(InlineCollection inlines, ContainerInline? container)
    {
        if (container is null)
            return;

        foreach (var inline in container)
            RenderInline(inlines, inline);
    }

    private static void RenderInline(InlineCollection inlines, MdInline inline)
    {
        switch (inline)
        {
            case LiteralInline literal:
                inlines.Add(new Run(literal.Content.ToString()));
                break;

            case CodeInline code:
                inlines.Add(new Run(code.Content)
                {
                    FontFamily = new FontFamily("Consolas, Courier New"),
                    Background = new SolidColorBrush(Color.FromRgb(0xEF, 0xEF, 0xEF)),
                });
                break;

            case LineBreakInline:
                inlines.Add(new LineBreak());
                break;

            case EmphasisInline emphasis:
            {
                var span = new Span();
                RenderInlines(span.Inlines, emphasis);
                if (emphasis.DelimiterCount == 2)
                    span.FontWeight = FontWeights.Bold;
                else if (emphasis.DelimiterCount >= 3)
                {
                    span.FontWeight = FontWeights.Bold;
                    span.FontStyle = FontStyles.Italic;
                }
                else
                    span.FontStyle = FontStyles.Italic;
                inlines.Add(span);
                break;
            }

            case LinkInline link:
            {
                var hyperlink = new Hyperlink
                {
                    Foreground = new SolidColorBrush(Color.FromRgb(0x1E, 0x6F, 0xC5)),
                };
                RenderInlines(hyperlink.Inlines, link);
                if (!string.IsNullOrWhiteSpace(link.Url))
                {
                    hyperlink.NavigateUri = new Uri(link.Url, UriKind.RelativeOrAbsolute);
                    hyperlink.ToolTip = link.Url;
                    hyperlink.RequestNavigate += (_, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
                        }
                        catch
                        {
                            // 忽略打开链接失败
                        }
                        e.Handled = true;
                    };
                }
                inlines.Add(hyperlink);
                break;
            }

            case ContainerInline container:
                RenderInlines(inlines, container);
                break;

            default:
                break;
        }
    }
}
