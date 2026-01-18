using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);

    public override bool CharacterIsValidForState(char c) => !char.IsWhiteSpace(c) && !char.IsDigit(c);
}