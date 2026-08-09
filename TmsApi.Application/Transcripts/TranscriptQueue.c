using System.Threading.Channels;

namespace TmsApi.Application.Transcripts;

public sealed class TranscriptQueue : ITranscriptQueue
{
    private readonly Channel<TranscriptRequest> _queue =
        Channel.CreateUnbounded<TranscriptRequest>();

    public async ValueTask EnqueueAsync(
        TranscriptRequest request,
        CancellationToken cancellationToken = default)
    {
        await _queue.Writer.WriteAsync(request, cancellationToken);
    }

    public async ValueTask<TranscriptRequest> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}