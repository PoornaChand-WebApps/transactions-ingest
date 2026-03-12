using Microsoft.Extensions.Logging;

namespace CodingExerciseTransactions.Utils
{
    public class Messages
    {
        private readonly ILogger<Messages> _Logger;
        public Messages(ILogger<Messages> logger)
        {
            _Logger = logger;
        }

    public void LogInformation(string message) => _Logger.LogInformation(message);
    public void LogWarning(string message) => _Logger.LogWarning(message);
    public void LogError(string message) => _Logger.LogError(message);
        }
    }