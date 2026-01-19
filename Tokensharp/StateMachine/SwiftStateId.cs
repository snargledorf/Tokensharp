namespace Tokensharp.StateMachine;

internal readonly record struct SwiftStateId<TTokenType>(IState<TTokenType> State) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;