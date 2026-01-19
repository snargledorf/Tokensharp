using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType>(TTokenType tokenType)
    : State<TTokenType>, IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => this;

    public TTokenType TokenType { get; } = tokenType;

    public override void OnEnter(StateMachineContext context)
    {
        if (context.FallbackLexemeLength > 0)
            context.ConfirmedLexemeLength = context.FallbackLexemeLength;
        else
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
    }

    protected override bool TryGetNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = null;
        return false;
    }

    public override bool CharacterIsValidForState(char c)
    {
        return false;
    }
}