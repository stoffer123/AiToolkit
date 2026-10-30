using AiToolkit.Contracts;

namespace AiToolkit.Builders;

public class PromptBuilder
{
    private readonly string _model;
    private string? _instructions;
    private readonly List<ChatMessage> _messages = new();
    private float? _temperature, _topP;
    private int? _maxOutputTokens;
    private ReasoningEffort? _reasoningEffort;

    public PromptBuilder(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model skal angives.", nameof(model));
        _model = model;
    }

    public PromptBuilder WithInstructions(string text)
    {
        _instructions = text;
        return this;
    }

    public PromptBuilder AddUserMessage(string text)
    {
        _messages.Add(new ChatMessage(ChatRole.User, text));
        return this;
    }

    public PromptBuilder AddAssistantMessage(string text)
    {
        _messages.Add(new ChatMessage(ChatRole.Assistant, text));
        return this;
    }

    public PromptBuilder WithTemperature(float t)
    {
        if (t < 0f)
            throw new ArgumentOutOfRangeException(nameof(t), "Temperature kan ikke være negativ.");
        _temperature = t;
        return this;
    }

    public PromptBuilder WithTopP(float p)
    {
        if (p is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(p), "TopP skal være mellem 0 og 1.");
        _topP = p;
        return this;
    }

    public PromptBuilder WithMaxOutputTokens(int n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n), "MaxOutputTokens skal være større end 0.");
        _maxOutputTokens = n;
        return this;
    }

    public PromptBuilder WithReasoningEffort(ReasoningEffort effort)
    {
        _reasoningEffort = effort;
        return this;
    }

    public AiRequest Build()
    {
        if (_messages.Count == 0)
            throw new InvalidOperationException("Der skal være mindst én besked.");

        return new AiRequest(
            Model: _model,
            Instructions: _instructions,
            Messages: _messages.ToList(),
            Temperature: _temperature,
            TopP: _topP,
            MaxOutputTokens: _maxOutputTokens,
            ReasoningEffort: _reasoningEffort);
    }
}