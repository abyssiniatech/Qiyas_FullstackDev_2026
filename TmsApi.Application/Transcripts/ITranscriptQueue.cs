namespace TmsApi.Application.Transcripts;

public interface ITranscriptQueue
{
    ValueTask EnqueueAsync(
        TranscriptRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<TranscriptRequest> DequeueAsync(
        CancellationToken cancellationToken);
}
