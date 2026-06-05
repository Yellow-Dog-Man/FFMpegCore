using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFMpegCore.Pipes;

/// <summary>
///     Implementation of <see cref="IPipeSource" /> for a raw audio stream that is gathered from <see cref="IEnumerator{float}" />.
///     It is the user's responsibility to make sure the enumerated samples match the configuration provided to this pipe.
/// </summary>
public class RawFloatAudioPipeSource : IPipeSource
{
    private readonly IEnumerator<float> _sampleEnumerator;

    public RawFloatAudioPipeSource(IEnumerator<float> sampleEnumerator)
    {
        _sampleEnumerator = sampleEnumerator;
    }

    public RawFloatAudioPipeSource(IEnumerable<float> sampleEnumerator)
        : this(sampleEnumerator.GetEnumerator())
    {
    }

    public string Format => BitConverter.IsLittleEndian ? "f32le" : "f32be";
    public uint SampleRate { get; set; } = 8000;
    public uint Channels { get; set; } = 1;

    public string GetStreamArguments()
    {
        return $"-f {Format} -ar {SampleRate} -ac {Channels}";
    }

    public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        var buffer = new byte[sizeof(float)];
        
        while (_sampleEnumerator.MoveNext() && !cancellationToken.IsCancellationRequested)
        {
            var sample = _sampleEnumerator.Current;
            var bits = Unsafe.As<float, uint>(ref sample);
            
            buffer[0] = (byte)(bits & 0xFF);
            buffer[1] = (byte)((bits >> 8) & 0xFF);
            buffer[2] = (byte)((bits >> 16) & 0xFF);
            buffer[3] = (byte)((bits >> 24) & 0xFF);
            
            await outputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }
}
