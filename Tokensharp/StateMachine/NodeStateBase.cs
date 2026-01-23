using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class NodeStateBase<TTokenType>(ITokenTreeNode<TTokenType> node)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override TransitionResult TransitionResult => TransitionResult.NewState;
    
    protected ITokenTreeNode<TTokenType> Node { get; } = node;

    private IStateLookup<TTokenType>? _stateLookup;
    
    internal IStateLookup<TTokenType> StateLookup { set => _stateLookup = value; }

    protected bool TryGetStateForChildNode(char c, [NotNullWhen(true)] out State<TTokenType>? state) =>
        _stateLookup!.TryGetState(c, out state);

    public override string ToString()
    {
        return $"{GetType().Name} Node: {Node}";
    }
}