using System.Diagnostics;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class CheckForTokenState<TTokenType> : NodeStateBase<TTokenType>, IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly FailedTokenCheckState<TTokenType> _fallbackFailedTokenCheckState;
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _endOfFallbackFailedTokenCheckState;

    private readonly MixedCharacterFailedTokenCheckState<TTokenType> _defaultFailedTokenCheckState;
    private readonly MixedCharacterCheckFailedEndOfTokenState<TTokenType> _defaultEndOfFallbackFailedTokenCheckState;
    private readonly IEndOfTokenStateAccessor<TTokenType> _fallbackStateEndOfTokenStateAccessor;
    private readonly IStateCharacterCheck _fallbackStateCharacterCheck;

    public CheckForTokenState(ITokenTreeNode<TTokenType> node,
        IState<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenStateAccessor,
        IStateCharacterCheck fallbackStateCharacterCheck) : base(node)
    {
        _fallbackStateEndOfTokenStateAccessor = fallbackStateEndOfTokenStateAccessor;
        _fallbackStateCharacterCheck = fallbackStateCharacterCheck;
        
        _fallbackFailedTokenCheckState =
            new FailedTokenCheckState<TTokenType>(fallbackState, fallbackStateCharacterCheck);
        
        _endOfFallbackFailedTokenCheckState =
            new MixedCharacterCheckFailedEndOfTokenState<TTokenType>(fallbackStateEndOfTokenStateAccessor
                .EndOfTokenStateInstance);
        
        _defaultFailedTokenCheckState =
            new MixedCharacterFailedTokenCheckState<TTokenType>(fallbackState, fallbackStateCharacterCheck);
        
        _defaultEndOfFallbackFailedTokenCheckState =
            new MixedCharacterCheckFailedEndOfTokenState<TTokenType>(fallbackStateEndOfTokenStateAccessor
                .EndOfTokenStateInstance);

        StateLookup = BuildStateLookup(node, fallbackState, fallbackStateEndOfTokenStateAccessor, fallbackStateCharacterCheck);
    }

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => _fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance;

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (StateLookup.TryGetState(c, out nextState!))
            return true;
        
        if (_fallbackStateCharacterCheck.CharacterIsValidForState(c))
        {
            nextState = _fallbackFailedTokenCheckState;
        }
        else
        {
            nextState = _endOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (_fallbackStateCharacterCheck.CharacterIsValidForState(Node.Character))
        {
            defaultState = _defaultFailedTokenCheckState;
        }
        else
        {
            defaultState = _defaultEndOfFallbackFailedTokenCheckState;
        }

        return true;
    }

    private static IStateLookup<TTokenType> BuildStateLookup(ITokenTreeNode<TTokenType> node, IState<TTokenType> fallbackState,
        IEndOfTokenStateAccessor<TTokenType> fallbackStateEndOfTokenStateAccessor, IStateCharacterCheck fallbackStateCharacterCheck)
    {
        var childStates = new StateLookupBuilder<TTokenType>();
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            if (childNode.IsEndOfToken)
            {
                childStates.Add(childNode.Character, fallbackStateEndOfTokenStateAccessor.EndOfTokenStateInstance);
            }
            else
            {
                childStates.Add(childNode.Character,
                    new CheckForTokenState<TTokenType>(childNode, fallbackState, fallbackStateEndOfTokenStateAccessor,
                        fallbackStateCharacterCheck));
            }
        }

        return childStates.Build();
    }
}