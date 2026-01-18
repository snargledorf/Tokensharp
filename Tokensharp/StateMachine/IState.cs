namespace Tokensharp.StateMachine;

internal interface IState<TTokenType> : ITransitionHandler<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool CharacterIsValidForState(char c);
    void OnEnter(StateMachineContext context);
}