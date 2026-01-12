using SwiftState;

namespace Tokensharp.StateMachine
{
    public class TokenReaderStateMachine<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        internal TokenReaderStateMachine(State<char, TokenizerStateId<TTokenType>> startState)
        {
            StartState = startState;
        }

        public static TokenReaderStateMachine<TTokenType> Default { get; } = For(TTokenType.Configuration);

        internal State<char, TokenizerStateId<TTokenType>> StartState { get; }

        public static TokenReaderStateMachine<TTokenType> For(ITokenConfiguration<TTokenType> tokenConfiguration)
        {
            return TokenReaderStateMachineFactory<TTokenType>.BuildStateMachine(tokenConfiguration);
        }
    }
}
