namespace Tokensharp;

public class DuplicateLexemeException(string lexeme) : Exception
{
    public string Lexeme { get; } = lexeme;
}