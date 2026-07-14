using System.IO;
using Microsoft.Extensions.Logging;

namespace FarmBooks.UI.Infrastructure;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logDirectory;
    private readonly object _syncRoot = new();
    private bool _disposed;

    public FileLoggerProvider(string logDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logDirectory);

        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
    }

    public ILogger CreateLogger(string categoryName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return new FileLogger(categoryName, _logDirectory, _syncRoot);
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logDirectory;
        private readonly object _syncRoot;

        public FileLogger(string categoryName, string logDirectory, object syncRoot)
        {
            _categoryName = categoryName;
            _logDirectory = logDirectory;
            _syncRoot = syncRoot;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (!IsEnabled(logLevel))
                return;

            ArgumentNullException.ThrowIfNull(formatter);

            var timestamp = DateTimeOffset.Now;
            var message = formatter(state, exception);

            var entry = $"""
                {timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{logLevel}] {_categoryName}
                {message}
                {FormatException(exception)}

                """;

            var logPath = Path.Combine(_logDirectory, $"farmbooks-{timestamp:yyyy-MM-dd}.log");

            lock (_syncRoot)
            {
                File.AppendAllText(logPath, entry);
            }
        }

        private static string FormatException(Exception? exception)
        {
            return exception is null ? string.Empty : exception.ToString();
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose() { }
    }
}
