using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode, TokenType<TTokenType>.Text)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(char c) => !char.IsWhiteSpace(c) && !char.IsDigit(c);
}