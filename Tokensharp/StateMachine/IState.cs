using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface IStateCharacterCheck
{
    bool CharacterIsValidForState(char c);
}

internal interface IState<TTokenType> : ITransitionHandler<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool IsEndOfToken(ref StateMachineContext context, out int lexemeLength,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType);
}