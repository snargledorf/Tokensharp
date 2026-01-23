namespace Tokensharp.StateMachine;

internal class FailedTokenCheckState<TTokenType>(State<TTokenType> fallbackState, IStateCharacterCheck fallbackStateCharacterCheck)
    : MixedCharacterFailedTokenCheckState<TTokenType>(fallbackState, fallbackStateCharacterCheck) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        base.UpdateCounts(ref context);
    }
}