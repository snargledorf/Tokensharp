using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public virtual bool TryTransition(char c, StateMachineContext<TTokenType> context, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!TryGetStateNextState(c, out nextState))
            return false;
        
        nextState.OnEnter(context);
        return true;
    }

    protected virtual bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    public virtual bool TryDefaultTransition(StateMachineContext<TTokenType> context, [NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (!TryGetDefaultState(out defaultState))
            return false;
        
        defaultState.OnEnter(context);
        return true;
    }

    protected virtual bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = null;
        return false;
    }

    public virtual void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (context.PotentialLexemeLength > context.ConfirmedLexemeLength)
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
        else
        {
            context.ConfirmedLexemeLength++;
            context.PotentialLexemeLength = context.ConfirmedLexemeLength;
        }
    }
}