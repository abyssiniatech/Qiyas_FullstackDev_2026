using System.Collections.Concurrent;

namespace TmsApi.Application.Transcripts;

public sealed class InMemoryTranscriptStatusStore : ITranscriptStatusStore
{
    private readonly ConcurrentDictionary<string, TranscriptStatus> _statuses = new();

    private readonly ConcurrentDictionary<string, string> _idempotencyKeys = new();

    public Task<TranscriptStatus?> GetAsync(
        string reportId,
        CancellationToken cancellationToken = default)
    {
        _statuses.TryGetValue(reportId, out var status);

        return Task.FromResult(status);
    }

    public Task<TranscriptStatus?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        if (_idempotencyKeys.TryGetValue(idempotencyKey, out var reportId) &&
            _statuses.TryGetValue(reportId, out var status))
        {
            return Task.FromResult<TranscriptStatus?>(status);
        }

        return Task.FromResult<TranscriptStatus?>(null);
    }

    public Task CreateAsync(
        string idempotencyKey,
        TranscriptStatus status,
        CancellationToken cancellationToken = default)
    {
        _statuses[status.ReportId] = status;
        _idempotencyKeys[idempotencyKey] = status.ReportId;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(
        TranscriptStatus status,
        CancellationToken cancellationToken = default)
    {
        _statuses[status.ReportId] = status;

        return Task.CompletedTask;
    }
}
