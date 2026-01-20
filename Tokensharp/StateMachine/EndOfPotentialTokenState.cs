namespace Tokensharp.StateMachine;

internal class EndOfPotentialTokenState<TTokenType>(TTokenType tokenType)
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override void UpdateCounts(ref StateMachineContext context)
    {
        context.PotentialLexemeLength++;
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }
}