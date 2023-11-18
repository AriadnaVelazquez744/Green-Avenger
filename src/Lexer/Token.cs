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
    Operator,
    BooleanExpression,
    Conditional,
    ElementalFunctions,
}

public class TokenValues
{
    //Aquí se especifican los valores que pueden tomar los token en mi vocabulario, para mi comodidad los separé en 
    //en el tipo de token al que pertenecen y les comenté cual es el string que les corresponde.
    protected TokenValues() { }

    //TokenType.Operator
    public const string Add = "+"; // +
    public const string Sub = "-"; // -
    public const string Mul = "*"; // *
    public const string Div = "/"; // /
    public const string Pow = "^"; // ^
    public const string And = "&"; // &
    public const string Or = "|"; // |
    public const string Not = "!"; // !
    public const string Equal = "=="; // ==
    public const string Different = "!="; // !=
    public const string Less = "<"; // <
    public const string LessOrEqual = "<="; // <=
    public const string More = ">"; // >
    public const string MoreOrEqual = ">="; // >=
    public const string Concat = "@"; // @

    //TokenType.Symbol
    public const string Assign = "="; // =
    public const string Reassign = ":="; // :=
    public const string ValueSeparator = ","; // ,
    public const string StatementSeparator = ";"; // ;
    public const string OpenBracket = "("; // (
    public const string ClosedBracket = ")"; // )
    public const string LambdaExpression = "=>"; // =>
    
    //TokenType.ElementalFunctions
    public const string sqrt = "sqrt"; // sqrt
    public const string sin = "sin"; // sin
    public const string cos = "cos"; //cos
    public const string exp = "exp"; // Euler pow to a value
    public const string log = "log"; // log
    public const string rand = "rand"; // random number between [0, 1]
    public const string PI = "PI"; // pi
    public const string E = "E"; //Euler value
        
    //TokenType.BooleanExpression
    public const string True = "true"; // True
    public const string False = "false"; // False

    //TokenType.Conditional
    public const string If = "if"; // if
    public const string Else = "else"; //else

    //TokenType.KeyWords
    public const string let = "let"; // let
    public const string In = "in"; // in
    public const string function = "function"; //function
    public const string print = "print"; // print
}