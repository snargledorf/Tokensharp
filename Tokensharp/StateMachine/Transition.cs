namespace Tokensharp.StateMachine;

public record struct Transition<TTokenType>(Func<char, bool> Condition, TokenizerState<TTokenType> State)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;