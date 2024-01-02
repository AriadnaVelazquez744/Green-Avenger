using System.Diagnostics.CodeAnalysis;

public class LexicalAnalyzer
{
    //Los valores de estos diccionarios se determinan en la clase Lexer y son el vocabulario de mi lenguaje
    public Dictionary<string, string> Operators { get; set; }
    public Dictionary<string, string> KeyWords { get; set; }
    public Dictionary<string, string> Texts { get; set; }

    public LexicalAnalyzer()
    {
        Operators = new();
        KeyWords = new();
        Texts = new();
    }

    public IEnumerable<Token> GetTokens(string code, List<CompilingError> errors)
    {
        //La función de este método es crear una interface recorrible con los token que conforman el código a analizar (input)
        List<Token> tokens = new();    //Listado de token en que se divide el input y luego serán analizados
        TokenReader stream = new(code);    //llamado a la clase stream para utilizar sus métodos en el recorrido del input

        while (!stream.EOF)
        {
            string value;   //valor que va a ir siendo modificado para declarar los string de los token
            if (stream.ReadWhiteSpace())    //Si es un espacio en blanco se salta el char
                continue;

            else if (stream.ReadId(out value))
            {
                //Se lee el string que cumple con los requisitos para ser identificador.
                //Se comprueba si es una palabra clave y se añade como tal o se añade como identificador en la lista de tokens
                //En caso de ser una palabra clave se realiza la diferenciación en que tipo de palabra clave es
                if (char.IsDigit(value[0]))
                {
                    errors.Add(new CompilingError(stream.Location, ErrorCode.Invalid, "The identifiers and keywords can't start with digits"));
                    continue;
                }

                if (KeyWords.ContainsKey(value))
                {
                    if (value == "true" || value == "false")
                    {
                        tokens.Add(new Token(TokenType.BooleanExpression, KeyWords[value], stream.Location));
                    }
                    else if (value == "if" || value == "else")
                    {
                        tokens.Add(new Token(TokenType.Conditional, KeyWords[value], stream.Location));
                    }
                    else if (value == "sqrt" || value == "sin" || value == "cos" || value == "exp" || value == "log" || value == "rand" || value == "PI" || value == "E")
                    {
                        tokens.Add(new Token(TokenType.ElementalFunctions, KeyWords[value], stream.Location));
                    }
                    else
                        tokens.Add(new Token(TokenType.KeyWord, KeyWords[value], stream.Location));
                }

                else
                    tokens.Add(new Token(TokenType.Identifier, value, stream.Location));

                continue;
            }

            else if (MatchSymbol(stream, tokens))
                continue;

            else if (stream.ReadNumber(out value))
            {
                //Si primer char es un número se entra en el cuerpo de la condicional y se comprueba si realmente es un numero al intentar 
                //parsearlo, de no ser posible se determina como un error, de lo contrario se añade como token numérico.
                if (!double.TryParse(value, out double d))
                    errors.Add(new CompilingError(stream.Location, ErrorCode.Invalid, String.Format("The element \"{0}\" is wrong declare", value)));
                else
                {
                    if (tokens.Count != 0)
                    {
                        if (tokens.ElementAt(tokens.Count - 1).Value.ToString() == "-")
                        {
                            if (tokens.Count == 1 || tokens.ElementAt(tokens.Count - 2).Type is TokenType.Operator || tokens.ElementAt(tokens.Count - 2).Type is TokenType.Symbol)
                            {
                                string newValue = "-" + value;
                                value = newValue;
                            }
                        }
                    }
                    tokens.Add(new Token(TokenType.Number, value, stream.Location));
                }

                continue;
            }

            else if (MatchText(stream, tokens, errors))
                continue;

            // else if (MatchSymbol(stream, tokens))
            //     continue;

            //si el char que se está leyendo con coincide con ninguna de las posibilidades es algo completamente desconocido de lo que ni siquiera se me ocurre un ejemplo y se declara como tal
            else 
            {
                var unkOp = stream.ReadAny();
                errors.Add(new CompilingError(stream.Location, ErrorCode.Unknown, unkOp.ToString()));
            }
        }

        return tokens;
    }

    private bool MatchSymbol(TokenReader stream, List<Token> tokens)
    {
        //Se recorre el diccionario de operadores y si el valor obtenido coincide con alguno de ellos se añade a la lista de tokens
        foreach (var op in Operators.Keys.OrderByDescending(k => k.Length))
        {
            if (stream.Match(op))
            {
                if ((op == "-"))
                {
                    if (tokens.Count == 0)
                    {
                        return false;
                    }
                    else if (tokens.ElementAt(tokens.Count - 1).Type is TokenType.Operator || tokens.ElementAt(tokens.Count - 1).Type is TokenType.Symbol)
                    {
                        return false;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Operator, Operators[op], stream.Location));
                        return true;
                    }
                }

                if (op == "=" || op == "," || op == ";" || op == "(" || op == ")" || op == "=>")
                {
                    tokens.Add(new Token(TokenType.Symbol, Operators[op], stream.Location));
                    return true;
                }
                else
                {
                    tokens.Add(new Token(TokenType.Operator, Operators[op], stream.Location));
                    return true;
                }
            }
        }
        return false;
    }

    private bool MatchText(TokenReader stream, List<Token> tokens, List<CompilingError> errors)
    {
        //Se busca una coincidencia con el valor guardado en el dict Text que es el símbolo \", si se encuentra se comienza a leer el 
        //resto de input y si se encuentra el símbolo nuevamente (lo cual se consideraría el cierre del string) se gurda como tal en el 
        //dict de tokens, de retornar false el ReadUntil se añade un error a la lista porque las comillas no están balanceadas.
        foreach (var start in Texts.Keys.OrderByDescending(k => k.Length))
        {
            string text;
            if (stream.Match(start))
            {
                if (!stream.ReadUntil(Texts[start], out text))
                    errors.Add(new CompilingError(stream.Location, ErrorCode.Expected, Texts[start]));

                tokens.Add(new Token(TokenType.Text, text, stream.Location));
                return true;
            }
        }
        return false;
    }
}

