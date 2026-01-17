using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.TokenTree;

internal class TokenTreeNodeBuilder<TTokenType>(
    char character,
    ITokenTreeNodeBuilder<TTokenType>? parentBuilder = null)
    : ITokenTreeNodeBuilder<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, ITokenTreeNodeBuilder<TTokenType>> _children = [];

    public TTokenType? TokenType { get; set; }

    public ITokenTreeNodeBuilder<TTokenType> AddChild(char childCharacter)
    {
        var treeNodeBuilder = new TokenTreeNodeBuilder<TTokenType>(childCharacter, this);
        _children.Add(childCharacter, treeNodeBuilder);
        return treeNodeBuilder;
    }

    public bool TryGetChild(char key, [NotNullWhen(true)] out ITokenTreeNodeBuilder<TTokenType>? node)
    {
        return _children.TryGetValue(key, out node);
    }

    private TokenTreeNode<TTokenType>? _builtNode;

    public ITokenTreeNode<TTokenType> Build()
    {
        if (_builtNode is not null)
            return _builtNode;
        
        _builtNode = new TokenTreeNode<TTokenType>(character, TokenType);

        FrozenDictionary<char, ITokenTreeNode<TTokenType>> childrenFrozenDict =
            _children.ToFrozenDictionary(child => child.Key, child => child.Value.Build());
        ITokenTreeNode<TTokenType>? parent = parentBuilder?.Build();

        _builtNode.Initialize(childrenFrozenDict, parent);

        return _builtNode;
    }
}