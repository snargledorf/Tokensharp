namespace Tokensharp.StateMachine;

internal class MixedCharacterCheckFailedEndOfTokenState<TTokenType>(IEndOfTokenStateAccessor<TTokenType> fallback)
    : EndOfTokenState<TTokenType>(fallback.EndOfTokenStateInstance.TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }
}