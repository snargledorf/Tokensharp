using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextWhiteSpaceNumberLookup<TTokenType> : TextWhiteSpaceNumberLookupBase<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly WhiteSpace<TTokenType> _whiteSpaceState;
    private readonly Number<TTokenType> _numberState;
    private readonly Text<TTokenType> _textState;

    public TextWhiteSpaceNumberLookup(ITokenTreeNode<TTokenType> tokenTreeNode)
    {
        _whiteSpaceState = new WhiteSpace<TTokenType>(tokenTreeNode);
        _numberState = new Number<TTokenType>(tokenTreeNode);
        _textState = new Text<TTokenType>(tokenTreeNode);

        IStateLookup<TTokenType> compiledTextWhiteSpaceNumberStates = BuildStateLookup(tokenTreeNode);

        _whiteSpaceState.StateLookup = compiledTextWhiteSpaceNumberStates;
        _numberState.StateLookup = compiledTextWhiteSpaceNumberStates;
        _textState.StateLookup = compiledTextWhiteSpaceNumberStates;
    }

    public override TextWhiteSpaceNumberBase<TTokenType> GetState(in char c)
    {
        if (char.IsWhiteSpace(c))
            return _whiteSpaceState;
        if (char.IsDigit(c))
            return _numberState;
        return _textState;
    }
}