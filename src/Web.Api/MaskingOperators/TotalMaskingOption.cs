using System.Configuration;
namespace Web.Api.MaskingOperators;

public class TotalMaskingOption : RegexMaskingOperator
{
    public TotalMaskingOption(IConfiguration configuration) : base(PatternFactory.GetPattern(configuration, "MaskRegex:TotalMask"))
    {

    }

    protected override string PreprocessMask(string mask, Match match)
    {
        string value = match.Value.Split(':')[1];
        mask = new string('*', value.Length);
        string key = match.Value.Split(':')[0];
        return $"{key}:{mask}, ";
    }
}