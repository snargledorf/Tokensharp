using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal sealed class Number<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberBase<TTokenType>(rootNode, TokenType<TTokenType>.Number)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool CharacterIsValidForState(in char c) => char.IsDigit(c);
}