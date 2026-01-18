using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static TextState<TTokenType>? _instance;

    public override EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);

    protected override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => field ??= WhiteSpaceState<TTokenType>.For(Node);

    protected override NumberState<TTokenType> NumberStateInstance => field ??= NumberState<TTokenType>.For(Node);

    protected override TextState<TTokenType> TextStateInstance => this;
    
    internal static TextState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        return _instance = new TextState<TTokenType>(treeNode.RootNode);
    }

    public override bool CharacterIsValidForState(char c)
    {
        return !char.IsWhiteSpace(c) && !char.IsDigit(c);
    }
}