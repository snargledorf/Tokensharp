namespace Tokensharp.StateMachine;

internal class StateMachineContext
{
    public int ConfirmedLexemeLength;
    public int PotentialLexemeLength;
    public int FallbackLexemeLength;

    public void Reset()
    {
        ConfirmedLexemeLength = 0;
        PotentialLexemeLength = 0;
        FallbackLexemeLength = 0;
    }
}