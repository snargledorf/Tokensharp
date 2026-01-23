using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(State<TTokenType> fallbackState, IStateCharacterCheck fallbackStateCharacterCheck)
    : State<TTokenType>, IStateCharacterCheck where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override TransitionResult TransitionResult => TransitionResult.NewState;

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out State<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out State<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }

    protected override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public bool CharacterIsValidForState(char c) => fallbackStateCharacterCheck.CharacterIsValidForState(c);
}