using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

public interface ITokenTreeNodeCollection<TValue> : IReadOnlyCollection<TokenTreeNode<TValue>>
{
    bool TryGetChild(char key, [MaybeNullWhen(false)] out TokenTreeNode<TValue> node);
    void AddChild(TokenTreeNode<TValue> node);
}