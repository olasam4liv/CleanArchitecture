namespace SharedKernel.Model;

public class MessageModel
{
    public string Tag { get; set; } = default!;
    public ICollection<LanguageModel> Languages { get; init; } = [];
}

public class LanguageModel
{
    public string Language { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string MessageType { get; set; } = default!;
    public string Title { get; set; } = default!;
}