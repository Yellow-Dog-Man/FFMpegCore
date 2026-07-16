namespace FFMpegCore.Arguments;

/// <summary>
///     Represents parameter of copy parameter
///     Defines if channel (audio, video or both) should be copied to output file
/// </summary>
public class CopyTimestampsArgument : IArgument
{
    public CopyTimestampsArgument()
    {
        
    }

    public string Text => "-copyts";
}
