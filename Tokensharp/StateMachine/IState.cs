using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface IStateCharacterCheck
{
    bool CharacterIsValidForState(char c);
}

internal interface IState<TTokenType> : ITransitionHandler<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool IsEndOfToken { get; }
    void UpdateCounts(ref StateMachineContext context);

    bool FinalizeToken(ref StateMachineContext context, out int lexemeLength, out TokenType<TTokenType> tokenType);
}