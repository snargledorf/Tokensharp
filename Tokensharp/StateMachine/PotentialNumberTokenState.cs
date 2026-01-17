using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class PotentialNumberTokenState<TTokenType>(
    ITokenTreeNode<TTokenType> node,
    IState<TTokenType> defaultState,
    NumberState<TTokenType> numberState,
    WhiteSpaceState<TTokenType> whiteSpaceState,
    TextState<TTokenType> textState)
    : PotentialTokenState<TTokenType>(node, defaultState, whiteSpaceState, numberState, textState)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
}