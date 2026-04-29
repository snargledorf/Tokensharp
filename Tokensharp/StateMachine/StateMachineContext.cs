namespace Tokensharp.StateMachine;

internal readonly ref struct StateMachineContext(int potentialLexemeLength, int fallbackLexemeLength)
{
    public readonly int PotentialLexemeLength = potentialLexemeLength;
    public readonly int FallbackLexemeLength = fallbackLexemeLength;
}