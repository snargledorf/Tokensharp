namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(IState<TTokenType> fallbackState, IStateCharacterCheck fallbackStateCharacterCheck)
    : State<TTokenType>, IStateCharacterCheck where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken => false;

    protected override IState<TTokenType> GetNextState(in char c)
    {
        return fallbackState;
    }

    protected override IState<TTokenType> DefaultState => fallbackState;

    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public bool CharacterIsValidForState(in char c) => fallbackStateCharacterCheck.CharacterIsValidForState(in c);
}