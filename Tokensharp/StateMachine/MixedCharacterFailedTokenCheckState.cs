namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(State<TTokenType> fallbackState, IStateCharacterCheck fallbackStateCharacterCheck)
    : State<TTokenType>, IStateCharacterCheck where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken => false;

    protected override State<TTokenType> GetNextState(char c)
    {
        return fallbackState;
    }

    protected override State<TTokenType> DefaultState => fallbackState;

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public bool CharacterIsValidForState(char c) => fallbackStateCharacterCheck.CharacterIsValidForState(c);
}