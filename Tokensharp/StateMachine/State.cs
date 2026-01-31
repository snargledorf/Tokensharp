using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class State<TTokenType> : IState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract bool IsEndOfToken { get; }

    public IState<TTokenType> Transition(in char c, ref StateMachineContext context)
    {
        IState<TTokenType> nextState = GetNextState(in c);
        nextState.UpdateCounts(ref context);
        return nextState;
    }

    protected abstract IState<TTokenType> GetNextState(in char c);

    public IState<TTokenType> PerformDefaultTransition(ref StateMachineContext context)
    {
        DefaultState.UpdateCounts(ref context);
        return DefaultState;
    }

    protected abstract IState<TTokenType> DefaultState { get; }

    public virtual bool FinalizeToken(ref StateMachineContext context, [NotNullWhen(true)] ref TokenType<TTokenType>? tokenType,
        ref int lexemeLength) =>
        false;

    public virtual void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
    }
}