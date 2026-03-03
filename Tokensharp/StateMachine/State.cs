using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract bool IsEndOfToken { get; }

    public State<TTokenType> Transition(char c, ref StateMachineContext context)
    {
        State<TTokenType> nextState = GetNextState(c);
        nextState.UpdateCounts(ref context);
        return nextState;
    }

    protected abstract State<TTokenType> GetNextState(char c);

    public State<TTokenType> PerformDefaultTransition(ref StateMachineContext context)
    {
        State<TTokenType> defaultState = DefaultState;
        defaultState.UpdateCounts(ref context);
        return defaultState;
    }

    protected abstract State<TTokenType> DefaultState { get; }

    public virtual bool FinalizeToken(ref StateMachineContext context, [NotNullWhen(true)] ref TokenType<TTokenType>? tokenType,
        ref int lexemeLength) =>
        false;

    public virtual void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }
}