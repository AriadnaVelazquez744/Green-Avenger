public class Token
{
    //Se declaran las propiedades que van a poseer los Token y son necesarias para su posterior uso
    //En este caso cada token tendrá un valor que es la parte del texto que se a separado, un tipo de 
    //acuerdo a cual podría ser su funcionalidad en la línea de código y la posición que ocupa dentro 
    //de la línea de entrada.
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

//Establece los parámetros que se tomaran en cuenta para determinar la posición del toquen, esta deberá 
//variar en dependencia de la forma en que se establezca la entrada del string a analizar.
public struct CodeLocation
{
    public string File;
    public int Line;
    public int Column;
}

public enum TokenType
{
    //Se declaran los tipos de token que utilizará el lenguaje y que corresponden con los tipos de operaciones 
    //que se pueden hacer de una manera muy general.
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
    //Aquí se especifican los valores que pueden tomar los token en mi vocabulario, para mi comodidad los separé en 
    //en el tipo de token al que pertenecen y les comenté cual es el string que les corresponde.
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
    public const string print = "print"; // print
}