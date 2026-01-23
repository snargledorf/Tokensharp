using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface ITransitionHandler<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    TransitionResult TryTransition(char c, ref StateMachineContext context, out IState<TTokenType>? nextState);
    bool TryDefaultTransition(ref StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? defaultState);
}

internal enum TransitionResult
{
    NewState,
    EndOfToken,
    Failure
}