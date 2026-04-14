namespace Tokensharp.StateMachine;

internal sealed class EndOfPotentialTokenState<TTokenType>(TTokenType tokenType)
    : EndOfTokenState<TTokenType>(tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override void UpdateCounts(ref StateMachineContext context)
    {
        int contextPotentialLexemeLength = context.PotentialLexemeLength+1;
        context = new StateMachineContext(contextPotentialLexemeLength, contextPotentialLexemeLength);
    }
}