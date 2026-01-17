using System.Collections;
using Tokensharp.TokenTree;

namespace Tokensharp;

public class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly ITokenTreeNode<TTokenType> _tokenTree;

    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes)
    {
        _tokenTree = lexemeToTokenTypes.ToTokenTree();
    }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }

    internal ITokenTreeNode<TTokenType> TokenTree => _tokenTree;
}