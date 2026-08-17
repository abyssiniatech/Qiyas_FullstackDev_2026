using System.Threading.Tasks;

namespace TmsApi.Application.Notifications;

public class SignalRTranscriptNotificationService : ITranscriptNotificationService
{
    public Task NotifyTranscriptReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl)
    {
        return Task.CompletedTask;
    }
}
