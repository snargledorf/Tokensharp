using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MixedCharacterFailedTokenCheckState<TTokenType>(IState<TTokenType> fallbackState)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        return TryGetDefaultState(out nextState);
    }

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = fallbackState;
        return true;
    }
    
    public override void OnEnter(StateMachineContext context)
    {
        context.FallbackLexemeLength = context.PotentialLexemeLength;
    }

    public override bool CharacterIsValidForState(char c) => fallbackState.CharacterIsValidForState(c);
}