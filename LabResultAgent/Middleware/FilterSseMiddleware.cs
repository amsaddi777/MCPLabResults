using System.Text;
using System.Text.Json;

namespace LabResultAgent.Middleware;

/// <summary>
/// Middleware that intercepts Server-Sent Events (SSE) responses and filters
/// out events where the JSON payload contains a tool message (role == "tool").
/// This preserves the tool data in the agent context but prevents the frontend
/// from seeing intermediate tool messages.
/// </summary>
public class FilterSseMiddleware
{
    private readonly RequestDelegate _next;

    public FilterSseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to AG-UI root path and SSE responses
        if (context.Request.Path == "/")
        {
            var originalBody = context.Response.Body;
            try
            {
                using var filteringStream = new SseFilteringStream(originalBody);
                context.Response.Body = filteringStream;
                await _next(context);
                // Ensure any buffered data is flushed
                await filteringStream.FlushAsync();
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
        else
        {
            await _next(context);
        }
    }

    private class SseFilteringStream : Stream
    {
        private readonly Stream _inner;
        private readonly StringBuilder _buffer = new();

        public SseFilteringStream(Stream inner)
        {
            _inner = inner;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() => _inner.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var chunk = Encoding.UTF8.GetString(buffer, offset, count);
            _buffer.Append(chunk);

            // Process complete SSE events separated by blank line (\r\n\r\n or \n\n)
            while (true)
            {
                var buf = _buffer.ToString();
                int delim = buf.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                int delimLen = 4;
                if (delim < 0)
                {
                    delim = buf.IndexOf("\n\n", StringComparison.Ordinal);
                    delimLen = 2;
                }

                if (delim < 0)
                    break;

                var eventText = buf.Substring(0, delim + delimLen);
                _buffer.Remove(0, delim + delimLen);

                if (!ShouldFilterEvent(eventText))
                {
                    var bytes = Encoding.UTF8.GetBytes(eventText);
                    await _inner.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                    await _inner.FlushAsync(cancellationToken);
                }
                // else: drop the event
            }
        }

        private bool ShouldFilterEvent(string eventText)
        {
            try
            {
                // Extract data: lines and concatenate
                var lines = eventText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var dataLines = lines.Where(l => l.StartsWith("data:") || l.StartsWith("data: ")).ToArray();
                if (dataLines.Length == 0)
                    return false;

                var data = string.Join('\n', dataLines.Select(l => l.Substring(l.IndexOf(':') + 1).TrimStart()));

                // Quick check: if it doesn't look like JSON, don't filter
                var trimmed = data.TrimStart();
                if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                    return false;

                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;

                // Check common shapes: { message: { role: "tool" } } or { role: "tool" }
                if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.Object)
                {
                    if (msg.TryGetProperty("role", out var role) && role.GetString() == "tool")
                        return true;
                }

                if (root.TryGetProperty("role", out var role2) && role2.GetString() == "tool")
                    return true;
            }
            catch
            {
                // Parsing failed — don't filter
            }

            return false;
        }

        #region NotSupported
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        #endregion
    }
}
