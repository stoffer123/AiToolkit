using System;
using System.Collections.Generic;
using System.Text;

namespace AiToolkit.Contracts
{
    public interface IAiService
    {
        Task<string> GetResponseAsync(AiRequest request, CancellationToken ct = default);
    }
}
