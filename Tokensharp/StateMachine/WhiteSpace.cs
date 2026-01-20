using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpace<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberBase<TTokenType>(rootNode, TokenType<TTokenType>.WhiteSpace)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(char c) => char.IsWhiteSpace(c);
}