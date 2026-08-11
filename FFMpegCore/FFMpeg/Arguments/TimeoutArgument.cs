namespace FFMpegCore.Arguments;

/// <summary>
///     Represents timeout parameter
///     How many microseconds will ffmpeg wait before timing out network connections
/// </summary>
public class TimeoutArgument : IArgument
{
    public readonly int TimeoutMicroseconds;

    public TimeoutArgument(int timeoutMicrosconds)
    {
        TimeoutMicroseconds = timeoutMicrosconds;
    }

    public TimeoutArgument(TimeSpan timespan)
    {
        TimeoutMicroseconds = (int)(timespan.TotalMilliseconds * 1000);
    }

    public string Text => $"-timeout {TimeoutMicroseconds}";
}
