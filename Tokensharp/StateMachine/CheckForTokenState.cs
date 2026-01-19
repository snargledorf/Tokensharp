using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IEndOfTokenAccessorState<TTokenType> fallbackState)
    : NodeStateBase<TTokenType>(node), IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly FailedTokenCheckState<TTokenType> _fallbackFailedTokenCheckState = new(fallbackState);
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _endOfFallbackFailedTokenCheckState = new(fallbackState.EndOfTokenStateInstance);

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _defaultFailedTokenCheckState = new(fallbackState);
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _defaultEndOfFallbackFailedTokenCheckState = new(fallbackState.EndOfTokenStateInstance);

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= fallbackState.EndOfTokenStateInstance;

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        Debug.Assert(Node.IsEndOfToken);
        if (TryGetStateForChildNode(c, out nextState))
            return true;
        
        if (CharacterIsValidForState(c))
        {
            nextState = _fallbackFailedTokenCheckState;
        }
        else
        {
            nextState = _endOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        Debug.Assert(Node.IsEndOfToken);
        
        if (CharacterIsValidForState(Node.Character))
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
        IEndOfTokenAccessorState<TTokenType> fallbackState)
    {
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.IsEndOfToken)
            {
                childStates.Add(childNode.Character, fallbackState.EndOfTokenStateInstance);
            }
            else
            {
                childStates.Add(childNode.Character, For(childNode, fallbackState));
            }
        }
        
        return new CheckForTokenState<TTokenType>(node, fallbackState)
        {
            StateLookup = childStates.Build()
        };
    }
}