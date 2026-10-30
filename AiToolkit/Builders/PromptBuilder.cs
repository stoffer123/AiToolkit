using AiToolkit.Contracts;

namespace AiToolkit.Builders;

/// <summary>
/// Fluent builder for creating a provider-neutral <see cref="AiRequest"/>.
/// </summary>
/// <remarks>
/// The builder performs validation that applies to all providers. Provider-specific
/// rules, such as which models support sampling or reasoning, are checked by the
/// <see cref="IAiService"/> implementation when the request is sent.
/// </remarks>
/// <example>
/// <code>
/// AiRequest request = new PromptBuilder("gpt-6-luna")
///     .WithInstructions("You are a helpful assistant.")
///     .WithReasoningEffort(ReasoningEffort.Low)
///     .AddUserMessage("Hello!")
///     .Build();
/// </code>
/// </example>
public class PromptBuilder
{
    private readonly string _model;
    private string? _instructions;
    private readonly List<ChatMessage> _messages = new();
    private float? _temperature, _topP;
    private int? _maxOutputTokens;
    private ReasoningEffort? _reasoningEffort;

    /// <summary>
    /// Creates a builder for a request to the specified model.
    /// </summary>
    /// <param name="model">The provider's model ID, for example <c>"gpt-6-luna"</c>.</param>
    /// <exception cref="ArgumentException"><paramref name="model"/> is null, empty or whitespace.</exception>
    public PromptBuilder(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("A model must be specified.", nameof(model));
        _model = model;
    }

    /// <summary>
    /// Sets system-level instructions that define the model's role and rules for the whole conversation.
    /// </summary>
    /// <param name="text">The instructions. Replaces any previously set instructions.</param>
    /// <returns>The same builder, for chaining.</returns>
    public PromptBuilder WithInstructions(string text)
    {
        _instructions = text;
        return this;
    }

    /// <summary>
    /// Adds a message from the user to the conversation.
    /// </summary>
    /// <param name="text">The message text.</param>
    /// <returns>The same builder, for chaining.</returns>
    public PromptBuilder AddUserMessage(string text)
    {
        _messages.Add(new ChatMessage(ChatRole.User, text));
        return this;
    }

    /// <summary>
    /// Adds a previous reply from the model to the conversation.
    /// </summary>
    /// <remarks>
    /// Use this to include earlier turns when continuing a conversation,
    /// for example to ask the model to revise its previous answer.
    /// </remarks>
    /// <param name="text">The text of the earlier model reply.</param>
    /// <returns>The same builder, for chaining.</returns>
    public PromptBuilder AddAssistantMessage(string text)
    {
        _messages.Add(new ChatMessage(ChatRole.Assistant, text));
        return this;
    }

    /// <summary>
    /// Sets the sampling temperature. Lower values give more focused and predictable output.
    /// </summary>
    /// <remarks>
    /// The upper limit depends on the provider and is validated when the request is sent.
    /// Typically not supported by reasoning models. Avoid setting it together with TopP.
    /// </remarks>
    /// <param name="t">The temperature. Must be 0 or greater.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="t"/> is negative.</exception>
    public PromptBuilder WithTemperature(float t)
    {
        if (t < 0f)
            throw new ArgumentOutOfRangeException(nameof(t), "Temperature cannot be negative.");
        _temperature = t;
        return this;
    }

    /// <summary>
    /// Sets nucleus sampling: the model only considers tokens within this cumulative probability mass.
    /// </summary>
    /// <remarks>
    /// Typically not supported by reasoning models. Avoid setting it together with Temperature.
    /// </remarks>
    /// <param name="p">A value between 0 and 1.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="p"/> is outside 0–1.</exception>
    public PromptBuilder WithTopP(float p)
    {
        if (p is < 0f or > 1f)
            throw new ArgumentOutOfRangeException(nameof(p), "TopP must be between 0 and 1.");
        _topP = p;
        return this;
    }

    /// <summary>
    /// Sets the upper limit on tokens the model may generate.
    /// </summary>
    /// <remarks>
    /// For reasoning models this includes reasoning tokens, so set it generously
    /// to avoid empty or incomplete responses.
    /// </remarks>
    /// <param name="n">The maximum number of output tokens. Must be greater than 0.</param>
    /// <returns>The same builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is 0 or negative.</exception>
    public PromptBuilder WithMaxOutputTokens(int n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n), "MaxOutputTokens must be greater than 0.");
        _maxOutputTokens = n;
        return this;
    }

    /// <summary>
    /// Sets how much the model should reason before answering.
    /// </summary>
    /// <remarks>Only supported by reasoning models.</remarks>
    /// <param name="effort">The reasoning effort level.</param>
    /// <returns>The same builder, for chaining.</returns>
    public PromptBuilder WithReasoningEffort(ReasoningEffort effort)
    {
        _reasoningEffort = effort;
        return this;
    }

    /// <summary>
    /// Creates the request from the current builder state.
    /// </summary>
    /// <remarks>
    /// The returned request holds its own copy of the messages, so later changes
    /// to the builder do not affect it.
    /// </remarks>
    /// <returns>A new <see cref="AiRequest"/>.</returns>
    /// <exception cref="InvalidOperationException">No messages have been added.</exception>
    public AiRequest Build()
    {
        if (_messages.Count == 0)
            throw new InvalidOperationException("The request must contain at least one message.");

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