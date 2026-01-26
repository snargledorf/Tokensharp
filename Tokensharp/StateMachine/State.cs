using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract bool IsEndOfToken { get; }

    public virtual bool TryTransition(in char c, ref StateMachineContext context, out IState<TTokenType> nextState)
    {
        if (!TryGetNextState(in c, out nextState) &&
            !TryGetDefaultState(out nextState))
            return false;
        
        nextState.UpdateCounts(ref context);
        return !nextState.IsEndOfToken;
    }

    protected abstract bool TryGetNextState(in char c, out IState<TTokenType> nextState);

    public virtual bool TryDefaultTransition(ref StateMachineContext context, out IState<TTokenType> defaultState)
    {
        if (!TryGetDefaultState(out defaultState))
            return false;

        defaultState.UpdateCounts(ref context);
        return true;
    }

    protected abstract bool TryGetDefaultState(out IState<TTokenType> defaultState);
    
    public virtual bool FinalizeToken(ref StateMachineContext context, out int lexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        lexemeLength = 0;
        tokenType = null;
        return false;
    }

    public virtual void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }
}