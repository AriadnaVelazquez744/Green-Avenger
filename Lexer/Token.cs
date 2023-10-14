public class Token
{
    public string Value { get; private set; }
    public TokenType Type { get; private set; }
    public CodeLocation Location { get; private set; }

    public Token(TokenType type, string value, CodeLocation location)
    {
        this.Type = type;
        this.Value = value;
        this.Location = location;
    }

    public override string ToString()
    {
        return $"Token({Type}, {Value})";
    }
}

public struct CodeLocation
{
    public string File;
    public int Line;
    public int Column;
}

public enum TokenType
{
    Number,
    Text,
    KeyWord,
    Identifier,
    Symbol,
    ArithmeticOperator,
    BooleanOperator,
    BooleanExpression,
    Conditional,
    ElementalFunctions,
    Constants,
}

public class TokenValues
{
    protected TokenValues() { }

    //TokenType.ArithmeticOperator
    public const string Add = "Addition"; // +
    public const string Sub = "Subtract"; // -
    public const string Mul = "Multiplication"; // *
    public const string Div = "Division"; // /
    public const string Pow = "Power"; // ^

    //TokenType.Symbol
    public const string Assign = "Assign"; // =
    public const string Reassign = "Reassign"; // :=
    public const string ValueSeparator = "ValueSeparator"; // ,
    public const string StatementSeparator = "StatementSeparator"; // ;
    public const string OpenBracket = "OpenBracket"; // (
    public const string ClosedBracket = "ClosedBracket"; // )
    public const string LambdaExpression = "LambdaExpression"; // =>
    public const string Concat = "Concat"; // @
    
    //TokenType.ElementalFunctions
    public const string sqrt = "sqrt"; // sqrt
    public const string sin = "sin"; // sin
    public const string cos = "cos"; //cos
    public const string exp = "exp"; // euler pow to a value
    public const string log = "log"; // log
    public const string rand = "rand"; // random number between [0, 1]
    public const string print = "print"; // print

    //TokenType.Constant
    public const string PI = "PI"; // pi
    public const string E = "E"; //euler value

    //TokenType.BooleanOperator
    public const string And = "And"; // &
    public const string Or = "Or"; // |
    public const string Not = "Not"; // !
    public const string Equal = "Equal"; // ==
    public const string Different = "Different"; // !=
    public const string Less = "Less"; // <
    public const string LessOrEqual = "LessOrEqual"; // <=
    public const string More = "More"; // >
    public const string MoreOrEqual = "MoreOrEqual"; // >=
    
    //TokenType.BooleanExpression
    public const string True = "True"; // True
    public const string False = "False"; // False

    //TokenType.Conditional
    public const string If = "if"; // if
    public const string Else = "else"; //else

    //TokenType.KeyWords
    public const string let = "let"; // let
    public const string In = "in"; // in
    public const string function = "function"; //function
}