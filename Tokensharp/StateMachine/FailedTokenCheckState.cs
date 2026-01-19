namespace Tokensharp.StateMachine;

internal class FailedTokenCheckState<TTokenType>(IEndOfTokenAccessorState<TTokenType> fallbackState)
    : MixedCharacterFailedTokenCheckState<TTokenType>(fallbackState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void OnEnter(StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        base.OnEnter(context);
    }
}