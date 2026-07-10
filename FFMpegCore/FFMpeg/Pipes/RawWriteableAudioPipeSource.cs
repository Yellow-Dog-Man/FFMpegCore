using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Nito.AsyncEx;

namespace FFMpegCore.Pipes;

/// <summary>
///     Implementation of <see cref="IPipeSource" /> for a raw audio stream that is gathered from <see cref="IEnumerator{float}" />.
///     It is the user's responsibility to make sure the enumerated samples match the configuration provided to this pipe.
/// </summary>
public class RawWriteableAudioPipeSource : IPipeSource
{
    public int AvailableSamples => _count;
    public int FreeSpace => (_buffer?.Length ?? 0) - _count;
    
    float[] _buffer;
    int _offset;
    int _count;
    
    object _lock = new();

    AsyncAutoResetEvent _event;
    
    public RawWriteableAudioPipeSource(int defaultBufferSize = 16384)
    {
        _buffer = new float[defaultBufferSize];
        _event = new AsyncAutoResetEvent();
    }

    public string Format => BitConverter.IsLittleEndian ? "f32le" : "f32be";
    public uint SampleRate { get; set; } = 8000;
    public uint Channels { get; set; } = 1;
    public bool UseWallClock { get; set; } = false;

    public string GetStreamArguments()
    {
        var args = $"-f {Format} -ar {SampleRate} -ac {Channels}";

        if (UseWallClock)
            args += " -use_wallclock_as_timestamps 1";

        return args;
    }

    public void WriteAudioData(ReadOnlySpan<float> data)
    {
        lock (_lock)
        {
            // Ensure the buffer has enough free space to accomodate the new data
            if (data.Length > FreeSpace)
            {
                // The buffer is too small, we need to expand it
                var newSize = Math.Max(_buffer.Length * 2, _count + data.Length);
                var newBuffer = new float[newSize];
                var newBufferSlice = newBuffer.AsSpan();
                
                // Copy data from the existing buffer
                int consumed = 0;

                while (_count > 0)
                {
                    var read = Read(newBufferSlice);

                    consumed += read;
                    newBufferSlice = newBufferSlice.Slice(read);
                }

                // Assign the new buffer
                _offset = 0;
                _count = consumed;
                _buffer = newBuffer;
            }
            
            // Write the new data
            while (data.Length > 0)
            {
                // Compute new write position
                int writePos = _offset + _count;
                int toWrite = Math.Min(_buffer.Length - writePos, data.Length);

                if (toWrite <= 0)
                {
                    // This means we need to wrap around to the start of the buffer
                    writePos = 0;
                    // Write the rest of the data 
                    // We already made sure earlier in the method that there's enough space to accomodate all the data
                    // so this will not overwrite anything
                    toWrite = data.Length; 
                }

                // Copy the data to the buffer
                data.Slice(0, toWrite).CopyTo(_buffer.AsSpan(writePos, toWrite));

                // Update the buffer to remainder slice
                data = data.Slice(toWrite);

                // Increase amount of data in the buffer by how much we just wrote
                _count += toWrite;
            }
        }
        
        // Wake up the reading thread to consume the data as soon as possible 
        _event.Set();
    }

    int Read(Span<float> targetBuffer)
    {
        if(_count == 0)
            return 0;

        var slice = _buffer.AsSpan(_offset);

        var maxRead = Math.Min(targetBuffer.Length, _count);
        
        if(slice.Length > maxRead)
            slice = slice.Slice(0, maxRead);

        // Advance the read position
        _offset += slice.Length;
        _count -= slice.Length;

        // Wrap around
        _offset %= _buffer.Length;
        
        // Copy the data to the target buffer
        slice.CopyTo(targetBuffer.Slice(0, slice.Length));

        return slice.Length;
    }

    public async Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        // We keep a local buffer in this method for reading the data from the ciruclar buffer
        // This is so we don't have to keep clock on the ciruclar buffer while IO is performing
        // We also keep this one as byte buffer, because that's what the output stream accepts
        var buffer = new byte[16384 * sizeof(float)];
        
        while (!cancellationToken.IsCancellationRequested)
        {
            int read;

            lock (_lock)
                read = Read(MemoryMarshal.Cast<byte, float>(buffer.AsSpan()));

            if (read > 0)
                await outputStream.WriteAsync(buffer, 0, read * sizeof(float), cancellationToken).ConfigureAwait(false);
            else
            {
                // We didn't get any data, so we will wait for an event to get new data to write
                // If we read data, we will try again immediately, to fully drain the buffer
                await _event.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
