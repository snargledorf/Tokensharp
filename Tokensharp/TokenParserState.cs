using Tokensharp.StateMachine;

namespace Tokensharp;

public readonly struct TokenParserState<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal readonly StartState<TTokenType>? StartState;

    public TokenParserState(TokenConfiguration<TTokenType> tokenConfiguration) 
        : this(tokenConfiguration.StartState)
    {
    }

    internal TokenParserState(StartState<TTokenType> startState)
    {
        StartState = startState;
    }
}