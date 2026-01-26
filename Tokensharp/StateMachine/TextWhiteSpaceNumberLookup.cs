using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextWhiteSpaceNumberLookup<TTokenType>(ITokenTreeNode<TTokenType> tokenTreeNode)
    : TextWhiteSpaceNumberLookupBase<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly WhiteSpace<TTokenType> _whiteSpaceState = new(tokenTreeNode);
    private readonly Number<TTokenType> _numberState = new(tokenTreeNode);
    private readonly Text<TTokenType> _textState = new(tokenTreeNode);

    public override TextWhiteSpaceNumberBase<TTokenType> GetState(in char c)
    {
        if (char.IsWhiteSpace(c))
            return _whiteSpaceState;
        if (char.IsDigit(c))
            return _numberState;
        return _textState;
    }
}