using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class NumberState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static NumberState<TTokenType>? _instance;

    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance =>
        field ??= WhiteSpaceState<TTokenType>.For(RootNode);
    
    internal override NumberState<TTokenType> NumberStateInstance => this;
    
    internal override TextState<TTokenType> TextStateInstance => field ??= TextState<TTokenType>.For(RootNode);
   
    protected override TTokenType TokenType => TokenType<TTokenType>.Number;
    protected override EndOfTokenState<TTokenType> EndOfTokenState { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);

    internal static NumberState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        return _instance = new NumberState<TTokenType>(treeNode.RootNode);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsDigit(c))
        {
            nextState = EndOfTokenState;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    protected override bool CharacterIsValidForToken(char c)
    {
        return char.IsDigit(c);
    }
}