using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents hwdownload parameter
/// </summary>
public class HardwareDownloadArgument : IVideoFilterArgument
{
    public HardwareDownloadArgument()
    {
    }

    public string Key { get; } = "";
    public string Value => "hwdownload";
}
