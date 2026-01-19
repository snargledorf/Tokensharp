using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface IState<TTokenType> : ITransitionHandler<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    TTokenType? TokenType { get; }
    bool CharacterIsValidForState(char c);

    bool UpdateCounts(ref int potentialLexemeLength, ref int fallbackLexemeLength, ref int confirmedLexemeLength,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType);
}