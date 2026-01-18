namespace Tokensharp.StateMachine;

internal interface IEndOfTokenAccessorState<TTokenType> : IState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; }
}