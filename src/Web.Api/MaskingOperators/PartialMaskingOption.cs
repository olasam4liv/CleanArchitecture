
namespace Web.Api.MaskingOperators;

public class PartialMaskOption : RegexMaskingOperator
{
    public PartialMaskOption(IConfiguration configuration) : base(PatternFactory.GetPattern(configuration, "MaskRegex:PartialMask"))
    {

    }

    protected override string PreprocessMask(string mask, Match match)
    {
        string value = match.Value.Split(':')[1];
        string key = match.Value.Split(':')[0];
        decimal length = value.Length;
        decimal splitCount = length / 3;
        mask = new string('*', (int)splitCount);
        string start = value.Substring(0, (int)splitCount);
        string end = value.Substring(start.Length + (int)splitCount);
        return $"{key}:{start}{mask}{end}, ";

    }
}