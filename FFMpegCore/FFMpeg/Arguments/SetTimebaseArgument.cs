using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents scale parameter
/// </summary>
public class SetTimebaseArgument : IVideoFilterArgument
{
    public readonly string Expression;

    public SetTimebaseArgument(string expression)
    {
        Expression = expression;
    }

    public string Key { get; } = "settb";
    public string Value => Expression;
}
