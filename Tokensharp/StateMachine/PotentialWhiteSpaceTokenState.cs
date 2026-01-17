using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialWhiteSpaceTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> defaultState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState,
    TextState<TTokenType> textState)
    : PotentialTokenState<TTokenType>(node, defaultState, whiteSpaceState, numberState, textState) 
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
}