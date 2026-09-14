using System.IO.Pipes;
using FFMpegCore.Pipes;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents input parameter for a named pipe
/// </summary>
public class InputPipeArgument : PipeArgument, IInputArgument
{
    public readonly IPipeSource Writer;

    public InputPipeArgument(IPipeSource writer, int bufferSize = 0) : base(PipeDirection.Out, 0, bufferSize)
    {
        Writer = writer;
    }

    public override string Text => $"{Writer.GetStreamArguments()} -i \"{PipePath}?pkt_size=16777216\"";

    protected override async Task ProcessDataAsync(CancellationToken token)
    {
        await Pipe.WaitForConnectionAsync(token).ConfigureAwait(false);
        if (!Pipe.IsConnected)
        {
            throw new OperationCanceledException();
        }

        await Writer.WriteAsync(Pipe, token).ConfigureAwait(false);
    }
}
