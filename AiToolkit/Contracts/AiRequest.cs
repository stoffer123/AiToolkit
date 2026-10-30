using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
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
