using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class FailedTokenCheckState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : MixedCharacterFailedTokenCheckState<TTokenType>(fallbackState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        potentialLexemeLength++;
        return base.UpdateCounts(ref potentialLexemeLength, ref fallbackLexemeLength, ref confirmedLexemeLength,
            out tokenType);
    }
}