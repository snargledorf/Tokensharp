using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    State<TTokenType> fallback,
    IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenStateAccessor,
    IStateCharacterCheck fallbackStateCharacterCheck)
    : NodeStateBase<TTokenType>(node), IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly FailedTokenCheckState<TTokenType> _fallbackFailedTokenCheckState = new(fallback, fallbackStateCharacterCheck);
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _endOfFallbackFailedTokenCheckState = new(fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance);

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _defaultFailedTokenCheckState = new(fallback, fallbackStateCharacterCheck);
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _defaultEndOfFallbackFailedTokenCheckState = new(fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance);

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => field ??= fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance;

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out State<TTokenType>? nextState)
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (TryGetStateForChildNode(c, out nextState))
            return true;
        
        if (fallbackStateCharacterCheck.CharacterIsValidForState(c))
        {
            nextState = _fallbackFailedTokenCheckState;
        }
        else
        {
            nextState = _endOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out State<TTokenType>? defaultState)
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (fallbackStateCharacterCheck.CharacterIsValidForState(Node.Character))
        {
            defaultState = _defaultFailedTokenCheckState;
        }
        else
        {
            defaultState = _defaultEndOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    protected static CheckForTokenState<TTokenType> For(
        ITokenTreeNode<TTokenType> node,
        State<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.IsEndOfToken)
            {
                childStates.Add(childNode.Character, fallbackStateEndOfTokenAccessor.EndOfTokenStateInstance);
            }
            else
            {
                childStates.Add(childNode.Character,
                    For(childNode, fallbackState, fallbackStateEndOfTokenAccessor, fallbackStateCharacterCheck));
            }
        }

        return new CheckForTokenState<TTokenType>(node, fallbackState, fallbackStateEndOfTokenAccessor,
            fallbackStateCharacterCheck)
        {
            StateLookup = childStates.Build()
        };
    }
}