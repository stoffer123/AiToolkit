using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    /// <summary>
    /// How much effort a reasoning model spends thinking before it answers.
    /// </summary>
    /// <remarks>
    /// Higher effort usually gives better answers on complex tasks, at the cost of
    /// more tokens and slower responses. Each provider maps these levels to its own
    /// mechanism, for example an effort setting or a token budget.
    /// </remarks>
    public enum ReasoningEffort
    {
        /// <summary>Minimal reasoning. Fastest and cheapest.</summary>
        Low,
        /// <summary>Balanced reasoning. A sensible default.</summary>
        Medium,
        /// <summary>Thorough reasoning for complex tasks. Slowest and most expensive.</summary>
        High
    }
}
