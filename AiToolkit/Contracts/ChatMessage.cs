using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    /// <summary>
    /// A single message in a conversation with the model.
    /// </summary>
    /// <param name="Role">Who wrote the message.</param>
    /// <param name="Content">The text of the message.</param>
    public record ChatMessage(ChatRole Role, string Content);
}
