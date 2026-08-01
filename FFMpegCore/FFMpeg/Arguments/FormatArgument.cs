using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents hwdownload parameter
/// </summary>
public class FormatArgument : IVideoFilterArgument
{
    public readonly string Format;

    public FormatArgument(string format)
    {
        Format = format;
    }

    public string Key { get; } = "format";
    public string Value => Format;
}
