using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForMixedTypeTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> fallbackState,
    TextState<TTokenType> textState,
    WhiteSpaceState<TTokenType> whiteSpace,
    NumberState<TTokenType> numberState)
    : BaseState<TTokenType>(node, whiteSpace, numberState, textState)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? nextTreeNode))
        {
            if (nextTreeNode.IsEndOfToken)
            {
                nextState = EndOfTokenState<TTokenType>.For(nextTreeNode.TokenType);
            }
            else
            {
                nextState = new CheckForMixedTypeTokenState<TTokenType>(nextTreeNode, fallbackState, TextState,
                    WhiteSpaceState, NumberState);
            }

            _transitions.Add(c, nextState);
            return true;
        }
        
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }
}