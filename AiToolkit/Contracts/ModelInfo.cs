namespace AiToolkit.Contracts;

/// <summary>
/// Describes a model supported by an <see cref="IAiService"/> and which parameters it accepts.
/// </summary>
/// <param name="Id">The provider's model ID, as passed to <c>PromptBuilder</c>.</param>
/// <param name="SupportsSampling">Whether the model accepts Temperature and TopP.</param>
/// <param name="SupportsReasoning">Whether the model accepts ReasoningEffort.</param>
/// <param name="ShutdownDate">
/// The date the provider removes the model, or <see langword="null"/> if none is announced.
/// </param>
public record ModelInfo(
    string Id,
    bool SupportsSampling,
    bool SupportsReasoning,
    DateOnly? ShutdownDate = null)
{
    /// <summary>Whether the provider has announced a shutdown date for the model.</summary>
    public bool IsDeprecated => ShutdownDate is not null;
}