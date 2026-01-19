using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public virtual bool TryTransition(char c, StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!TryGetNextState(c, out nextState) &&
            !TryGetDefaultState(out nextState))
            return false;
        
        nextState.OnEnter(context);
        return true;
    }

    protected abstract bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState);

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