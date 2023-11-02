public class Parser
{
    //La clase contiene el TokenStream para obtener los Tokens a parsear e inicializa las clases Context y Scope para su utilización.
    public TokenStream Stream { get; set; }
    public Context Context { get; set; }
    public Scope Scope { get; set; }
    public Parser(TokenStream stream)
    {
        Stream = stream;
        this.Context = new();
        this.Scope = new();
    }

    public List<ASTNode> Parse(List<CompilingError> errors)
    {
        //Se crea una lista de ASTNode que son los que se analizarán en la semántica, se inicia el enumerator en la primera posición se empieza a analizar cada token a partir de ahí
        List<ASTNode> nodes = new();

        while (Stream.CanLookAhead(0))
        {
            Token currentToken = Stream.LookAhead();

            //Si el token actual coincide con el nombre de alguna de alguna de las funciones elementales definidas se parsea como tal, en caso de no devolver nada es un error de invalidez en el uso 
            if (Context.ContainsElemFunc(currentToken.Value.ToString()))
            {
                ASTNode value = ParseElementalFunction(errors, currentToken);
                if (value is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The function {0} is wrong use", currentToken.Value)));
                else
                    nodes.Add(value!);
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
                if (Stream.Next(TokenValues.OpenBracket) || Stream.Next(TokenType.Number) || Stream.Next(TokenType.Text))
                {
                    FunctionCall call = ParseFunctionInvocation(errors, currentToken);
                    if (call is null)
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("Wrong invocation of the function {0}", currentToken.Value.ToString())));
                    else
                        nodes.Add(call!);
                }
                else
                {
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The variable {0} is use but never declared", currentToken.Value.ToString())));        
                    
                    Expression variableUse = ParseExpression()!;
                    nodes.Add(variableUse);
                }
            }

            //Print es una especie de función ya definida aunque se trata como un tipo para poder guardar el desglose de la expresión que ha de imprimir
            else if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    nodes.Add(impression!);
            }

            // else if (currentToken.Type == TokenType.Text)
            // {
            //     Expression exp = ParseExpression()!;
            //     nodes.Add(exp);
            //     // Expression literal;
            //     // if (Stream.Next(TokenValues.Concat))
            //     // {
            //     //     literal = ParseConcat(errors, currentToken)!;
            //     //     if (literal is null)
            //     //         errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong concat declaration"));
            //     //     else
            //     //         nodes.Add(literal!);
            //     // }
            //     // else 
            //     // {
            //     //     literal = ParseBooleanExpression()!;
            //     //     if (literal is null)
            //     //         errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong implementation of a text"));
            //     //     else
            //     //         nodes.Add(literal!);
            //     // }
            // }

            //Es un operador booleano por lo que este es el análisis que se le realiza.
            else if (currentToken.Value.ToString() == "!")
            {
                BooleanExpression boolean = ParseBooleanExpression()!;
                if (boolean is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of \"!\""));
                else
                    nodes.Add(boolean!);
            }

            else if (currentToken.Value.ToString() == ";")
                continue;

            
            //Si no es ninguno de los anteriores ha de ser un string, un numero o un literal booleano y se analiza como expression.
            else
            {
                Expression exp = ParseExpression()!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    nodes.Add(exp!);
            }
        }  

        //El ultimo token leído ha de ser un ; para que esté completamente bien declarado el código.
        Stream.MoveBack(1);
        if (!Stream.Next(TokenValues.StatementSeparator))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "; expected")); 

        return nodes;
    }

    private List<ASTNode> ParseBody(List<CompilingError> errors)
    {
        List<ASTNode> body = new();

        //Este procedimiento es muy similar al Parse inicial, la diferencia radica en que este ciclo se detiene al aparecer un paréntesis cerrado para detener el cuerpo de la función declarada o 
        //el ; para delimitar el cuerpo de la variable (la vecindad del código en que las variables son válidas).
        //También al no ser posible declarar funciones dentro de ninguna de las otras estructuras si se encuentra la palabra clave function se parsea normalmente la declaración de la función pero 
        //no se incluye en la lista de ASTNode.
        //Además, en el caso de la utilización de variables aquí sí se guarda en la lista el valor obtenido con el parseo de expresión si es que la variable existe en este scope.

        while(!Stream.Next(TokenValues.ClosedBracket) || !Stream.Next(TokenValues.StatementSeparator))
        {
            Token currentToken = Stream.LookAhead(1);

            if (Context.ContainsElemFunc(currentToken.Value.ToString()))
            {
                ASTNode value = ParseElementalFunction(errors, currentToken);
                if (value is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The function {0} is wrong use", currentToken.Value)));
                else
                    body.Add(value!);
            }

            else if (currentToken.Value.ToString() == "function")
            {
                FunctionDeclare element = ParseFunctionDeclaration(errors);
                
                errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Is not possible declare a new function here"));
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
                    if (!Scope.ContainsVariable(currentToken.Value.ToString()))
                        errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, String.Format("The variable {0} is use but never declared", currentToken.Value.ToString())));        
                    
                    Expression variableUse = ParseExpression()!;
                    body.Add(variableUse);
                }
                
            }

            else if (currentToken.Value.ToString() == "print")
            {
                Print impression = ParsePrint(errors);
                if (impression is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of print"));
                else
                    body.Add(impression!);
            }

            // else if (currentToken.Type == TokenType.Text)
            // {
            //     Expression literal;
            //     if (Stream.Next(TokenValues.Concat))
            //     {
            //         literal = ParseConcat(errors, currentToken)!;
            //         if (literal is null)
            //             errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong concat declaration"));
            //         else
            //             body.Add(literal!);
            //     }
            //     else 
            //     {
            //         literal = ParseBooleanExpression()!;
            //         if (literal is null)
            //             errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong implementation of a text"));
            //         else
            //             body.Add(literal!);
            //     }
            // }

            else if (currentToken.Value.ToString() == "!")
            {
                BooleanExpression boolean = ParseBooleanExpression()!;
                if (boolean is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Invalid, "Wrong use of \"!\""));
                else
                    body.Add(boolean!);
            }

            else
            {
                Expression exp = ParseExpression()!;
                if (exp is null)
                    errors.Add(new CompilingError(currentToken.Location, ErrorCode.Unknown, String.Format("The element {0} doesn't belong to the syntax", currentToken.Value.ToString())));
                else
                    body.Add(exp!);
            }
        }   

        return body;

    }
    
    private Number ParseElementalFunction(List<CompilingError> errors, Token Id)
    {
        //Se crea una nueva variable del tipo ElementalFunction y se comienzan a tomar los valores necesarios: se comprueba si el id es una contante ya que estas no llevan parámetros, de no 
        //pertenecer a ese diccionario se pasa a mediante un bucle tomar todos los valores pasados como argumentos y que tienen que estar separados por comas, luego dependiendo de la cantidad 
        //de argumentos que tiene es el tipo de función que puede ser y si no coincide devuelve un valor nulo; en caso de coincidir en alguno de los casos devuelve directamente un nodo tipo
        //numero con el valor de calculado y la posición dada por el token con el id.

        ElementalFunction func = new();

        string id = Id.Value.ToString();

        if (func.Constants.ContainsKey(id))
            return new Number(func.Constants[id](), Id.Location);

        if (!Stream.CanLookAhead(0)) return null!;

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));

        List<double> arguments = new();
        do
        {
            if (!Stream.Next(TokenType.Number))
            {
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "argument expected after the ,"));
            }
            else
            {
                string argument = Stream.LookAhead().Value.ToString();
                double arg = double.Parse(argument);   
                arguments.Add(arg);
            }

        } while (Stream.Next(TokenValues.ValueSeparator));
        
        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        if (arguments.Count == 1 && func.MathFunction.ContainsKey(id))
        {
            return new Number(func.MathFunction[id](arguments[0]), Id.Location);
        }
        else if (arguments.Count == 2 && func.Log.ContainsKey(id))
        {
            return new Number(func.Log[id](arguments[0], arguments[1]), Stream.LookAhead().Location);
        }

        return null!;
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
        int count = Context.GetArgNumber(functionName);

        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }

        if (count != 0)
        {
            while (true)
            {
                Expression argValue = ParseExpression()!;
                if (argValue is not null) 
                {
                    count--;
                    argumentValues.Add(argValue);
                }

                if (!Stream.Next(TokenValues.ValueSeparator))
                    break;    
            }

            if (count != 0)
            {
                if (!Stream.Next(TokenValues.ClosedBracket))
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
                
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Invalid, "Wrong number of arguments"));
                return null!;
            }
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
        
        Token forLocation = Stream.LookAhead();
        Dictionary<string, Expression> variables = new();
        
        do
        {
            //El while va a seguir funcionando siempre que sea posible crear otra variable que pertenezca al mismo scope que en este caso sería si lo siguiente al valor es una coma
            //Se va revisando si el siguiente token cumple con la sintaxis de la declaración de variables y una vez se llega al final de la posible declaración y ninguno de los valores sea nulo
            //se añade la variable al diccionario que formara parte del tipo Variable y se incluirá en el scope para poder comprobar más adelante en caso de tener in si ha sido declarada y 
            //se puede utilizar para añadirla ahora la scope su valor no es relevante por lo que pasa como null para evitar incoherencias en cuanto a si es object o Expression
            string variableName = null!;
            if (Stream.LookAhead().Value.ToString() == "let")

            if (!Stream.Next(TokenType.Identifier))
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

            Expression value = ParseExpression()!;
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
                else if (Context.ContainFunc(variableName))
                {
                    errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Invalid, String.Format("already exist a function named {0}", variableName)));
                }
                else
                {
                    variables.Add(variableName, value);
                    Scope.AddVariable(variableName, null!);
                }
            }
        } while (Stream.Next(TokenValues.StatementSeparator));
        

        //Si aparece el token in quiere decir que lo que se encuentre a continuación pertenece al área en la que se utilizan las variables declaradas y se parsea lo que he denominado cuerpo de la 
        //variable que también es un lista de ASTNode.
        if (!Stream.Next(TokenValues.In))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.None, "The variables will not be use in anything"));
        }
        
        List<ASTNode> body = ParseBody(errors);

        //Si no se declaró ninguna variable después del let, no importa q haya dentro del in no cumplirá ninguna función
        if (variables is null)
        {
            return null!;
        }

        //Antes de devolver el ASTNode se eliminan todas las variables declaradas en este Scope para que no se vaya fuera de rango su utilización
        Scope.DeleteVariables(variables);

        return new(variables, body, Scope, forLocation.Location);
    }

    private Conditional ParseConditional(List<CompilingError> errors)
    {
        //Se lee token a token teniendo en cuenta la formula sintácticamente correcta de las condicionales.

        if (!Stream.Next(TokenValues.OpenBracket))
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));


        //La condicionales se parsea como expression para en la evaluación poder obtener un literal booleano. 
        BooleanExpression condition = ParseBooleanExpression()!;
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


        //Se inicializan los cuerpos de ambas partes, el if se analiza directamente pues siempre hay que intentar encontrarlo aunque sea vacío y luego en dependencia de si aparece la keyWord 
        //else se busca el cuerpo de esta. 
        List<ASTNode> ifBody = ParseBody(errors);
        List<ASTNode> elseBody = null!;

        if (Stream.Next(TokenValues.Else))
        {
            if (!Stream.Next(TokenValues.OpenBracket))
                errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "() expected"));

            elseBody = ParseBody(errors);
        }

        if (condition is null)
            return null!;

        return new Conditional(condition, ifBody, elseBody, Stream.LookAhead().Location);
    }

    private Print ParsePrint(List<CompilingError> errors)
    {
        //Se realiza la lectura de la sintaxis de la función predeterminada print, no tiene nada particular, unicamente se introduce en el tipo Print la expression y la posición de la expression.
        if (!Stream.Next(TokenValues.OpenBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "( expected"));
        }

        Expression exp = ParseExpression()!;

        if (!Stream.Next(TokenValues.ClosedBracket))
        {
            errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, ") expected"));
        }

        return new Print(exp, exp.Location);
    }

    // private Expression? ParseConcat(List<CompilingError> errors, Token text)
    // {
    //     string left = text.Value.ToString();

    //     Expression? Right = ParseText()!;
    //     if (Right == null)
    //     {
    //         errors.Add(new CompilingError(Stream.LookAhead().Location, ErrorCode.Expected, "Expected text for the concat operation"));
    //         return null;
    //     }
    //     string right = Right.Value!.ToString()!;

    //     return new Text(left + right, text.Location);
    // }

    private Expression? ParseNumber()
    {
        if (!Stream.Next(TokenType.Number)) return null;
    
        return new Number(double.Parse(Stream.LookAhead().Value.ToString()), Stream.LookAhead().Location);
    }
    private Expression? ParseText()
    {
        if (!Stream.Next(TokenType.Text)) return null;

        return new Text(Stream.LookAhead().Value, Stream.LookAhead().Location);
    }
    
    private Expression? ParseExpression()
    {
        return ParseExpressionLv1();
    }
    private Expression? ParseExpressionLv0()
    {
        if (Stream.Next(TokenValues.OpenBracket))
        {
            Expression? exp = ParseExpression();
            
            if (exp == null)
                return null;
            if (!Stream.Next(TokenValues.ClosedBracket))
                return null;
            
            return exp;
        }
        return ParseExpressionLv4();
    }
    private Expression? ParseExpressionLv1()
    {
        Expression? newLeft = ParseExpressionLv0();
        Expression? exp = ParseExpressionLv1_(newLeft);
        return exp;
    }
    private Expression? ParseExpressionLv1_(Expression? left)
    {
        Expression? exp = ParseAdd(left);
        if (exp != null) return exp;

        exp = ParseSub(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv2()
    {
        Expression? newLeft = ParseExpressionLv1();
        return ParseExpressionLv2_(newLeft);
    }
    private Expression? ParseExpressionLv2_(Expression? left)
    {
        Expression? exp = ParseMul(left);
        if (exp != null) return exp;

        exp = ParseDiv(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv3()
    {
        Expression? newLeft = ParseExpressionLv2();
        return ParseExpressionLv3_(newLeft);
    }
    private Expression? ParseExpressionLv3_(Expression? left)
    {
        Expression? exp = ParsePow(left);
        if (exp != null) return exp;

        return left;
    }
    private Expression? ParseExpressionLv4()
    {
        Expression? exp = ParseNumber();
        if (exp != null) return exp;

        exp = ParseText();
        if (exp != null) return exp;

        exp = ParseBooleanExpression();
        if (exp != null) return exp;

        return null;
    }

    private Expression? ParseAdd(Expression? left)
    {
        Add sum = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Add)) return null;

        sum.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sum.Right = right;

        return ParseExpressionLv1_(sum);
    }
    private Expression? ParseSub(Expression? left)
    {
        Sub sub = new(Stream.LookAhead().Location);

        if (left == null || !Stream.Next(TokenValues.Sub)) return null;

        sub.Left = left;

        Expression? right = ParseExpressionLv1();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        sub.Right = right;

        return ParseExpressionLv1_(sub);
    }
    private Expression? ParseMul(Expression? left)
    {
        Mul mul = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Mul)) return null;

        mul.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        mul.Right = right;

        return ParseExpressionLv2_(mul);
    }
    private Expression? ParseDiv(Expression? left)
    {
        Div div = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Div)) return null;

        div.Left = left;

        Expression? right = ParseExpressionLv2();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;   
        }
        div.Right = right;

        return ParseExpressionLv2_(div);
    }
    private Expression? ParsePow(Expression? left)
    {
        Pow pow = new(Stream.LookAhead().Location);

        if (left is null || !Stream.Next(TokenValues.Pow)) return null;

        pow.Left = left;

        Expression? right = ParseExpressionLv3();
        if (right == null)
        {
            Stream.MoveBack(2);
            return null;
        }
        pow.Right = right;

        return ParseExpressionLv3_(pow);
    }


    private BooleanExpression? ParseBooleanExpression()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv1();
        return ParseBooleanExpressionLv1_(newLeft!);
    }
    private BooleanExpression? ParseBooleanExpressionLv1()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv2();
        return ParseBooleanExpressionLv2_(newLeft!);
    }
    private BooleanExpression? ParseBooleanExpressionLv1_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.And))
        {
            And and = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv2()
            };
            return ParseBooleanExpressionLv1_(and);
        }

        if (Stream.Next(TokenValues.Or))
        {
            Or or = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv2()
            };
            return ParseBooleanExpressionLv1_(or);
        }        

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv2()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv3();
        return ParseBooleanExpressionLv3_(newLeft!); 
    }
    private BooleanExpression? ParseBooleanExpressionLv2_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.Equal))
        {
            Equal equal = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(equal);
        }        

        if (Stream.Next(TokenValues.Different))
        {
            Different different = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(different);
        }        

        if (Stream.Next(TokenValues.Less))
        {
            Less less = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(less);
        }        

        if (Stream.Next(TokenValues.LessOrEqual))
        {
            LessOrEqual lessOrEqual = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(lessOrEqual);
        }        

        if (Stream.Next(TokenValues.More))
        {
            More more = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(more);
        }        

        if (Stream.Next(TokenValues.MoreOrEqual))
        {
            MoreOrEqual moreOrEqual = new(Stream.LookAhead().Location)
            {
                Left = left,
                Right = ParseBooleanExpressionLv3()
            };
            return ParseBooleanExpressionLv2_(moreOrEqual);
        }        

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv3()
    {
        BooleanExpression? newLeft = ParseBooleanExpressionLv4();
        return newLeft; 
    }
    private BooleanExpression? ParseBooleanExpressionLv3_(BooleanExpression left)
    {
        if (Stream.Next(TokenValues.Not))
        {
            Not not = new(Stream.LookAhead().Location);
            not.Right = ParseBooleanExpressionLv4();
            return ParseBooleanExpressionLv3_(not);
        }

        return left;
    }
    private BooleanExpression? ParseBooleanExpressionLv4()
    {
        if (Stream.Next(TokenType.BooleanExpression))
        {
        
        }
        throw new NotImplementedException();
    }

}