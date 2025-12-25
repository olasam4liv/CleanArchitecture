
namespace Web.Api.MaskingOperators;

public static class PatternFactory
{
    public static string GetPattern(IConfiguration configuration, string configSection)
    {
        string patterns = string.Empty;
        List<string>? properties = configuration.GetSection(configSection).Get<List<string>>();
        if (properties == null || properties.Count == 0)
        {
            return string.Empty;
        }
            
        foreach (string item in properties)
        {
            string firstCharacter = item[0].ToString();
            string propertyName = $"({firstCharacter.ToUpperInvariant()}|{firstCharacter.ToUpperInvariant()}){item.Substring(1)}";
            string pattern = $"(\"{propertyName}\"?[\t\r\n]:[\t\r\n]\"?([^\"]*))";
            patterns = string.IsNullOrEmpty(patterns)
                ? pattern
                : $"{patterns}|{pattern}";
        }
        return patterns;
    }
}