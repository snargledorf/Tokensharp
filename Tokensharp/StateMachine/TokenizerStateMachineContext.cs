using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal record TokenizerStateMachineContext<TTokenType>(
    TokenTree<TTokenType> Tree,
    TokenizerState<TTokenType> WhiteSpaceState,
    TokenizerState<TTokenType> TextState,
    TokenizerState<TTokenType> NumberState) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>;