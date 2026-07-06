using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents FPS parameter
/// </summary>
public class FPSArgument : IVideoFilterArgument
{
    public readonly int FPS;

    public FPSArgument(int fps)
    {
        FPS = fps;
    }

    public string Key { get; } = "fps";
    public string Value => FPS.ToString();
}
