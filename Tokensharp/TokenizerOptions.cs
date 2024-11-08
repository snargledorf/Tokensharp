namespace Tokensharp;

public class TokenizerOptions
{
    private int _defaultBufferSize = 1024;

    public int DefaultBufferSize
    {
        get => _defaultBufferSize;
        set
        {
            if (value < 1)
                throw new ArgumentException("Buffer size must be greater than zero.");
            
            _defaultBufferSize = value;
        }
    }
}