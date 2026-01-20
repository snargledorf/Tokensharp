namespace Tokensharp.StateMachine;

internal class MixedCharacterCheckFailedEndOfTokenState<TTokenType>(IEndOfTokenStateAccessor<TTokenType> fallback)
    : EndOfTokenState<TTokenType>(fallback.EndOfTokenStateInstance.TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }
}