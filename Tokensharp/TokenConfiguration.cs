namespace Tokensharp;

public class TokenConfiguration : TokenConfiguration<RuntimeConfigToken>
{
    internal TokenConfiguration(IEnumerable<RuntimeConfigToken> tokenTypes) : base(tokenTypes)
    {
    }
}

public class TokenConfiguration<TTokenType>
    where TTokenType : ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<TTokenType> tokenTypes)
    {
        TokenTypes = tokenTypes;
    }
    
    internal IEnumerable<TTokenType> TokenTypes { get; }
}