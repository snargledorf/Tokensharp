using System.Collections;
using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

public class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes)
    {
        TokenTree = lexemeToTokenTypes.ToTokenTree();
        StartState = StartState<TTokenType>.For(TokenTree);
    }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }

    internal ITokenTreeNode<TTokenType> TokenTree { get; }
    
    internal StartState<TTokenType> StartState { get; }
}