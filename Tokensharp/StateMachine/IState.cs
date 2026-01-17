namespace Tokensharp.StateMachine;

internal interface IState<TTokenType> : ITransitionHandler<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    void OnEnter(StateMachineContext<TTokenType> context);
}