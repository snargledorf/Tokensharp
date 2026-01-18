using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private protected readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    public virtual bool TryTransition(char c, StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!TryGetStateFromCache(c, out nextState) &&
            !TryGetStateNextState(c, out nextState) &&
            !TryGetDefaultState(out nextState))
            return false;
        
        nextState.OnEnter(context);
        return true;
    }

    private bool TryGetStateFromCache(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        return _states.TryGetValue(c, out state);
    }

    protected void AddStateToCache(char c, IState<TTokenType> state)
    {
        _states.Add(c, state);
    }

    protected abstract bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState);

    public virtual bool TryDefaultTransition(StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (!TryGetDefaultState(out defaultState))
            return false;
        
        defaultState.OnEnter(context);
        return true;
    }

    protected abstract bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState);

    public abstract bool CharacterIsValidForState(char c);
    
    public virtual void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }
}