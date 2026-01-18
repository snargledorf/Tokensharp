using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType> : State<TTokenType>, IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => this;

    private EndOfTokenState(TTokenType tokenType)
    {
        TokenType = tokenType;
    }

    public TTokenType TokenType { get; }


    public static EndOfTokenState<TTokenType> For(TTokenType tokenType)
    {
        return new EndOfTokenState<TTokenType>(tokenType);
    }

    public override void OnEnter(StateMachineContext context)
    {
        if (context.FallbackLexemeLength > 0)
            context.ConfirmedLexemeLength = context.FallbackLexemeLength;
        else
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
    }

    public override bool CharacterIsValidForState(char c)
    {
        return false;
    }
}