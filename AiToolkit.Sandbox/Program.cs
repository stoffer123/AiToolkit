using AiToolkit.Builders;
using AiToolkit.Contracts;
using AiToolkit.Providers.OpenAi;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

string openAiApiKey = config["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI-nøgle mangler.");

IAiService ai = new OpenAiService(new ResponsesClient(openAiApiKey));

AiRequest request = new PromptBuilder("gpt-4o-mini")
    .WithInstructions("""
        Du er madplanlægger for en familie på 2 voksne og 2 børn (4 år, 2 år).
        Der er allergi over for laktose og gluten.
        Hårde oste kan godt tåles.
        """)
    .WithTemperature(0.7f)
    .WithMaxOutputTokens(2000)
    .AddUserMessage("""
        Lav en madplan til 4 dage.
        Børnevenlig og gerne med pasta.
        Tilføj et frokostforslag til hver dag.
        Lav til sidst en samlet indkøbsliste.
        """)
    .Build();

string svar = await ai.GetResponseAsync(request);
Console.WriteLine(svar);