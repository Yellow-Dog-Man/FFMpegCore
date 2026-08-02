using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents hwdownload parameter
/// </summary>
public class HardwareUploadArgument : IVideoFilterArgument
{
    public readonly string? Target;

    public HardwareUploadArgument(string? target)
    {
        Target = target;
    }

    public string Key => string.IsNullOrEmpty(Target) ? "" : "hwupload";
    public string Value => string.IsNullOrEmpty(Target) ? "hwdownload" : Target;
}
