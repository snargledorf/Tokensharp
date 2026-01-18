using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

internal interface ITokenTreeNode<TTokenType> : IEnumerable<ITokenTreeNode<TTokenType>>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public char Character { get; }
    public TTokenType? TokenType { get; }
    ITokenTreeNode<TTokenType>? Parent { get; }
    ITokenTreeNode<TTokenType> RootNode { get; }
    public int Count { get; }
    public bool HasChildren => Count > 0;
    [MemberNotNullWhen(true, nameof(TokenType))]
    public bool IsEndOfToken => TokenType is not null;
    bool TryGetChild(char key, [MaybeNullWhen(false)] out ITokenTreeNode<TTokenType> node);
}