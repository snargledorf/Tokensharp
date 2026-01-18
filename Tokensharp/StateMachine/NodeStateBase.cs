using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class NodeStateBase<TTokenType>(ITokenTreeNode<TTokenType> node)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected ITokenTreeNode<TTokenType> Node { get; } = node;

    protected bool TryGetStateForChildNode(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        if (Node.TryGetChild(c, out ITokenTreeNode<TTokenType>? child))
        {
            state = CreateStateForChildNode(child);
            AddStateToCache(c, state);
            return true;
        }

        state = null;
        return false;
    }

    protected abstract IState<TTokenType> CreateStateForChildNode(ITokenTreeNode<TTokenType> childNode);

    public override string ToString()
    {
        return $"{GetType().Name} Node: {Node}";
    }
}