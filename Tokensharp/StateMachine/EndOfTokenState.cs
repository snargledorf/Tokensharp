using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType> : State<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected EndOfTokenState(TTokenType tokenType)
    {
        TokenType = tokenType;
    }

    private static readonly Dictionary<TTokenType, EndOfTokenState<TTokenType>> Instances = new();
    
    public TTokenType TokenType { get; }
    
    public static EndOfTokenState<TTokenType> For(TTokenType tokenType)
    {
        return Instances.GetOrAdd(tokenType, tt => new EndOfTokenState<TTokenType>(tt));
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        nextState = null;
        return false;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        if (context is { PotentialLexemeLength: > 0, ConfirmedLexemeLength: 0 })
            context.ConfirmedLexemeLength = context.PotentialLexemeLength;
    }
}