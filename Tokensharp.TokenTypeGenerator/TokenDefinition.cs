namespace Tokensharp.TokenTypeGenerator;

internal readonly record struct TokenDefinition
{
    public readonly string ClassName;
    public readonly IEnumerable<KeyValuePair<string, string>> Tokens;

    public TokenDefinition(string className, IEnumerable<KeyValuePair<string, string>> tokens)
    {
        ClassName = className;
        Tokens = tokens;
    }
}