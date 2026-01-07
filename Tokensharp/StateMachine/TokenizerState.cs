using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

public sealed class TokenizerState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IReadOnlyDictionary<char, TokenizerState<TTokenType>> _directInputTransitions;
    private readonly IReadOnlyCollection<Transition<TTokenType>>? _transitions;
    private readonly TokenizerState<TTokenType>? _defaultState;

    public TokenizerState(TTokenType? tokenType, IReadOnlyDictionary<char, TokenizerState<TTokenType>> directInputTransitions, IReadOnlyCollection<Transition<TTokenType>> transitions, TokenizerState<TTokenType>? defaultState)
    {
        TokenType = tokenType;
        _directInputTransitions = directInputTransitions;
        _transitions = transitions;
        _defaultState = defaultState;
        HasTokenType = tokenType is not null;
        HasInputTransitions = transitions.Count > 0;
        IsFinalState = HasTokenType && !HasInputTransitions;
    }

    public TTokenType? TokenType { get; }

    public bool HasInputTransitions { get; }
    
    public bool IsFinalState { get; }
    
    public bool HasTokenType { get; }

    public bool TryTransition(char input, [NotNullWhen(true)] out TokenizerState<TTokenType>? newState)
    {
        if (_directInputTransitions.TryGetValue(input, out newState))
            return true;
        
        newState = FindTransitionForInput(input)?.State ?? _defaultState;
        return newState is not null;
    }

    private Transition<TTokenType>? FindTransitionForInput(char input)
    {
        return _transitions?.FirstOrDefault(t => t.Condition(input));
    }

    public bool TryGetDefault([NotNullWhen(true)] out TTokenType? tokenType)
    {
        tokenType = _defaultState?.TokenType;
        return tokenType is not null;
    }
}