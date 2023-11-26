public class FunctionDeclare : ASTNode
{
    //Se crea un tipo para guardar la estructura de las funciones declaras que contienen el nombre de la función, una lista de los argumentos que 
    //tiene y una lista con los nodos que conforman los comandos que ejecutan.
    public string Id { get; set; }
    public List<string> Arguments { get; set; }
    public List<ASTNode> Statement { get; set; }
    public FunctionDeclare(string id, List<string> arguments, List<ASTNode> statement, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
        Statement = statement;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar que sea correcta semánticamente lo que hay que comprobar es que cada uno de sus nodos lo sean.
        bool check = true;
        bool x = true;

        context.AddFunc(Id, Arguments.Count);
        foreach (var item in Arguments)
        {
            scope.AddFuncVar(item, null!);
        }

        foreach (var item in Statement)
        {
            if (item is Expression expression)
            {
                check = expression.CheckSemantic(context, scope, errors);
            }
            else if (item is Variable variable)
            {
                check = variable.CheckSemantic(context, scope, errors);
            }
            else if (item is FunctionCall call)
            {
                check = call.CheckSemantic(context, scope, errors);
            }
            else if (item is Conditional conditional)
            {
                check = conditional.CheckSemantic(context, scope, errors);
            }
            else if (item is Print print)
            {
                check = print.CheckSemantic(context, scope, errors);
            }
            else if (item is ElementalFunction elem)
            {
                check = elem.CheckSemantic(context, scope, errors);
            }

            if (check is false) x = false;
        }
        
        return check && x;
    }

    public override void Evaluate(Context context, Scope scope) { }
}


public class FunctionCall : ASTNode
{
    //Sus propiedades son el nombre de la función que llama y  las expresiones que darán valor a los argumentos, también se le atribuyen instancias 
    
    //de Context y Scope para utilizar en su evaluación.
    public string Id { get; set; }
    public List<Expression> Arguments { get; set; }
    public FunctionCall(string id, List<Expression> arguments, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para asegurarse de su correcta semántica se comprueba si la función fue creada y si cada una de las expresiones de los argumentos son correctas.
        bool check = true;

        if (!context.ContainFunc(Id))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "This function has not been declare, so it can't be call"));
            check = false;
        }

        foreach (var item in Arguments)
        {
            if (!item.CheckSemantic(context, scope, errors))
            {
                errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The arguments are not correctly declared"));
                check = false;
            }
        }

        int func = context.GetArgNumber(Id);
        int call = Arguments.Count;

        if (func != call)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("The number of arguments is incorrect. {0} arguments were expected and {1} were given", func, call)));
            check = false; 
        }

        return check;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        //Se instancia la función a la que está llamando para poder acceder a sus valores, si solo contiene una expresión se imprime el resultado, de lo 
        //contrario se realiza la evaluación de cada uno de los nodos.

        FunctionDeclare func = context.GetFunction(Id);

        List<string> variable = func.Arguments;
        Dictionary<string, Expression> redefine = new();

        //Para poder utilizar los argumentos como variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que 
        //compone su valor antes de añadirla, el nombre se toma de la lista de argumentos de func utilizando como referencia la variable x que aumenta en 
        //cada iteración y el valor de la lista de argumentos sobre la que se está iterando y pertenece al llamado de la función.
        //En caso que una variable con el mismo id ya fuera declarada se guarda el nombre y valor anteriormente guardados y se declara la nueva variable.
        int x = 0;
        foreach (var item in Arguments)
        {
            if (scope.ContainFuncVar(variable[x]))
            {
                redefine.Add(variable[x], scope.FuncVars[variable[x]]);
                scope.FuncVars.Remove(variable[x]);
            }
            item.Evaluate(context, scope);
            scope.AddFuncVar(variable[x], item);
            x++;
        }


        //Se obtiene el cuerpo de la función y se evalúa cada uno de los nodos que la conforman.
        List<ASTNode> body = func.Statement;
        foreach (var item in body)
        {
            if (item is Expression expression)
            {
                expression.Evaluate(context, scope);
                Console.WriteLine(expression.Value!.ToString());
            }
            else if (item is ElementalFunction elem)
            {
                elem.Evaluate(context, scope);
                Console.WriteLine(elem.Value!.ToString());
            }
            else if (item is Variable variable1)
            {
                variable1.Evaluate(context, scope);
            }
            else if (item is FunctionCall call)
            {
                call.Evaluate(context, scope);
            }
            else if (item is Conditional conditional)
            {
                conditional.Evaluate(context, scope);
            }
            else if (item is Print print)
            {
                print.Evaluate(context, scope);
            }
        }

        //Al finalizar con la evaluación del cuerpo de la función se eliminan las variables que pertenecen a este llamado y si se tuvieron que redefinir 
        //algunas (como ocurriría en llamados recursivos) se vuelven a añadir con sus valores originales.
        foreach (var item in variable)
        {
            scope.FuncVars.Remove(item);
            if (redefine.ContainsKey(item))
            {
                scope.AddFuncVar(item, redefine[item]);
            }
        }
    }
}