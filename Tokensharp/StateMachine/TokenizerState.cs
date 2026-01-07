using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

public sealed class TokenizerState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly IReadOnlyCollection<Transition<TTokenType>>? _transitions;

    public TokenizerState(IReadOnlyCollection<Transition<TTokenType>>? transitions = null) 
        : this(null, transitions)
    {
    }

    public TokenizerState(TTokenType? tokenType, IReadOnlyCollection<Transition<TTokenType>>? transitions = null)
    {
        TokenType = tokenType;
        _transitions = transitions;
        HasTokenType = tokenType is not null;
        HasInputTransitions = transitions?.Count > 0;
        IsFinalState = HasTokenType && !HasInputTransitions;
    }

    public TTokenType? TokenType { get; }

    public bool HasInputTransitions { get; }
    
    public bool IsFinalState { get; }
    
    public bool HasTokenType { get; }

    public bool TryTransition(char input, [NotNullWhen(true)] out TokenizerState<TTokenType>? newState)
    {
        newState = FindTransitionForInput(input)?.State;
        return newState is not null;
    }

    private Transition<TTokenType>? FindTransitionForInput(char input)
    {
        return _transitions?.FirstOrDefault(t => t.Condition(input));
    }
}