using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextAndNumberAsTextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberBase<TTokenType>(rootNode, TokenType<TTokenType>.Text)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(in char c) => !char.IsWhiteSpace(c);
}