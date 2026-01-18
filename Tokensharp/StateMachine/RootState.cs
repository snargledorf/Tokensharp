using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class RootState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : NodeStateBase<TTokenType>(rootNode.RootNode) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected abstract WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; }
    protected abstract NumberState<TTokenType> NumberStateInstance { get; }
    protected abstract TextState<TTokenType> TextStateInstance { get; }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            IEndOfTokenAccessorState<TTokenType> fallbackState = GetFallbackState(c);
            nextState = GetNextStateForChildNode(childNode, fallbackState);

            AddStateToCache(c, nextState);
            return true;
        }

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

    protected virtual IEndOfTokenAccessorState<TTokenType> GetNextStateForChildNode(ITokenTreeNode<TTokenType> childNode, IEndOfTokenAccessorState<TTokenType> fallbackState)
    {
        return new PotentialTokenState<TTokenType>(childNode, fallbackState, WhiteSpaceStateInstance,
            NumberStateInstance, TextStateInstance);
    }

    private IEndOfTokenAccessorState<TTokenType> GetFallbackState(char c)
    {
        if (char.IsWhiteSpace(c))
            return WhiteSpaceStateInstance;
        
        if (char.IsDigit(c))
            return NumberStateInstance;
        
        return TextStateInstance;
    }
}