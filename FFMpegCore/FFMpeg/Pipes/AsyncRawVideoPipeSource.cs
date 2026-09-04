using System.Globalization;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Pipes;

/// <summary>
///     Implementation of <see cref="IPipeSource" /> for a raw video stream that is gathered from <see cref="IAsyncEnumerator{IVideoFrame}" />
/// </summary>
public class AsyncRawVideoPipeSource : IPipeSource
{
    private readonly IAsyncEnumerator<IVideoFrame> _framesEnumerator;

    public AsyncRawVideoPipeSource(IAsyncEnumerable<IVideoFrame> framesEnumerator)
    {
        _framesEnumerator = framesEnumerator.GetAsyncEnumerator();
    }

    public string StreamFormat { get; private set; } = null!;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double FrameRate { get; set; } = 25;

    public string GetStreamArguments()
    {
        return $"-f rawvideo -r {FrameRate.ToString(CultureInfo.InvariantCulture)} -pix_fmt {StreamFormat} -s {Width}x{Height}";
    }

    public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        if (_framesEnumerator.Current != null)
        {
            CheckFrameAndThrow(_framesEnumerator.Current);
            await _framesEnumerator.Current.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
        }

        while (await _framesEnumerator.MoveNextAsync().ConfigureAwait(false))
        {
            CheckFrameAndThrow(_framesEnumerator.Current!);
            await _framesEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
        }
    }

    private void CheckFrameAndThrow(IVideoFrame frame)
    {
        if (frame.Width != Width || frame.Height != Height || frame.Format != StreamFormat)
        {
            throw new FFMpegStreamFormatException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                                                                                 $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.Format}\r\n" +
                                                                                 $"Stream format: {Width}x{Height} pix_fmt: {StreamFormat}");
        }
    }
}
