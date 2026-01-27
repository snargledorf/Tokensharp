namespace Tokensharp.StateMachine;

internal interface ITransitionHandler<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    IState<TTokenType> Transition(in char c, ref StateMachineContext context);
    IState<TTokenType> PerformDefaultTransition(ref StateMachineContext context);
}