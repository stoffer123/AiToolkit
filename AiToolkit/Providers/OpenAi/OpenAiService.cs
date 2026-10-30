using AiToolkit.Contracts;
using OpenAI.Responses;
#pragma warning disable OPENAI001

namespace AiToolkit.Providers.OpenAi;

public class OpenAiService : IAiService
{
    private record ModelCapabilities(bool SupportsSampling, bool SupportsReasoning);

    private static readonly Dictionary<string, ModelCapabilities> Models = new()
    {
        ["gpt-4o-mini"] = new(SupportsSampling: true, SupportsReasoning: false),
        ["gpt-4.1"] = new(SupportsSampling: true, SupportsReasoning: false),
        ["gpt-4.1-mini"] = new(SupportsSampling: true, SupportsReasoning: false),
        ["gpt-5"] = new(SupportsSampling: false, SupportsReasoning: true),
        ["gpt-5-mini"] = new(SupportsSampling: false, SupportsReasoning: true),
    };

    private readonly ResponsesClient _client;

    public OpenAiService(ResponsesClient client)
    {
        _client = client;
    }

    public async Task<string> GetResponseAsync(AiRequest request, CancellationToken ct = default)
    {
        Validate(request);
        CreateResponseOptions options = ToOptions(request);

        ResponseResult response = await _client.CreateResponseAsync(options, ct);

        if (response.Status == ResponseStatus.Incomplete)
            throw new InvalidOperationException(
                $"Svaret blev afbrudt: {response.IncompleteStatusDetails?.Reason}");

        return response.GetOutputText();
    }

    private static void Validate(AiRequest request)
    {
        if (!Models.TryGetValue(request.Model, out var caps))
            throw new ArgumentException($"Ukendt OpenAI-model '{request.Model}'. Tilføj den til Models-tabellen.");

        if (!caps.SupportsSampling && (request.Temperature is not null || request.TopP is not null))
            throw new NotSupportedException($"{request.Model} understøtter ikke Temperature/TopP.");

        if (request.Temperature > 2f)
            throw new ArgumentOutOfRangeException(nameof(request.Temperature), "OpenAI tillader Temperature mellem 0 og 2.");

        if (!caps.SupportsReasoning && request.ReasoningEffort is not null)
            throw new NotSupportedException($"{request.Model} er ikke en reasoning-model.");
    }

    private static CreateResponseOptions ToOptions(AiRequest request)
    {
        var options = new CreateResponseOptions
        {
            Model = request.Model,
            Instructions = request.Instructions,
            Temperature = request.Temperature,
            TopP = request.TopP,
            MaxOutputTokenCount = request.MaxOutputTokens,
        };

        if (request.ReasoningEffort is { } effort)
        {
            options.ReasoningOptions = new ResponseReasoningOptions
            {
                ReasoningEffortLevel = effort switch
                {
                    ReasoningEffort.Low => ResponseReasoningEffortLevel.Low,
                    ReasoningEffort.Medium => ResponseReasoningEffortLevel.Medium,
                    _ => ResponseReasoningEffortLevel.High,
                }
            };
        }

        foreach (var msg in request.Messages)
        {
            options.InputItems.Add(msg.Role switch
            {
                ChatRole.Assistant => ResponseItem.CreateAssistantMessageItem(msg.Content),
                _ => ResponseItem.CreateUserMessageItem(msg.Content),
            });
        }

        return options;
    }
}