class TokenReader
{
    //Su función es ir leyendo el código para determinar los tokens
    readonly string code;   //Es el código a analizar y no es modificable pues es necesario que no haya posibilidad de cambiarlo para no perder información
    int pos;    //Hace referencia a la posición del char que se analiza
    int line;   //Se refiere a la línea de código que se está recorriendo (en este caso siempre es la primera pero se incluye para facilitar futuras mejoras)
    int lastLB;    //Ayuda a determinar la columna en la que se encuentra el object pues indica la posición anterior al inicio de la linea en la que se encuentra
    public TokenReader(string code)
    {
        this.code = code;
        this.pos = 0;
        this.line = 1;
        this.lastLB = -1;
    }

    //Estos dos métodos determinan si se alcanzó el final del código o el final de la línea que se está analizando en ese momento
    public bool EOF => (pos >= code.Length);
    public bool EOL => (EOF || code[pos] == '\n');

    public CodeLocation Location
    {
        //Asigna los valores de correspondientes de posición a cada token
        get
        {
            return new CodeLocation
            {
                Line = line,
                Column = pos - lastLB
            };
        }
    }

    public char Peek()
    {
        //Toma de ser posible el char indicado por la variable global pos dentro del input
        if (pos < 0 || pos >= code.Length)
            throw new InvalidOperationException();

        return code[pos];
    }

    public bool Match(string prefix)
    { 
        //Determina si el arg coincide con el fragmento de input que se está leyendo, se ser asi modifica el valor de pos según tamaño de arg.
        if (ContinuesWith(prefix))
        {
            pos += prefix.Length;
            return true;
        }
        return false;
    }

    public bool ReadWhiteSpace()
    {
        //Determina si el char es un espacio en blanco.
        if (char.IsWhiteSpace(Peek()))
        {
            ReadAny();
            return true;
        }
        return false;
    }

    public bool ReadId(out string id)
    {
        //Determina si lo que que se lee es un posible identificador al ir tomando caracteres mientras sea posible y confirmar que 
        //el resultado no es una cadena vacía.
        id = "";
        while (!EOL && ValidIdCharacter(Peek(), id.Length == 0))
            id += ReadAny();
        return id.Length > 0;
    }

    public bool ReadNumber(out string number)
    {
        //Como todos los valores devueltos son strings, se inicializa la variable como nula y se empieza a revisar los siguientes 
        //caracteres, si estos son números o es un punto se van añadiendo, si después de esto no se ha añadido nada number  se devuelve false,
        //si no ha aparecido un char que no sea un numero (después del primer punto no se puede añadir otro punto) se comienzan a añadir 
        //letras o numero que aparezcan ininterrumpidamente, si no aparece nada es que no está relacionado con el contexto de llamada y retorna false.
        number = "";

        if (pos > 0) 
        {
            pos -= 1;
            if (!EOL && Match("-"))
            {
                if (pos == 1)
                {
                    number += "-";
                }
                else
                {
                    pos -= 2;
                    if (Match("+") || Match("%") || Match("^") || Match("/") || Match("*") || Match("(") || Match("=") || Match(">") || Match("-"))
                    {
                        pos += 1;
                        number += "-";
                    }
                    else
                        pos += 2;
                }
            }
            else
                pos += 1;
        }
            
        while (!EOL && char.IsDigit(Peek()))
            number += ReadAny();
        if (!EOL && Match("."))
        {
            number += '.';
            while (!EOL && char.IsDigit(Peek()))
                number += ReadAny();
        }

        // if (number.Length == 1 && number == "-")
        //     return false;
        if (number.Length == 0)
            return false;

        while (!EOL && char.IsLetterOrDigit(Peek()))
            number += ReadAny();

        return number.Length > 0;
    }

    public bool ReadUntil(string end, out string text)
    {
        //Se inicializa con comillas que corresponden a la formación de strings y mientras no aparezca una coincidencia con el arg 
        //end se siguen añadiendo letras a la variable text cuyo valor hay que modificar, sin embargo si se llega al final del input 
        //antes de encontrar la coincidencia se retorna false. 
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
        //Devuelve un char si no se ha excedido los límites de la línea y salta de linea en caso de haya llegado al final de esta 
        //pero no del input, devolviendo el char de la pos siguiente y no en la que se encuentra a diferencia de Peek()
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
        //Determina si la secuencia de char del string coinciden con una palabra en específico, primero asegurándose que desde donde 
        //se encuentra el primer char al sumarle el tamaño del arg no se sobrepase la capacidad del input, y luego al comprobar la 
        //coincidencia de cada char con el del arg
        if (pos + prefix.Length > code.Length)
            return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (code[pos + i] != prefix[i])
                return false;
        }

        return true;
    }
    private bool ValidIdCharacter(char c, bool beginning)
    {
        //Determina si un char es válido para ser parte de un identificador teniendo en cuenta las restricciones de que no puede incluir un id
        return (c == '_') || (beginning ? char.IsLetter(c) : char.IsLetterOrDigit(c));
    }
}