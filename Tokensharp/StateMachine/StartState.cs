using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : BaseState<TTokenType>(rootNode, WhiteSpaceState<TTokenType>.For(rootNode), NumberState<TTokenType>.For(rootNode), TextState<TTokenType>.For(rootNode))
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (base.TryGetStateNextState(c, out nextState))
            return true;

        if (char.IsWhiteSpace(c))
        {
            nextState = WhiteSpaceState;
            return true;
        }

        if (char.IsDigit(c))
        {
            nextState = NumberState;
            return true;
        }

        nextState = TextState;
        return true;
    }
}