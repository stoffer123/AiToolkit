using AiToolkit.Contracts;

namespace AiToolkit.Providers.OpenAI;

/// <summary>
/// OpenAI models supported by <see cref="OpenAiService"/>.
/// Use the constants with <c>PromptBuilder</c> to avoid typos in model IDs.
/// </summary>
public static class OpenAiModels
{
    /// <summary>Efficient, low-cost reasoning model for focused, high-volume tasks.</summary>
    public const string Gpt6Luna = "gpt-6-luna";

    /// <summary>GPT-6 reasoning model.</summary>
    public const string Gpt6Sol = "gpt-6-sol";

    /// <summary>Flagship reasoning model for complex professional work.</summary>
    public const string Gpt55 = "gpt-5.5";

    /// <summary>Non-reasoning model. Deprecated; removed 2027-01-14.</summary>
    public const string Gpt41 = "gpt-4.1";

    /// <summary>Non-reasoning model. Deprecated; removed 2027-01-14.</summary>
    public const string Gpt41Mini = "gpt-4.1-mini";

    /// <summary>Non-reasoning model. Deprecated; removed 2027-01-14.</summary>
    public const string Gpt4oMini = "gpt-4o-mini";

    private static readonly DateOnly Gpt4Shutdown = new(2027, 1, 14);

    /// <summary>All supported OpenAI models.</summary>
    public static IReadOnlyList<ModelInfo> All { get; } =
    [
        new(Gpt6Luna,  SupportsSampling: false, SupportsReasoning: true),
        new(Gpt6Sol,   SupportsSampling: false, SupportsReasoning: true),
        new(Gpt55,     SupportsSampling: false, SupportsReasoning: true),
        new(Gpt41,     SupportsSampling: true,  SupportsReasoning: false, Gpt4Shutdown),
        new(Gpt41Mini, SupportsSampling: true,  SupportsReasoning: false, Gpt4Shutdown),
        new(Gpt4oMini, SupportsSampling: true,  SupportsReasoning: false, Gpt4Shutdown),
    ];

    /// <summary>Finds a model by ID, or returns <see langword="null"/> if it is not supported.</summary>
    public static ModelInfo? Find(string id) =>
        All.FirstOrDefault(m => m.Id == id);
}