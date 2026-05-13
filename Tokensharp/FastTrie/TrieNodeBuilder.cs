using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.FastTrie;

internal sealed class TrieNodeBuilder<T>(char character, T? value = null) where T : class
{
    private readonly Dictionary<char, TrieNodeBuilder<T>> _characterToTrieNodeBuilders = new();
    
    public T? Value { get; set; } = value;

    public TrieNodeBuilder<T> Add(char character)
    {
        var trieNodeBuilder = new TrieNodeBuilder<T>(character);
        _characterToTrieNodeBuilders.Add(character, trieNodeBuilder);
        return trieNodeBuilder;
    }
    
    public TrieNodeBuilder<T> Add(char character, T nodeValue)
    {
        var trieNodeBuilder = new TrieNodeBuilder<T>(character, nodeValue);
        _characterToTrieNodeBuilders.Add(character, trieNodeBuilder);
        return trieNodeBuilder;
    }

    public bool TryGetChild(char c, [NotNullWhen(true)] out TrieNodeBuilder<T>? trieNodeBuilder)
    {
        return _characterToTrieNodeBuilders.TryGetValue(c, out trieNodeBuilder);
    }

    public bool HasNodeForCharacter(char character)
    {
        return _characterToTrieNodeBuilders.ContainsKey(character);
    }

    public IEnumerable<char> Characters => _characterToTrieNodeBuilders.Keys;

    public TrieNode<T> Build()
    {
        Dictionary<char, TrieNode<T>> trieNodes = _characterToTrieNodeBuilders.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Build());
        
        return new TrieNode<T>(trieNodes, Value);
    }
}