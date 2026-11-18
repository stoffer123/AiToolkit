using System.Runtime.CompilerServices;
using System.Text;
using AiToolkit.Builders;
using AiToolkit.Contracts;

namespace AiToolkit.Conversations;

/// <summary>
/// A conversation with a model that remembers earlier messages.
/// </summary>
/// <remarks>
/// The model itself has no memory. The conversation keeps the message history
/// and sends all of it, together with the instructions, on every call.
/// The history lives in memory only and is lost when the object is discarded.
/// </remarks>
public class Conversation
{
    private readonly IAiService _ai;
    private readonly string _model;
    private readonly Action<PromptBuilder>? _configure;
    private readonly Func<string?>? _turnInstruction;
    private readonly List<ChatMessage> _messages = new();
    private bool _busy;

    /// <summary>Creates a new, empty conversation.</summary>
    /// <param name="ai">The service used to send messages.</param>
    /// <param name="model">The provider's model ID.</param>
    /// <param name="configure">
    /// Optional settings applied to every request, such as instructions,
    /// reasoning effort or max output tokens. Do not add messages here.
    /// </param>
    /// <param name="turnInstruction">
    /// Optional short instruction sent as a developer message after the newest user message
    /// on every call, for example a reminder about the response language. It is not stored
    /// in <see cref="Messages"/>. Use it for rules the model must follow even when the
    /// conversation pulls in another direction.
    /// </param>
    public Conversation(IAiService ai, string model, Action<PromptBuilder>? configure = null, Func<string?>? turnInstruction = null)
    {
        _ai = ai;
        _model = model;
        _configure = configure;
        _turnInstruction = turnInstruction;
    }

    /// <summary>The messages exchanged so far, in order.</summary>
    public IReadOnlyList<ChatMessage> Messages => _messages;

    /// <summary>
    /// Sends a message, waits for the model's reply and adds both to the history.
    /// </summary>
    /// <param name="text">The user's message.</param>
    /// <param name="ct">Token used to cancel the request.</param>
    /// <returns>The model's reply.</returns>
    /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">A message is already being sent.</exception>
    public async Task<string> SendAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("The message cannot be empty.", nameof(text));
        if (_busy)
            throw new InvalidOperationException("Wait for the current reply before sending another message.");

        _busy = true;
        var userMessage = new ChatMessage(ChatRole.User, text);
        _messages.Add(userMessage);

        try
        {
            string reply = await _ai.GetResponseAsync(BuildRequest(), ct);
            _messages.Add(new ChatMessage(ChatRole.Assistant, reply));
            return reply;
        }
        catch
        {
            // Keep the history consistent: no user message without a reply
            _messages.Remove(userMessage);
            throw;
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>
    /// Sends a message and returns the model's reply piece by piece, as it is produced.
    /// </summary>
    /// <remarks>
    /// The message is sent when enumeration starts, not when this method is called.
    /// The user message and the reply stay in <see cref="Messages"/> only if the whole reply
    /// is received. If the request fails, is canceled or the caller stops reading early,
    /// the user message is removed and the partial reply is discarded.
    /// </remarks>
    /// <param name="text">The user's message.</param>
    /// <param name="ct">Token used to cancel the request, also while the reply is being received.</param>
    /// <returns>The pieces of the reply, in order.</returns>
    /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
    /// <exception cref="InvalidOperationException">
    /// A message is already being sent, or the response failed or was incomplete.
    /// </exception>
    /// <exception cref="OperationCanceledException"><paramref name="ct"/> was canceled.</exception>
    public async IAsyncEnumerable<string> StreamAsync(string text, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("The message cannot be empty.", nameof(text));
        if (_busy)
            throw new InvalidOperationException("Wait for the current reply before sending another message.");

        _busy = true;
        var userMessage = new ChatMessage(ChatRole.User, text);
        _messages.Add(userMessage);

        var reply = new StringBuilder();
        bool completed = false;

        // yield return is not allowed in a try block with a catch, so the cleanup happens in finally
        try
        {
            await foreach (string delta in _ai.StreamResponseAsync(BuildRequest(), ct))
            {
                reply.Append(delta);
                yield return delta;
            }

            _messages.Add(new ChatMessage(ChatRole.Assistant, reply.ToString()));
            completed = true;
        }
        finally
        {
            // Keep the history consistent: no user message without a reply
            if (!completed)
                _messages.Remove(userMessage);

            _busy = false;
        }
    }

    /// <summary>Removes all messages and starts the conversation over.</summary>
    public void Clear() => _messages.Clear();

    private AiRequest BuildRequest()
    {
        var builder = new PromptBuilder(_model);
        _configure?.Invoke(builder);
        builder.AddMessages(_messages);

        // A short instruction after the newest message, sent but not stored in the history
        if (_turnInstruction?.Invoke() is { Length: > 0 } instruction)
            builder.AddDeveloperMessage(instruction);

        return builder.Build();
    }
}