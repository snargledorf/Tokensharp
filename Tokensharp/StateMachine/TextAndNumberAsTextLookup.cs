using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextAndNumberAsTextLookup<TTokenType>(ITokenTreeNode<TTokenType> tokenTreeNode)
    : TextWhiteSpaceNumberLookupBase<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly WhiteSpace<TTokenType> _whiteSpaceState = new(tokenTreeNode);
    private readonly TextAndNumberAsTextState<TTokenType> _textAndNumberAsTextState = new(tokenTreeNode);

    public override TextWhiteSpaceNumberBase<TTokenType> GetState(in char c)
    {
        if (char.IsWhiteSpace(c))
            return _whiteSpaceState;
        return _textAndNumberAsTextState;
    }
}