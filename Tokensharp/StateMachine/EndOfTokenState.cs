using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType>(TTokenType tokenType)
    : State<TTokenType>, IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => this;

    public override TTokenType TokenType { get; } = tokenType;

    public override bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength,
        ref int confirmedLexemeLength, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        if (fallbackLexemeLength > 0)
            confirmedLexemeLength = fallbackLexemeLength;
        else
            confirmedLexemeLength = potentialLexemeLength;

        tokenType = TokenType;
        return true;
    }

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = null;
        return false;
    }

    public override bool CharacterIsValidForState(char c)
    {
        return false;
    }
}