using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    /// <summary>
    /// The author of a <see cref="ChatMessage"/>.
    /// </summary>
    /// <remarks>
    /// System instructions are not a role here; set them with <see cref="AiRequest.Instructions"/>.
    /// </remarks>
    public enum ChatRole
    {
        /// <summary>A message from the user.</summary>
        User,
        /// <summary>
        /// A previous reply from the model. Use it to include earlier turns
        /// when continuing a conversation.
        /// </summary>
        Assistant
    }
}
