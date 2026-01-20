using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

internal record TokenTreeNode<TTokenType>(
    char Character,
    TTokenType? TokenType)
    : ITokenTreeNode<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private FrozenDictionary<char, ITokenTreeNode<TTokenType>>? _children;

    private FrozenDictionary<char, ITokenTreeNode<TTokenType>> Children =>
        _children ?? throw new InvalidOperationException("Children not initialized");

    public int Count => Children.Count;
    public ITokenTreeNode<TTokenType>? Parent { get; private set; }
    public ITokenTreeNode<TTokenType> RootNode => Parent?.RootNode ?? this;

    public bool TryGetChild(char key, [MaybeNullWhen(false)] out ITokenTreeNode<TTokenType> node) =>
        Children.TryGetValue(key, out node);
    
    public void Initialize(FrozenDictionary<char, ITokenTreeNode<TTokenType>> children, ITokenTreeNode<TTokenType>? parent)
    {
        _children = children;
        Parent = parent;
    }

    public IEnumerator<ITokenTreeNode<TTokenType>> GetEnumerator()
    {
        return Children.Values.Select(cn => cn).GetEnumerator();
    }

    public override string ToString()
    {
        return string.Concat(Parent?.ToString(), Character.ToString());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}