using System.Text;
using Seq.Api;

namespace SeqMcpServer.Tests.Integration.Helpers;

internal static class SeqTestDataHelper
{
    public static async Task SeedEventsAsync(string seqUrl)
    {
        var clefLines = new[]
        {
            """{"@t":"2025-01-15T10:00:00.000Z","@l":"Information","@mt":"Application started","Application":"TestApp","Environment":"Production"}""",
            """{"@t":"2025-01-15T10:01:00.000Z","@l":"Warning","@mt":"High memory usage detected","Application":"TestApp","MemoryMB":950}""",
            """{"@t":"2025-01-15T10:02:00.000Z","@l":"Error","@mt":"Unhandled exception in request pipeline","Application":"TestApp","RequestPath":"/api/orders"}""",
            """{"@t":"2025-01-15T10:03:00.000Z","@l":"Information","@mt":"User login successful","Application":"AuthService","UserId":"user-42"}""",
            """{"@t":"2025-01-15T10:04:00.000Z","@l":"Error","@mt":"Database connection timeout","Application":"OrderService","Database":"orders-db"}"""
        };

        var payload = string.Join("\n", clefLines);

        using var httpClient = new HttpClient { BaseAddress = new Uri(seqUrl) };
        var content = new StringContent(payload, Encoding.UTF8, "application/vnd.serilog.clef");
        var response = await httpClient.PostAsync("/api/events/raw", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task WaitForEventsIndexedAsync(SeqConnection connection, int maxRetries = 20)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            var events = await connection.Events.ListAsync(count: 1);
            if (events.Count > 0)
                return;

            await Task.Delay(500);
        }

        throw new TimeoutException("Seq did not index events within the expected time.");
    }

    public static async Task SeedSignalAsync(SeqConnection connection)
    {
        var signal = await connection.Signals.TemplateAsync();
        signal.Title = "Errors";
        signal.OwnerId = null;
        signal.Filters.Add(new Seq.Api.Model.Shared.DescriptiveFilterPart
        {
            Filter = "@Level = 'Error'"
        });
        await connection.Signals.AddAsync(signal);
    }
}
