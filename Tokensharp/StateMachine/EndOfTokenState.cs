using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class EndOfTokenState<TTokenType>(TTokenType tokenType)
    : State<TTokenType>, IEndOfTokenStateAccessor<TTokenType>, IStateCharacterCheck
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public TTokenType TokenType { get; } = tokenType;

    public override bool IsEndOfToken => true;

    public EndOfTokenState<TTokenType> EndOfTokenStateInstance => this;

    public override void UpdateCounts(ref StateMachineContext context)
    {
        // NoOp
    }

    public override bool FinalizeToken(ref StateMachineContext context, out int length, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType)
    {
        length = context.FallbackLexemeLength > 0 ? context.FallbackLexemeLength : context.PotentialLexemeLength;
        tokenType = TokenType;
        return true;
    }

    protected override bool TryGetNextState(char c, out IState<TTokenType> nextState)
    {
        nextState = this;
        return false;
    }

    protected override bool TryGetDefaultState(out IState<TTokenType> defaultState)
    {
        defaultState = this;
        return false;
    }

    public bool CharacterIsValidForState(char c)
    {
        return false;
    }
}