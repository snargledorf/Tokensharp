namespace Tokensharp.StateMachine;

internal interface ITransitionHandler<TTokenType> 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    State<TTokenType> Transition(char c, ref StateMachineContext context);
    State<TTokenType> PerformDefaultTransition(ref StateMachineContext context);
}