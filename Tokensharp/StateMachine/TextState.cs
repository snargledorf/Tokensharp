using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class TextState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static TextState<TTokenType>? _instance;

    protected override TTokenType TokenType => TokenType<TTokenType>.Text;
    protected override EndOfTokenState<TTokenType> EndOfTokenState { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Text);

    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => field ??= WhiteSpaceState<TTokenType>.For(RootNode);

    internal override NumberState<TTokenType> NumberStateInstance => field ??= NumberState<TTokenType>.For(RootNode);

    internal override TextState<TTokenType> TextStateInstance => this;
    
    internal static TextState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        return _instance = new TextState<TTokenType>(treeNode.RootNode);
    }

    protected override bool CharacterIsValidForToken(char c)
    {
        return !char.IsWhiteSpace(c) && !char.IsDigit(c);
    }
}