namespace Tokensharp.TokenTree;

public sealed class TokenTreeNode<TTokenType>(char key, TokenTree<TTokenType> tree, TokenTreeNode<TTokenType>? parent = null) : TokenTreeNodeCollection<TTokenType>
{
    public char Key { get; } = key;

    public TokenTreeNode<TTokenType>? Parent { get; } = parent;
    
    public TokenTreeNode<TTokenType> RootNode => Parent?.RootNode ?? this;

    public TokenTree<TTokenType> Tree { get; } = tree;

    public TTokenType? TokenType
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (field is not null)
                throw new InvalidOperationException("Node already has a token type");

            field = value;
        }
    }

    public bool HasChildren => Count > 0;

    public override string ToString()
    {
        return Key.ToString();
    }
}