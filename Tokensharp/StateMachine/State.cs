using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected abstract TransitionResult TransitionResult { get; }

    public virtual TransitionResult TryTransition(char c, ref StateMachineContext context, out IState<TTokenType>? nextState)
    {
        if (!TryGetNextState(c, out State<TTokenType>? state) &&
            !TryGetDefaultState(out state))
        {
            nextState = null;
            return TransitionResult.Failure;
        }
        
        state.UpdateCounts(ref context);
        nextState = state;
        return state.TransitionResult;
    }

    protected abstract bool TryGetNextState(char c, [NotNullWhen(true)] out State<TTokenType>? nextState);

    public virtual bool TryDefaultTransition(ref StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        if (!TryGetDefaultState(out State<TTokenType>? state))
        {
            defaultState = null;
            return false;
        }

        state.UpdateCounts(ref context);
        defaultState = state;
        return true;
    }

    protected abstract bool TryGetDefaultState([NotNullWhen(true)] out State<TTokenType>? defaultState);
    
    public virtual bool TryFinalizeToken(ref StateMachineContext context, out int lexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        lexemeLength = 0;
        tokenType = null;
        return false;
    }

    protected virtual void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }
}