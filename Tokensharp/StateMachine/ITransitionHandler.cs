namespace Tokensharp.StateMachine;

internal interface ITransitionHandler<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    void Transition(in char c, ref StateMachineContext context, out IState<TTokenType> nextState);
    bool TryDefaultTransition(ref StateMachineContext context, out IState<TTokenType> defaultState);
}