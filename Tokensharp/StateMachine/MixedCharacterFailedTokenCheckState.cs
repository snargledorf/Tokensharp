namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(IState<TTokenType> fallbackState, IStateCharacterCheck fallbackStateCharacterCheck)
    : State<TTokenType>, IStateCharacterCheck where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken => false;

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        nextState = fallbackState;
        return true;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = fallbackState;
        return true;
    }

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public bool CharacterIsValidForState(in char c) => fallbackStateCharacterCheck.CharacterIsValidForState(c);
}