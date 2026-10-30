using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    public record ChatMessage(ChatRole Role, string Content);
}
