using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfSingleCharacterTokenState<TTokenType>(TTokenType tokenType) 
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength, ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        confirmedLexemeLength = 1;
        
        tokenType = TokenType;
        return true;
    }
}