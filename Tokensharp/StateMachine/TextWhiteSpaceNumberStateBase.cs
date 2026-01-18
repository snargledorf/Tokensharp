using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberStateBase<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : NodeStateBase<TTokenType>(rootNode.RootNode), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!CharacterIsValidForState(c))
        {
            nextState = EndOfTokenStateInstance;
        }
        else if (!TryGetStateForChildNode(c, out nextState))
        {
            nextState = this;
        }

        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenStateInstance;
        return true;
    }
}