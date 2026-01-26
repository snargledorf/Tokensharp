using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

public sealed class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes, bool textAndNumbersAreText = false)
    {
        ITokenTreeNode<TTokenType> tokenTree = lexemeToTokenTypes.ToTokenTree();
        
        StartState = StartState<TTokenType>.For(tokenTree, textAndNumbersAreText);
        
        TextAndNumbersAreText = textAndNumbersAreText;
    }

    public bool TextAndNumbersAreText { get; }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }
    
    internal StartState<TTokenType> StartState { get; }
}