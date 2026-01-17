using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class FoundTokenState<TTokenType>(IState<TTokenType> fallbackState) : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        nextState = fallbackState;
        return true;
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        // NoOp
    }
}