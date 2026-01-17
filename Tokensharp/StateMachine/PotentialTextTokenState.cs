using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialTextTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> defaultState,
    TextState<TTokenType> textState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    NumberState<TTokenType> numberState)
    : PotentialTokenState<TTokenType>(node, defaultState, whiteSpaceState, numberState, textState)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
}