using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents scale parameter
/// </summary>
public class SetPtsArgument : IVideoFilterArgument
{
    public readonly string Expression;

    public SetPtsArgument(string expression)
    {
        Expression = expression;
    }

    public string Key { get; } = "setpts";
    public string Value => Expression;
}
