using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

public sealed class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes, bool numbersAreText = false)
    {
        TokenTree = lexemeToTokenTypes.ToTokenTree();
        
        StartState = new StartState<TTokenType>(TokenTree, numbersAreText);
        
        NumbersAreText = numbersAreText;
    }

    public bool NumbersAreText { get; }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }
    
    internal ITokenTreeNode<TTokenType> TokenTree { get; }
    
    internal StartState<TTokenType> StartState { get; }
}