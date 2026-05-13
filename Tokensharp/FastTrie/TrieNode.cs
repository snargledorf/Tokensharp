using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.FastTrie;

internal sealed class TrieNode<T>
{
    private readonly TrieNode<T>?[]? _childNodes;
    private readonly int _maxChar;
    private readonly int _offset;

    public TrieNode(IReadOnlyDictionary<char, TrieNode<T>> characterToNode, T? value)
    {
        Value = value;
        HasValue = value is not null;
        HasChildren = characterToNode.Count > 0;
        
        if (!HasChildren)
            return;
        
        _offset = characterToNode.Min(x => (int)x.Key);
        _maxChar = characterToNode.Max(x => (int)x.Key);
        
        _childNodes = new TrieNode<T>[_maxChar - _offset + 1];
        foreach (KeyValuePair<char, TrieNode<T>> keyValuePair in characterToNode)
            _childNodes[keyValuePair.Key - _offset] = keyValuePair.Value;
    }

    public readonly T? Value;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }

    public readonly bool HasChildren;

    public bool TryGetChildNode(char c, [NotNullWhen(true)] out TrieNode<T>? state)
    {
        if (!HasChildren || c < _offset || c > _maxChar)
        {
            state = null;
            return false;
        }

        state = _childNodes![c - _offset];
        return state != null;
    }
}