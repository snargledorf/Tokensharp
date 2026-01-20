using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class Text<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberBase<TTokenType>(rootNode, TokenType<TTokenType>.Text)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(char c) => !char.IsWhiteSpace(c) && !char.IsDigit(c);
}