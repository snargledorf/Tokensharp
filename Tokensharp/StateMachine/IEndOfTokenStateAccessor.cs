namespace Tokensharp.StateMachine;

internal interface IEndOfTokenStateAccessor<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; }
}