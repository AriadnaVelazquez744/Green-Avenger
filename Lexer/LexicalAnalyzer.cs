using System.Diagnostics.CodeAnalysis;

public class LexicalAnalyzer
{
    public Dictionary<string, string> Operators { get; set; }
    public Dictionary<string, string> KeyWords { get; set; }
    public Dictionary<string, string> Texts { get; set; }

    public LexicalAnalyzer()
    {
        Operators = new Dictionary<string, string>();
        KeyWords = new Dictionary<string, string>();
        Texts = new Dictionary<string, string>();
    }

    public IEnumerable<Token> GetTokens(string fileName, string code, List<CompilingError> errors)
    {
        List<Token> tokens = new();
        TokenReader stream = new(fileName, code);

        while (!stream.EOF)
        {
            string value;

            if (stream.ReadWhiteSpace())
                continue;

            if (stream.ReadId(out value))
            {
                if (KeyWords.ContainsKey(value))
                    tokens.Add(new Token(TokenType.KeyWord, KeyWords[value], stream.Location));

                else
                    tokens.Add(new Token(TokenType.Identifier, value, stream.Location));

                continue;
            }

            if (stream.ReadNumber(out value))
            {
                double d;
                if (!double.TryParse(value, out d))
                    errors.Add(new CompilingError(stream.Location, ErrorCode.Invalid, "Number format"));

                tokens.Add(new Token(TokenType.Number, value, stream.Location));

                continue;
            }

            if (MatchText(stream, tokens, errors))
                continue;

            if (MatchSymbol(stream, tokens))
                continue;

            var unkOp = stream.ReadAny();
            errors.Add(new CompilingError(stream.Location, ErrorCode.Unknown, unkOp.ToString()));
        }

        return tokens;
    }

    private bool MatchSymbol(TokenReader stream, List<Token> tokens)
    {
        foreach (var op in Operators.Keys.OrderByDescending(k => k.Length))
        {
            if (stream.Match(op))
            {
                tokens.Add(new Token(TokenType.Symbol, Operators[op], stream.Location));
                return true;
            }
        }
        return false;
    }

    private bool MatchText(TokenReader stream, List<Token> tokens, List<CompilingError> errors)
    {
        string text;
        foreach (var start in Texts.Keys.OrderByDescending(k => k.Length))
        {
            if (!stream.ReadUntil(Texts[start], out text))
                errors.Add(new CompilingError(stream.Location, ErrorCode.Expected, Texts[start]));

            tokens.Add(new Token(TokenType.Text, text, stream.Location));
            return true;
        }
        return false;
    }
}

class TokenReader
{
    readonly string fileName;
    readonly string code;
    int pos;
    int line;
    int lastLB;
    public TokenReader(string fileName, string code)
    {
        this.fileName = fileName;
        this.code = code;
        this.pos = 0;
        this.line = 1;
        this.lastLB = -1;
    }

    public bool EOF => (pos >= code.Length);
    public bool EOL => (EOF || code[pos] == '\n');

    public CodeLocation Location
    {
        get
        {
            return new CodeLocation
            {
                File = fileName,
                Line = line,
                Column = pos - lastLB
            };
        }
    }

    public char Peek()
    {
        if (pos < 0 || pos >= code.Length)
            throw new InvalidOperationException();
        
        return code[pos];
    }

    public bool Match(string prefix)
    {
        if (ContinuesWith(prefix))
        {
            pos += prefix.Length;
            return true;
        }
        return false;
    }

    public bool ReadWhiteSpace()
    {
        if (char.IsWhiteSpace(Peek()))
        {
            ReadAny();
            return true;
        }
        return false;
    }

    public bool ReadId(out string id)
    {
        id = "";
        while (!EOL && ValidIdCharacter(Peek(), id.Length == 0))
            id += ReadAny();
        return id.Length > 0;
    }

    public bool ReadNumber(out string number)
    {
        number = "";

        while (!EOL && char.IsDigit(Peek()))
            number += ReadAny();
        if (!EOL && Match("."))
        {
            number += '.';
            while (!EOL && char.IsDigit(Peek()))
                number += ReadAny();
        }

        if (number.Length == 0)
            return false;

        while (!EOL && char.IsLetterOrDigit(Peek()))
            number += ReadAny();

        return number.Length > 0;
    }

    public bool ReadUntil(string end, out string text)
    {
        text = "";
        while (!Match(end))
        {
            if (EOL || EOF)
                return false;
            text += ReadAny();
        }
        return true;
    }

    public char ReadAny()
    {
        if (EOF) 
            throw new InvalidOperationException();
        if (EOL)
        {
            line++;
            lastLB = pos;
        }
        return code[pos++];
    }

    private bool ContinuesWith(string prefix)
    {
        if (pos + prefix.Length > code.Length)  
            return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (code[pos + 1] != prefix[i])     
                return false;
        }

        return true;
    }
    private bool ValidIdCharacter(char c, bool beginning)
    {
        return (c == '_') || (beginning ? char.IsLetter(c) : char.IsLetterOrDigit(c));
    }
}