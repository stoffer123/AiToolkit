using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    /// <summary>
    /// A provider-neutral request to a large language model.
    /// </summary>
    /// <remarks>
    /// Optional parameters left as <see langword="null"/> use the provider's default.
    /// Not every model supports every parameter; the <see cref="IAiService"/>
    /// implementation validates the request against the selected model.
    /// </remarks>
    /// <param name="Model">The provider's model ID, for example <c>"gpt-4o-mini"</c>.</param>
    /// <param name="Instructions">
    /// System-level instructions that set the model's role and rules for the whole conversation.
    /// </param>
    /// <param name="Messages">The conversation, in order. Must contain at least one message.</param>
    /// <param name="Temperature">
    /// Controls randomness. Lower values give more focused and predictable output.
    /// The allowed range depends on the provider (0–2 for OpenAI, 0–1 for Anthropic).
    /// Typically not supported by reasoning models. Avoid setting it together with <paramref name="TopP"/>.
    /// </param>
    /// <param name="TopP">
    /// Nucleus sampling between 0 and 1: the model only considers tokens within this
    /// cumulative probability mass. Typically not supported by reasoning models.
    /// </param>
    /// <param name="MaxOutputTokens">
    /// Upper limit on generated tokens. For reasoning models this includes reasoning tokens,
    /// so set it generously to avoid empty or cut-off responses.
    /// </param>
    /// <param name="ReasoningEffort">
    /// How much the model should reason before answering. Only supported by reasoning models.
    /// </param>
    public record AiRequest(
        string Model,
        string? Instructions,
        IReadOnlyList<ChatMessage> Messages,
        float? Temperature = null,
        float? TopP = null,
        int? MaxOutputTokens = null,
        ReasoningEffort? ReasoningEffort = null
        );
}
