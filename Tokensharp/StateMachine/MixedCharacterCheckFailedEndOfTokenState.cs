namespace Tokensharp.StateMachine;

internal class MixedCharacterCheckFailedEndOfTokenState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : EndOfTokenState<TTokenType>(fallbackState.EndOfTokenStateInstance.TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void OnEnter(StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
        base.OnEnter(context);
    }
}