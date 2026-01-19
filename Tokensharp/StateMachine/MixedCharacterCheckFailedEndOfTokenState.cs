namespace Tokensharp.StateMachine;

internal class MixedCharacterCheckFailedEndOfTokenState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : EndOfTokenState<TTokenType>(fallbackState.EndOfTokenStateInstance.TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength, ref int confirmedLexemeLength)
    {
        fallbackLexemeLength = potentialLexemeLength;
        base.UpdateCounts(ref  potentialLexemeLength, ref fallbackLexemeLength, ref confirmedLexemeLength);
    }
}