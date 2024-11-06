using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Tokenizer.TokenTree;

public class TokenTreeNodeCollection<TState> : ITokenTreeNodeCollection<TState>
{
    private readonly Dictionary<char, TokenTreeNode<TState>> _children = [];

    public IEnumerator<TokenTreeNode<TState>> GetEnumerator()
    {
        return _children.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryGetChild(char key, [MaybeNullWhen(false)] out TokenTreeNode<TState> node)
    {
        return _children.TryGetValue(key, out node);
    }

    public void AddChild(TokenTreeNode<TState> node)
    {
        _children.Add(node.Key, node);
    }

    public int Count => _children.Count;
}