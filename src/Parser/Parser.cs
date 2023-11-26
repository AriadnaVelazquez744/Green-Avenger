using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class Parser
{
    //La clase contiene el TokenStream para obtener los Tokens a analizar e inicializa las clases Context y Scope para su utilización.
    public TokenStream Stream { get; set; }
    public Context Context { get; set; }
    public Scope Scope { get; set; }
    public Parser(TokenStream stream)
    {
        Stream = stream;
        this.Context = new();
        this.Scope = new();
    }

    public MainProgram Parse(List<CompilingError> errors) 
    {
        //Se crea una lista de ASTNode que son los que se analizarán en la semántica, se inicia el enumerator en la primera posición se empieza a analizar cada token a partir de ahí
        List<ASTNode> nodes = new();

        //Se utiliza para declara la localización del primer token para poder ponerlo en el MainProgram que devuelve el método.
        int x = 1;
        CodeLocation loc = new();

        while (Stream.CanLookAhead())
        {
            Token currentToken = Stream.LookAhead(0);
            if (x == 1) loc = currentToken.Location;

            //Print es una especie de función ya definida aunque se trata como un tipo para poder guardar el desglose de la expresión que ha de imprimir
            if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    nodes.Add(impression!);
            }

            //El string function es una palabra reservada para la declaración de funciones, por lo que una vez encontrado es la única operación viable.
            else if (currentToken.Value.ToString() == "function")
            {
                FunctionDeclare element = ParseFunctionDeclaration(errors);
                if (element is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong declaration of the function {0}", Stream.LookAhead().Value.ToString())));
                else
                    nodes.Add(element!);
            }

            //Al igual que con function, let es una palabra reservada en este caso para declarar variables
            else if (currentToken.Value.ToString() == "let")
            {
                Variable var = ParseVariable(errors);
                if (var is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of the KeyWord \"let\" "));
                else
                    nodes.Add(var!);
            }

            //Continuando con las palabras reservadas if indica el inicio de una condicional
            else if (currentToken.Value.ToString() == "if")
            {
                Conditional decision = ParseConditional(errors);
                if (decision is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong conditional declaration"));
                else
                    nodes.Add(decision!);
            }

            
            //En caso de encontrar un identificador hay dos opciones:
            //Si este se encuentra seguido de un paréntesis o un numero o texto que serían argumentos, se refiere al llamado de una función y se parsea como tal, lo que incluye pasarle el token de 
            //identificación para tener el nombre de la función que llama.
            //El otro caso es cuando no llama una función y por lo tanto se refiere a una variable, si esta variable es llamada desde aquí quiere decir que no pertenece a ningún scope y por tanto 
            //no tiene valor por lo que es un error sintáctico, sin embargo se realiza el parseo de expresiones para poder descartar la expression que es invalida completamente.
            else if (currentToken.Type == TokenType.Identifier)
            {
                if (Stream.Next(TokenValues.OpenBracket) || Stream.Next(TokenType.Number) || Stream.Next(TokenType.Text) || Stream.Next(TokenType.Identifier))
                {
                    FunctionCall call = ParseFunctionInvocation(errors, currentToken);
                    if (call is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong invocation of the function {0}", currentToken.Value.ToString())));
                    else
                        nodes.Add(call!);
                }
                else
                {
                    Expression exp = ParseExpression(errors)!;
                    if (exp is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                    else
                        nodes.Add(exp!);
                }
            }

            else if (currentToken.Value.ToString() == ";")
            {
                Stream.MoveNext(1);
                continue;
            }
            
            //Si no es ninguno de los anteriores ha de ser un string, un numero o un literal booleano y se analiza como expression.
            else
            {
                Expression exp = ParseExpression(errors)!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    nodes.Add(exp!);
            }

            Stream.MoveNext(1);
        }  

        //El ultimo token leído ha de ser un ; para que esté completamente bien declarado el código.
        if (Stream.LookAhead(-1).Value != ";")
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "; expected")); 

        return new(nodes, Context, loc);
    }

    private List<ASTNode> ParseBody(List<CompilingError> errors)
    {
        List<ASTNode> body = new();

        //Este procedimiento es muy similar al Parse inicial, la diferencia radica en que este ciclo se detiene al aparecer un paréntesis cerrado para detener el cuerpo de la función declarada o 
        //el ; para delimitar el cuerpo de la variable (la vecindad del código en que las variables son válidas).
        //También al no ser posible declarar funciones dentro de ninguna de las otras estructuras si se encuentra la palabra clave function se parsea normalmente la declaración de la función pero 
        //no se incluye en la lista de ASTNode.
        //Además, en el caso de la utilización de variables aquí sí se guarda en la lista el valor obtenido con el parseo de expresión si es que la variable existe en este scope.

        while(Stream.LookAhead().Value != ")" && Stream.LookAhead().Value != ";")
        {
            Token currentToken = Stream.LookAhead();

            if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    body.Add(impression!);
            }

            else if (currentToken.Value.ToString() == "let")
            {
                Variable var = ParseVariable(errors);
                if (var is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of the KeyWord \"let\" "));
                else
                    body.Add(var!);
            }

            else if (currentToken.Value.ToString() == "if")
            {
                Conditional decision = ParseConditional(errors);
                if (decision is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong conditional declaration"));
                else
                    body.Add(decision!);
            }

            else if (currentToken.Type == TokenType.Identifier)
            {
                if (Stream.Next(TokenValues.OpenBracket) || Stream.Next(TokenType.Number) || Stream.Next(TokenType.Text))
                {
                    FunctionCall call = ParseFunctionInvocation(errors, currentToken);
                    if (call is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong invocation of the function {0}", currentToken.Value.ToString())));
                    else
                        body.Add(call!);
                }
                else
                {
                    Expression exp = ParseExpression(errors)!;
                    if (exp is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                    else
                        body.Add(exp!);
                }
            }

            else
            {
                Expression exp = ParseExpression(errors)!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    body.Add(exp!);
            }
            
            Stream.MoveNext(1);
        }   

        return body;
    }
    
    private Expression? ParseElementalFunction(List<CompilingError> errors)
    {
        //Se crea una nueva variable del tipo ElementalFunction y se comienzan a tomar los valores necesarios: se comprueba si el id es una contante ya que estas no llevan parámetros, de no 
        //pertenecer a ese diccionario se pasa a mediante un bucle tomar todos los valores pasados como argumentos y que tienen que estar separados por comas, luego dependiendo de la cantidad 
        //de argumentos que tiene es el tipo de función que puede ser y si no coincide devuelve un valor nulo; en caso de coincidir en alguno de los casos devuelve directamente un nodo tipo
        //numero con el valor de calculado y la posición dada por el token con el id.
        Token Id = Stream.LookAhead();
        if (Id.Type is not TokenType.ElementalFunctions)    return null!;
            
        string id = Id.Value.ToString();
        List<Expression> arguments = new();

        if (id == "PI" || id == "E" || id == "rand")
            return new ElementalFunction(id, arguments, Id.Location);

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        Stream.MoveNext(1);

        int x = 0;
        do
        {
            if ( x == 1 && Stream.Next(TokenValues.ClosedBracket))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected after the ,"));
            }
            else
            {
                if (x == 1)
                    Stream.MoveNext(1);

                Expression? exp = ParseExpression(errors);
                if (exp is null)
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected after the ,"));
                else
                    arguments.Add(exp);
            }
            x = 1;
        } while (Stream.Next(TokenValues.ValueSeparator));
        
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        return new ElementalFunction(id, arguments, Id.Location);
    }

    private Conditional ParseConditional(List<CompilingError> errors)
    {
        //Se lee token a token teniendo en cuenta la formula sintácticamente correcta de las condicionales.

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        Stream.MoveNext(1);

        //La condicionales se parsea como expression para en la evaluación poder obtener un literal booleano. 
        Expression condition = ParseExpression(errors)!;
        if (condition is null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "condition value expected"));
        }

        if (!Stream.Next(TokenValues.ClosedBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));

        if (!Stream.Next(TokenValues.LambdaExpression))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "=> expected"));

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));

        Stream.MoveNext(1);
        //Se inicializan los cuerpos de ambas partes, el if se analiza directamente pues siempre hay que intentar encontrarlo aunque sea vacío y luego en dependencia de si aparece la keyWord 
        //else se busca el cuerpo de esta. 
        List<ASTNode> ifBody = ParseBody(errors);
        List<ASTNode> elseBody = null!;

        if (Stream.Next(TokenValues.Else))
        {
            if (!Stream.Next(TokenValues.OpenBracket))
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "() expected"));
            Stream.MoveNext(1);

            elseBody = ParseBody(errors);
        }

        if (condition is null)
            return null!;

        return new Conditional(condition, ifBody, elseBody, Stream.LookAhead().Location);
    }

    private FunctionDeclare ParseFunctionDeclaration(List<CompilingError> errors)
    {
        //Se busca el nombre dado a la nueva función, se extrae como token primero para poder utilizar su ubicación y luego se le separa el string identificativo. 
        //Luego se comienza a leer de acuerdo a la sintaxis de la declaración de funciones del hulk
        if (!Stream.Next(TokenType.Identifier))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "function name expected"));
        }
        Token id = Stream.LookAhead();
        string functionName = id.Value.ToString();
        
        

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }

        //Se inicializa una lista que contendrá todos los argumentos y que luego se pasará al nodo creado y que será devuelto
        List<string> arguments = new();

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            do
            {
                if (!Stream.Next(TokenType.Identifier))
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected"));
                
                else
                    arguments.Add(Stream.LookAhead().Value.ToString());

            } while (Stream.Next(TokenValues.ValueSeparator));
            
        } 

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        if (!Stream.Next(TokenValues.LambdaExpression))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "=> expected"));
        }

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }

        Stream.MoveNext(1);

        //Se crea el cuerpo de la función como una lista de nodos para poder evaluarlos más adelante, si no existe cuerpo no hay función
        List<ASTNode> body = ParseBody(errors);
        if (body == null)
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "Function statement expected"));
            return null!;
        }

        //Si no se le declaró un identificador para declararla o ese id ya fue utilizado para nombrar otra función, entonces no se devuelve nada
        if (id is null)
            return null!;

        if (Context.ContainFunc(functionName))
        {
            errors.Add(new CompilingError(id.Location, ErrorCode.Invalid, String.Format("The function {0} already exist", functionName)));
            return null!;
        }
        
        Context.AddFunc(functionName, arguments.Count);

        return new FunctionDeclare(functionName, arguments, body, id.Location);
    }

    private FunctionCall ParseFunctionInvocation(List<CompilingError> errors, Token id)
    {
        //Se crea una lista de argumentos que deberán coincidir en cantidad con la función a la que invocan. Luego de determina si el numero de argumentos 
        //necesarios es distinto de 0 y si lo es en un bucle se van determinando cada una, en caso de que una vez terminado este procedimiento el count sea 
        //distinto de cero se devuelve nulo porque fue mal declarada la función.

        List<Expression> argumentValues = new();
        string functionName = id.Value.ToString();

        if (Stream.LookAhead().Value != "(")
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }
        Stream.MoveNext(1);

        while (true)
        {
            Expression argValue = ParseExpression(errors)!;
            if (argValue is not null) 
            {
                argumentValues.Add(argValue);
            }

            if (!Stream.Next(TokenValues.ValueSeparator))
                break; 

            Stream.MoveNext(1);   
        }

        
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        return new FunctionCall(functionName, argumentValues, id.Location);
    }

    private Variable ParseVariable(List<CompilingError> errors) 
    {
        //Se inicializa el primer token de la expression para poder obtener de él la posición que necesita el ASTNode devuelto
        //así como el diccionario de variables que se están creando en este scope
        
        CodeLocation loc = Stream.LookAhead().Location;
        
        Dictionary<string, Expression> variables = new();
        int x = 0;
        do
        {
            //El while va a seguir funcionando siempre que sea posible crear otra variable que pertenezca al mismo scope que en este caso sería si lo siguiente al valor es una coma
            //Se va revisando si el siguiente token cumple con la sintaxis de la declaración de variables y una vez se llega al final de la posible declaración y ninguno de los valores sea nulo
            //se añade la variable al diccionario que formara parte del tipo Variable y se incluirá en el scope para poder comprobar más adelante en caso de tener in si ha sido declarada y 
            //se puede utilizar para añadirla ahora la scope su valor no es relevante por lo que pasa como null para evitar incoherencias en cuanto a si es object o Expression
            string variableName = null!;

            if (x > 0)  Stream.MoveNext(1);

            if (Stream.LookAhead().Value.ToString() == "let")
                Stream.MoveNext(1);

            if (Stream.LookAhead().Type is not TokenType.Identifier)
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "identifier expected"));
            }
            else 
            {
                variableName = Stream.LookAhead().Value.ToString();
            }

            if (!Stream.Next(TokenValues.Assign))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "= expected"));
            }
            Stream.MoveNext(1);
            
            Expression value = ParseExpression(errors)!;
            if (value == null)
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "expression value expected"));
                continue;
            }

            if (variableName is not null)
            {
                if (variables.ContainsKey(variableName))
                {
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Invalid, String.Format("the variable {0} already exist", variableName)));
                }
                else
                {
                    variables.Add(variableName, value);
                    Scope.AddVariable(variableName, null!);
                }
            }

            x++;

        } while (Stream.Next(TokenValues.ValueSeparator));
        

        //Si aparece el token in quiere decir que lo que se encuentre a continuación pertenece al área en la que se utilizan las variables declaradas y se parsea lo que he denominado cuerpo de la 
        //variable que también es un lista de ASTNode.
        
        List<ASTNode> body = new();
        if (Stream.Next(TokenValues.In))
        {
            Stream.MoveNext(1);
            body = ParseBody(errors);
        }

        //Si no se declaró ninguna variable después del let, no importa q haya dentro del in no cumplirá ninguna función
        if (variables is null)
        {
            return null!;
        }

        //Antes de devolver el ASTNode se eliminan todas las variables declaradas en este Scope para que no se vaya fuera de rango su utilización
        Scope.DeleteVariables(variables);

        return new(variables, body, loc);
    }

    private Print ParsePrint(List<CompilingError> errors)
    {
        //Se realiza la lectura de la sintaxis de la función predeterminada print, no tiene nada particular, unicamente se introduce en el tipo Print la expression y la posición de la expression.
        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }
        Stream.MoveNext(1);

        Expression exp = ParseExpression(errors)!;

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        return new Print(exp, exp.Location);
    }


    private Expression? ParseNumber()
    {
        //Comprueba si el token en la posición actual es un numero y de ser así devuelve ese valor
        if (Stream.LookAhead().Type is not TokenType.Number) return null;
    
        return new Number(double.Parse(Stream.LookAhead().Value.ToString()), Stream.LookAhead().Location);
    }
    private Expression? ParseText()
    {
        //Comprueba si el token en la posición actual es un texto y de ser así devuelve ese valor
        if (Stream.LookAhead().Type is not TokenType.Text) return null;

        return new Text(Stream.LookAhead().Value, Stream.LookAhead().Location);
    }
    private Expression? ParseLiteral()
    {
        if (Stream.LookAhead().Type is not TokenType.BooleanExpression) return null;

        return new BoolLiteral(bool.Parse(Stream.LookAhead().Value.ToString()), Stream.LookAhead().Location);
    }
    private Expression? ParseVar()
    {
        if (Stream.LookAhead().Type is not TokenType.Identifier) return null;

        return new Var(Stream.LookAhead().Value.ToString(), Stream.LookAhead().Location);
    }
    private Expression? ParseParenthesis(List<CompilingError> errors)
    {
        if (Stream.LookAhead().Value.ToString() == "(")
        {
            Stream.MoveNext(1);
            Expression? exp = ParseExpression(errors);
            
            if (exp == null)
                return null;
            if (!Stream.Next(TokenValues.ClosedBracket))
                return null;
            
            return exp;
        }
        return null;
    }
    private Expression ParseNot(List<CompilingError> errors)
    {
        if (Stream.LookAhead().Value.ToString() != "!") return null!;
        CodeLocation loc = Stream.LookAhead().Location;
        Stream.MoveNext(1);
        
        //Se parsea una expresión que se le pasa al nodo not como valor.
        Expression exp = ParseExpression(errors)!;
        if (exp is null)
        {
            errors.Add(new CompilingError(loc, ErrorCode.Expected, "Is necessary an expression after the symbol \"!\""));
            return null!;
        }
        return new Not(exp, loc);
    }
   






    private Expression? ParseExpressionLv1_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParseConcat(left, errors);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv2_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParsePropositional(left, errors);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv3_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParseEquals(left, errors);
        if (exp != null) return exp;

        exp = ParseCompare(left, errors);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv4_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParseAdd(left, errors);
        if (exp != null) return exp;

        exp = ParseSub(left, errors);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv5_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParseMul(left, errors);
        if (exp != null) return exp;

        exp = ParseDiv(left, errors);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv6_(Expression? left, List<CompilingError> errors)
    {
        Expression? exp = ParsePow(left, errors);
        if (exp != null) return exp;

        return left;
    }

    private Expression? ParseExpression(List<CompilingError> errors)
    {
        return ParseExpressionLv1(errors);
    }
    private Expression? ParseExpressionLv1(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv2(errors);
        return ParseExpressionLv1_(newLeft, errors);
    }
    private Expression? ParseExpressionLv2(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv3(errors);
        return ParseExpressionLv2_(newLeft, errors);
    }
    private Expression? ParseExpressionLv3(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv4(errors);
        return ParseExpressionLv3_(newLeft, errors);
    }
    private Expression? ParseExpressionLv4(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv5(errors);
        return ParseExpressionLv4_(newLeft, errors);
    }
    private Expression? ParseExpressionLv5(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv6(errors);
        return ParseExpressionLv5_(newLeft, errors);
    }
    private Expression? ParseExpressionLv6(List<CompilingError> errors)
    {
        Expression? newLeft = ParseExpressionLv7(errors);
        return ParseExpressionLv6_(newLeft, errors);
    }
    
    private Expression? ParseExpressionLv7(List<CompilingError> errors)
    {
        Expression? exp = ParseParenthesis(errors);
        if (exp != null) return exp;

        exp = ParseNumber();
        if (exp != null) return exp;

        exp = ParseText();
        if (exp != null) return exp;

        exp = ParseLiteral();
        if (exp != null) return exp;

        exp = ParseNot(errors);
        if (exp != null) return exp;

        exp = ParseElementalFunction(errors);
        if (exp != null) return exp;

        exp = ParseVar();
        if (exp != null) return exp;

        return null;
    }

    private Expression? ParseAdd(Expression? left, List<CompilingError> errors)
    {
        Add sum = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Add)) return null;

        sum.Left = left;
        
        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv4(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sum.Right = right;

        return ParseExpressionLv4_(sum, errors);
    }
    private Expression? ParseSub(Expression? left, List<CompilingError> errors)
    {
        Sub sub = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Sub)) return null;

        sub.Left = left;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv4(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sub.Right = right;

        return ParseExpressionLv4_(sub, errors);
    }
    private Expression? ParseMul(Expression? left, List<CompilingError> errors)
    {
        Mul mul = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Mul)) return null;

        mul.Left = left;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv5(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        mul.Right = right;

        return ParseExpressionLv5_(mul, errors);
    }
    private Expression? ParseDiv(Expression? left, List<CompilingError> errors)
    {
        Div div = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Div)) return null;

        div.Left = left;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv5(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;   
        }
        div.Right = right;

        return ParseExpressionLv5_(div, errors);
    }
    private Expression? ParsePow(Expression? left, List<CompilingError> errors)
    {
        Pow pow = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Pow)) return null;

        pow.Left = left;
        
        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv6(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        pow.Right = right;

        return ParseExpressionLv6_(pow, errors);
    }
    
    private Expression? ParseConcat(Expression? left, List<CompilingError> errors)
    {
        Concat concat = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Concat))   return null;
        Stream.MoveNext(1);
        concat.Left = left;

        Expression? right = ParseExpressionLv1(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        concat.Right = right;

        return ParseExpressionLv1_(concat, errors);
    }

    private Expression? ParsePropositional(Expression? left, List<CompilingError> errors)
    {
        PropOp prop = new(Stream.LookAhead().Location);

        if (left is null || (!Stream.Next(TokenValues.And) && !Stream.Next(TokenValues.Or))) return null;

        prop.Left = left;
        prop.Op = Stream.LookAhead().Value;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv2(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        prop.Right = right;

        return ParseExpressionLv2_(prop, errors);
    }
    private Expression? ParseEquals(Expression? left, List<CompilingError> errors)
    {
        Equality equal = new(Stream.LookAhead().Location);

        if (left is null || (!Stream.Next(TokenValues.Equal) && !Stream.Next(TokenValues.Different)))  return null;

        equal.Left = left;
        equal.Op = Stream.LookAhead().Value;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv3(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        equal.Right = right;

        return ParseExpressionLv3_(equal, errors);
    }
    private Expression? ParseCompare(Expression? left, List<CompilingError> errors)
    {
        BooleanOp boolOp = new(Stream.LookAhead().Location);

        if (left is null || (!Stream.Next(TokenValues.Less) && !Stream.Next(TokenValues.LessOrEqual) && !Stream.Next(TokenValues.More) && !Stream.Next(TokenValues.MoreOrEqual)))    return null;

        boolOp.Left = left;
        boolOp.Op = Stream.LookAhead().Value;

        Stream.MoveNext(1);

        Expression? right = ParseExpressionLv3(errors);
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        boolOp.Right = right;

        return ParseExpressionLv3_(boolOp, errors);
    }   
}