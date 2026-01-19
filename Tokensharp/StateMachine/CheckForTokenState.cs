using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState)
    : NodeStateBase<TTokenType>(node), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly FoundTokenState<TTokenType> _foundTokenState = new(fallbackState.EndOfTokenStateInstance);

    private readonly FailedTokenCheckState<TTokenType> _fallbackFailedTokenCheckState = new(fallbackState);

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _endOfFallbackFailedTokenCheckState = new(fallbackState.EndOfTokenStateInstance);

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _defaultEndOfFallbackFailedTokenCheckState = new(fallbackState.EndOfTokenStateInstance);

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _defaultFailedTokenCheckState = new(fallbackState);

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= fallbackState.EndOfTokenStateInstance;

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (Node.IsEndOfToken)
        {
            nextState = _foundTokenState;
        }
        else if (!TryGetStateForChildNode(c, out nextState))
        {
            if (CharacterIsValidForState(c))
            {
                nextState = _fallbackFailedTokenCheckState;
            }
            else
            {
                nextState = _endOfFallbackFailedTokenCheckState;
            }
        }

        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (Node.IsEndOfToken)
        {
            defaultState = _foundTokenState;
        }
        else if (CharacterIsValidForState(Node.Character))
        {
            defaultState = _defaultFailedTokenCheckState;
        }
        else
        {
            defaultState = _defaultEndOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    public override void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }

    protected static CheckForTokenState<TTokenType> For(ITokenTreeNode<TTokenType> node,
        Func<ITokenTreeNode<TTokenType>, IEndOfTokenAccessorState<TTokenType>> getFallbackState)
    {
        IEndOfTokenAccessorState<TTokenType> fallbackState = getFallbackState(node);
        
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
            childStates.Add(childNode.Character, For(childNode, getFallbackState));
        
        return new CheckForTokenState<TTokenType>(node, fallbackState)
        {
            StateLookup = childStates.Build()
        };
    }
}