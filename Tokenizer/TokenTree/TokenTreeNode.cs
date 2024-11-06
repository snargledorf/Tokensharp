namespace Tokenizer.TokenTree;

public sealed class TokenTreeNode<TState>(char key, TokenTreeNode<TState>? parent = null) : TokenTreeNodeCollection<TState>
{
    private TState? _state;

    public char Key { get; } = key;

    public TokenTreeNode<TState>? Parent { get; } = parent;
    
    public TokenTreeNode<TState> RootNode => Parent?.Parent ?? this;

    public TState? State
    {
        get => _state;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            
            if (_state is not null)
                throw new InvalidOperationException("Node already has a state");

            _state = value;
        }
    }

    public bool HasChildren => Count > 0;

    public override string ToString()
    {
        return Key.ToString();
    }
}