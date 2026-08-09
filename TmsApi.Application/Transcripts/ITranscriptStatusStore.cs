namespace TmsApi.Application.Transcripts;

public interface ITranscriptStatusStore
{
    Task<TranscriptStatus?> GetAsync(
        string reportId,
        CancellationToken cancellationToken = default);

    Task<TranscriptStatus?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string idempotencyKey,
        TranscriptStatus status,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TranscriptStatus status,
        CancellationToken cancellationToken = default);
}