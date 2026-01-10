using Tokensharp.StateMachine;

namespace Tokensharp.TokenTree;

internal sealed class TokenTreeNode<TTokenType>(char key, TokenizerStateId<TTokenType> stateId, TokenTree<TTokenType> tree, TokenTreeNode<TTokenType>? parent = null) : TokenTreeNodeCollection<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public char Key { get; } = key;

    public TokenTreeNode<TTokenType>? Parent { get; } = parent;
    
    public TokenTreeNode<TTokenType> RootNode => Parent?.RootNode ?? this;

    public TokenTree<TTokenType> Tree { get; } = tree;

    public TokenizerStateId<TTokenType> StateId { get; } = stateId;

    public bool HasChildren => Count > 0;
    public bool IsEndOfToken { get; set; }

    public override string ToString()
    {
        return Key.ToString();
    }
}