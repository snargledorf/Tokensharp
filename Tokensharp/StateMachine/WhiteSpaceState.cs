using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpaceState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode, TokenType<TTokenType>.WhiteSpace)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(char c) => char.IsWhiteSpace(c);
}