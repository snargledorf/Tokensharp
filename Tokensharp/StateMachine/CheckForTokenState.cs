using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    TextWhiteSpaceNumberStateBase<TTokenType> fallbackState)
    : NodeStateBase<TTokenType>(node)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _transitions = new();
    
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
                nextState = new FoundTokenState<TTokenType>(fallbackState.EndOfTokenState);
            }
            else
            {
                nextState = new CheckForTokenState<TTokenType>(childNode, fallbackState);
            }

            _transitions.Add(c, nextState);
            return true;
        }

        IState<TTokenType> failedFallbackState;
        if (fallbackState.CharacterIsValidForToken(c))
            failedFallbackState= fallbackState;
        else
            failedFallbackState = fallbackState.EndOfTokenState;
        
        nextState = new FailedTokenCheckState<TTokenType>(failedFallbackState);
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