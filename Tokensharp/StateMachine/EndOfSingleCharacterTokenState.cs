namespace Tokensharp.StateMachine;

internal class EndOfSingleCharacterTokenState<TTokenType>(TTokenType tokenType) 
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void OnEnter(StateMachineContext context)
    {
        context.ConfirmedLexemeLength = 1;
    }
}