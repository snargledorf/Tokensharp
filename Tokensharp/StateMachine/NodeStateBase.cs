using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class NodeStateBase<TTokenType>(ITokenTreeNode<TTokenType> node)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken => false;
    
    protected ITokenTreeNode<TTokenType> Node { get; } = node;

    private IStateLookup<TTokenType>? _stateLookup;
    
    internal IStateLookup<TTokenType> StateLookup { set => _stateLookup = value; }

    protected override bool TryGetNextState(in char c, out IState<TTokenType> nextState)
    {
        if (_stateLookup!.TryGetState(c, out nextState!))
            return true;

        nextState = null!;
        return false;
    }

    public override string ToString()
    {
        return $"{GetType().Name} Node: {Node}";
    }
}