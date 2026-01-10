using System.Collections.Frozen;

namespace Tokensharp;

public class TokenConfigurationBuilder
{
    private readonly HashSet<RuntimeConfigToken> _tokenTypes = [];

    public TokenConfigurationBuilder AddTokenType(string lexeme)
    {
        _tokenTypes.Add(new RuntimeConfigToken(lexeme));
        return this;
    }
    
    public TokenConfiguration Build()
    {
        return new TokenConfiguration(_tokenTypes.ToFrozenSet());
    }
}