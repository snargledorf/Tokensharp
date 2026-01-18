using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface ITransitionHandler<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool TryTransition(char c, StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? nextState);
    bool TryDefaultTransition(StateMachineContext context, [NotNullWhen(true)] out IState<TTokenType>? defaultState);
}