using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType> : State<TTokenType>, IEndOfTokenAccessorState<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly Dictionary<TTokenType, EndOfTokenState<TTokenType>> _instances = new();
    
    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => this;

    private EndOfTokenState(TTokenType tokenType)
    {
        TokenType = tokenType;
    }

    public TTokenType TokenType { get; }

    public static EndOfTokenState<TTokenType> For(TTokenType tokenType)
    {
        return _instances.GetOrAdd<TTokenType, EndOfTokenState<TTokenType>>(tokenType, tt => new EndOfTokenState<TTokenType>(tt));
    }

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