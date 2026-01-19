using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(IState<TTokenType> fallbackState)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }

    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        fallbackLexemeLength = potentialLexemeLength;
        tokenType = null;
        return false;
    }

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);
}