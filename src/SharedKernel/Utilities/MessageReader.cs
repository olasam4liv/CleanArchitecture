using System.Text;
using System.Text.Json;
using SharedKernel.Model;

namespace SharedKernel.Helper;

public static class MessageReader
{
    public static string GetMessage(string tag, string? language)
    {
        language = string.IsNullOrEmpty(language) ? "en" : language;
        return LoadMessages(tag, language, "ResponseMessages").Message;
    }

    public static LanguageModel LoadMessages(string tag, string language, string fileName, string? type = default)
    {
        string filePath = Path.Combine("Resources", $"{fileName}.json");
        string jsonString = File.ReadAllText(filePath, Encoding.UTF8);
        List<MessageModel>? messageResources = JsonSerializer.Deserialize<List<MessageModel>>(jsonString);
        if (messageResources == null)
        {
            return LoadMessages("PROCESS_ERROR", language, "ResponseMessages");
        }
        
        MessageModel? messages = messageResources.Find(m => m.Tag.ToUpperInvariant().Equals(tag.ToUpperInvariant(), StringComparison.Ordinal));
        if (messages == null)
        {
            return LoadMessages("PROCESS_ERROR", language, "ResponseMessages");
        }

        LanguageModel? message = type == null
            ? messages.Languages.FirstOrDefault(l => l.Language.ToUpperInvariant().Equals(language.ToUpperInvariant(), StringComparison.Ordinal))
            : messages.Languages.FirstOrDefault(l =>
                l.Language.ToUpperInvariant().Equals(language.ToUpperInvariant(), StringComparison.Ordinal)
                && l.MessageType == type);
                
        if (message == null)
        {
            return LoadMessages("PROCESS_ERROR", language, "ResponseMessages");
        }
        
        return message;
    }
}
