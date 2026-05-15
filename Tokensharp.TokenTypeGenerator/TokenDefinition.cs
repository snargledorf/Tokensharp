namespace Tokensharp.TokenTypeGenerator;

internal readonly record struct TokenDefinition
{
    public readonly string ClassName;
    public readonly IEnumerable<KeyValuePair<string, string>> Tokens;
    public readonly bool NumbersAreText;

    public TokenDefinition(string className, IEnumerable<KeyValuePair<string, string>> tokens, bool numbersAreText)
    {
        ClassName = className;
        Tokens = tokens;
        NumbersAreText = numbersAreText;
    }
}