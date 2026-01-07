namespace Tokensharp.TokenTree;

public sealed class TokenTreeNode<TTokenType>(char key, TokenTreeNode<TTokenType>? parent = null) : TokenTreeNodeCollection<TTokenType>
{
    private TTokenType? _tokenType;

    public char Key { get; } = key;

    public TokenTreeNode<TTokenType>? Parent { get; } = parent;
    
    public TokenTreeNode<TTokenType> RootNode => Parent?.RootNode ?? this;

    public TTokenType? TokenType
    {
        get => _tokenType;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            
            if (_tokenType is not null)
                throw new InvalidOperationException("Node already has a token type");

            _tokenType = value;
        }
    }

    public bool HasChildren => Count > 0;

    public override string ToString()
    {
        return Key.ToString();
    }
}