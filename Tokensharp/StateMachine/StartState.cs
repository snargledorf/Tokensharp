using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : RootState<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = WhiteSpaceState<TTokenType>.For(rootNode);
    internal override NumberState<TTokenType> NumberStateInstance { get; } = NumberState<TTokenType>.For(rootNode);
    internal override TextState<TTokenType> TextStateInstance { get; } = TextState<TTokenType>.For(rootNode);

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (base.TryGetStateNextState(c, out nextState))
            return true;

        if (char.IsWhiteSpace(c))
        {
            nextState = WhiteSpaceStateInstance;
            return true;
        }

        if (char.IsDigit(c))
        {
            nextState = NumberStateInstance;
            return true;
        }

        nextState = TextStateInstance;
        return true;
    }

    protected override IState<TTokenType> GetFallbackEndOfTokenState(ITokenTreeNode<TTokenType> node)
    {
        if (node.IsWhiteSpaceToRoot)
            return EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.WhiteSpace);
        if (node.IsDigitsToRoot)
            return EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);
        
        return EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);
    }
}