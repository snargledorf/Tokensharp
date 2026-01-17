using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> fallbackState,
    IState<TTokenType> endOfFallbackState)
    : NodeStateBase<TTokenType>(node)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
    private readonly FailedTokenCheckState<TTokenType> _state = new(fallbackState);
    
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.IsEndOfToken)
        {
            nextState = fallbackState;
            return true;
        }
        
        if (_transitions.TryGetValue(c, out nextState)) 
            return true;

        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? childNode))
        {
            if (childNode.IsEndOfToken)
            {
                nextState = new FoundTokenState<TTokenType>(endOfFallbackState);
            }
            else
            {
                nextState = new CheckForTokenState<TTokenType>(childNode, fallbackState, endOfFallbackState);
            }

            _transitions.Add(c, nextState);
            return true;
        }

        nextState = _state;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.PotentialLexemeLength++;
    }
}