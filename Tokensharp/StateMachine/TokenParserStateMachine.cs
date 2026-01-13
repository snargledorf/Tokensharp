using SwiftState;

namespace Tokensharp.StateMachine
{
    public class TokenParserStateMachine<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        internal TokenParserStateMachine(State<char, TokenizerStateId<TTokenType>> startState)
        {
            StartState = startState;
        }

        public static TokenParserStateMachine<TTokenType> Default { get; } = For(TTokenType.Configuration);

        internal State<char, TokenizerStateId<TTokenType>> StartState { get; }

        public static TokenParserStateMachine<TTokenType> For(ITokenConfiguration<TTokenType> tokenConfiguration)
        {
            State<char, TokenizerStateId<TTokenType>> startState =
                TokenReaderStateMachineFactory<TTokenType>.BuildStateMachineStartState(tokenConfiguration);
            
            return new TokenParserStateMachine<TTokenType>(startState);
        }
    }
}
