using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextAndNumberAsTextLookup<TTokenType> : TextWhiteSpaceNumberLookupBase<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly WhiteSpace<TTokenType> _whiteSpaceState;
    private readonly TextAndNumberAsTextState<TTokenType> _textAndNumberAsTextState;

    public TextAndNumberAsTextLookup(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        _whiteSpaceState = new WhiteSpace<TTokenType>(tokenTreeNode);
        _textAndNumberAsTextState = new TextAndNumberAsTextState<TTokenType>(tokenTreeNode);

        IStateLookup<TTokenType> compiledTextWhiteSpaceNumberStates = BuildStateLookup(tokenTreeNode);

        _whiteSpaceState.StateLookup = compiledTextWhiteSpaceNumberStates;
        _textAndNumberAsTextState.StateLookup = compiledTextWhiteSpaceNumberStates;
    }

    public override TextWhiteSpaceNumberBase<TTokenType> GetState(in char c)
    {
        if (char.IsWhiteSpace(c))
            return _whiteSpaceState;
        return _textAndNumberAsTextState;
    }
}