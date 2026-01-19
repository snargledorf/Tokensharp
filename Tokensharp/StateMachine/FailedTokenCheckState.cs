namespace Tokensharp.StateMachine;

internal class FailedTokenCheckState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : MixedCharacterFailedTokenCheckState<TTokenType>(fallbackState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength, ref int confirmedLexemeLength)
    {
        potentialLexemeLength++;
        base.UpdateCounts(ref potentialLexemeLength, ref fallbackLexemeLength, ref confirmedLexemeLength);
    }
}