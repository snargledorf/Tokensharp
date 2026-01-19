using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MixedCharacterCheckFailedEndOfTokenState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : EndOfTokenState<TTokenType>(fallbackState.EndOfTokenStateInstance.TokenType)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        fallbackLexemeLength = potentialLexemeLength;
        return base.UpdateCounts(ref potentialLexemeLength, ref fallbackLexemeLength, ref confirmedLexemeLength,
            out tokenType);
    }
}