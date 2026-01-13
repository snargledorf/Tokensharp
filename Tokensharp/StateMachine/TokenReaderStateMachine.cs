using SwiftState;

namespace Tokensharp.StateMachine;

public class TokenReaderStateMachine<TTokenType> : TokenParserStateMachine<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private TokenReaderStateMachine(State<char, TokenizerStateId<TTokenType>> startState) : base(startState)
    {
    }

    public new static TokenReaderStateMachine<TTokenType> Default { get; } = For(TTokenType.Configuration);

    public new static TokenReaderStateMachine<TTokenType> For(ITokenConfiguration<TTokenType> tokenConfiguration)
    {
        State<char, TokenizerStateId<TTokenType>> startState =
            TokenReaderStateMachineFactory<TTokenType>.BuildStateMachineStartState(tokenConfiguration);

        return new TokenReaderStateMachine<TTokenType>(startState);
    }
}