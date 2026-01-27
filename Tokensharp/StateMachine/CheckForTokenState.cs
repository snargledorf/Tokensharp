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

    protected override IState<TTokenType> GetNextState(in char c)
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (StateLookup.TryGetState(c, out IState<TTokenType>? nextState))
            return nextState;
        
        if (_fallbackStateCharacterCheck.CharacterIsValidForState(c))
        {
            return _fallbackFailedTokenCheckState;
        }

        return _endOfFallbackFailedTokenCheckState;
    }

    protected override IState<TTokenType> GetDefaultState()
    {
        Debug.Assert(!Node.IsEndOfToken);
        
        if (_fallbackStateCharacterCheck.CharacterIsValidForState(Node.Character))
            return _defaultFailedTokenCheckState;

        return _defaultEndOfFallbackFailedTokenCheckState;
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