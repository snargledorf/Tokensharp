using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class FoundTokenState<TTokenType>(EndOfTokenState<TTokenType> endOfTokenState) : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        nextState = endOfTokenState;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = endOfTokenState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        // NoOp
    }
}