using AiToolkit.Contracts;
using OpenAI.Responses;

namespace AiToolkit.Providers.OpenAI;

/// <summary>
/// <see cref="IAiService"/> implementation for OpenAI, using the Responses API.
/// </summary>
/// <remarks>
/// Supported models and their capabilities are listed in <see cref="OpenAiModels"/>.
/// Requests are validated against the selected model before they are sent.
/// </remarks>
public class OpenAiService : IAiService
{
    /// <inheritdoc/>
    public IReadOnlyList<ModelInfo> SupportedModels => OpenAiModels.All;

    private readonly ResponsesClient _client;

    /// <summary>
    /// Creates a service that sends requests through the given client.
    /// </summary>
    /// <param name="client">
    /// An authenticated OpenAI Responses client. Create it once and reuse it
    /// for the lifetime of the application.
    /// </param>
    public OpenAiService(ResponsesClient client)
    {
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<string> GetResponseAsync(AiRequest request, CancellationToken ct = default)
    {
        Validate(request);
        CreateResponseOptions options = ToOptions(request);

        ResponseResult response = await _client.CreateResponseAsync(options, ct);

        if (response.Status == ResponseStatus.Incomplete)
            throw new InvalidOperationException(
                $"The response was incomplete: {response.IncompleteStatusDetails?.Reason}");

        return response.GetOutputText();
    }

    private static void Validate(AiRequest request)
    {
        var model = OpenAiModels.Find(request.Model)
            ?? throw new ArgumentException(
                $"Unknown OpenAI model '{request.Model}'. See OpenAiModels.All for supported models.");

        if (!model.SupportsSampling && (request.Temperature is not null || request.TopP is not null))
            throw new NotSupportedException($"{model.Id} does not support Temperature or TopP.");

        if (request.Temperature > 2f)
            throw new ArgumentOutOfRangeException(nameof(request.Temperature), "OpenAI requires Temperature to be between 0 and 2.");

        if (!model.SupportsReasoning && request.ReasoningEffort is not null)
            throw new NotSupportedException($"{model.Id} is not a reasoning model and does not support ReasoningEffort.");
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