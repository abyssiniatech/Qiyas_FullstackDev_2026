
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Transcripts;

namespace TmsApi.Infrastructure.Workers;

public sealed class TranscriptWorker : BackgroundService
{
    private readonly Channel<TranscriptRequest> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITranscriptStatusStore _statusStore;
    private readonly ILogger<TranscriptWorker> _logger;

    public TranscriptWorker(
        Channel<TranscriptRequest> channel,
        IServiceScopeFactory scopeFactory,
        ITranscriptStatusStore statusStore,
        ILogger<TranscriptWorker> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _statusStore = statusStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Transcript worker started.");

        await foreach (var request in _channel.Reader.ReadAllAsync(ct))
        {
            var reportId = request.ReportId
                ?? throw new InvalidOperationException(
                    "ReportId must be set before queueing.");

            try
            {
                var status = await _statusStore.GetAsync(
                    reportId,
                    ct);

                if (status is null)
                {
                    _logger.LogWarning(
                        "Transcript status not found for {ReportId}.",
                        reportId);

                    continue;
                }

                // Mark as Processing
                await _statusStore.UpdateAsync(
                    status with
                    {
                        State = TranscriptState.Processing,
                        StartedAt = DateTimeOffset.UtcNow
                    },
                    ct);

                _logger.LogInformation(
                    "Generating transcript {ReportId} for student {StudentId}.",
                    reportId,
                    request.StudentId);

                // Create a scope for future EF Core/database work.
                using var scope = _scopeFactory.CreateScope();

                // Real production work goes here:
                //
                // 1. Get EF Core DbContext from scope
                // 2. Load student/transcript data
                // 3. Generate the PDF
                // 4. Save the PDF to blob/file storage

                // Simulate long-running work.
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    ct);

                var downloadUrl =
                    $"/api/v2/transcripts/{reportId}/download";

                // Mark as Completed
                await _statusStore.UpdateAsync(
                    status with
                    {
                        State = TranscriptState.Completed,
                        StartedAt = status.StartedAt
                            ?? DateTimeOffset.UtcNow,
                        CompletedAt = DateTimeOffset.UtcNow,
                        ErrorMessage = null
                    },
                    ct);

                _logger.LogInformation(
                    "Transcript ready: {ReportId}. Download: {DownloadUrl}",
                    reportId,
                    downloadUrl);
            }
            catch (OperationCanceledException)
                when (ct.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Worker shutdown: transcript {ReportId} did not complete.",
                    reportId);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate transcript {ReportId}.",
                    reportId);

                var failedStatus = await _statusStore.GetAsync(
                    reportId,
                    CancellationToken.None);

                if (failedStatus is not null)
                {
                    await _statusStore.UpdateAsync(
                        failedStatus with
                        {
                            State = TranscriptState.Failed,
                            CompletedAt = DateTimeOffset.UtcNow,
                            ErrorMessage = ex.Message
                        },
                        CancellationToken.None);
                }
            }
        }

        _logger.LogInformation(
            "Transcript worker stopped.");
    }
}




