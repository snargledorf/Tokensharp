using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

internal interface ITokenTreeNodeCollection<TValue> : IReadOnlyCollection<TokenTreeNode<TValue>>
    where TValue : TokenType<TValue>, ITokenType<TValue>
{
    bool TryGetChild(char key, [MaybeNullWhen(false)] out TokenTreeNode<TValue> node);
    void AddChild(TokenTreeNode<TValue> node);
}