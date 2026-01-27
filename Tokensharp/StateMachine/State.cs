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
        IState<TTokenType> defaultState = GetDefaultState();
        defaultState.UpdateCounts(ref context);
        return defaultState;
    }

    protected abstract IState<TTokenType> GetDefaultState();
    
    public virtual bool FinalizeToken(ref StateMachineContext context, out int lexemeLength,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
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