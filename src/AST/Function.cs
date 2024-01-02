public class FunctionDeclare : ASTNode
{
    //Se crea un tipo para guardar la estructura de las funciones declaras que contienen el nombre de la función, una lista de los argumentos que 
    //ha de recibir y una lista con los nodos que conforman los comandos que ejecuta.
    public string Id { get; set; }
    public Dictionary<string, Var> Arguments { get; set; }
    public Expression? Statement { get; set; }
    public ExpressionType Type { get; set; }
    public FunctionDeclare(string id, Dictionary<string, Var> arguments, Expression? statement, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
        Statement = statement;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar que sea correcta semánticamente lo que hay que comprobar es que cada uno de sus nodos lo sean.
        bool check = true;

        //Pero primero hay que establecer que existe la función y cuales son los argumentos que la conforman para que no hayan errores en las evaluaciones de las expresiones internan 
        //que utilicen estos argumentos como variables.
        List<ExpressionType> types = new();

        foreach (var item in Arguments)
        {
            if (!scope.ContainFuncVar(item.Key))
            {
                scope.AddFuncVar(item.Key, item.Value);
            }
            else
            {
                errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("A function already use {0} to name one of his variables", item.Key)));
                return false;
            }
        }

        context.AddFunc(Id, Arguments.Count);

        if (Statement is not null)
        {
            check = Statement.CheckSemantic(context, scope, errors);
            Type = Statement.Type;
        }
        else
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("The function {0} doesn't contain a statement", Id)));
            check = false;
        }

        if (check)
        {
            foreach (var item in Arguments)
            {
                types.Add(scope.FuncVars[item.Key].Type);
            }
            context.AddFuncTypesList(Id, types);
        }


        return check;
    }

    public override void Evaluate(Context context, Scope scope) { }
}


public class FunctionCall : Expression
{
    //Sus propiedades son el nombre de la función que llama y  las expresiones que darán valor a los argumentos
    
    //de Context y Scope para utilizar en su evaluación.
    public string Id { get; set; }
    public List<Expression> Arguments { get; set; }
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public FunctionCall(string id, List<Expression> arguments, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para asegurarse de su correcta semántica se comprueba si la función fue creada, si cada una de las expresiones de los argumentos son correctas y  si el número de argumentos de 
        //la llamada coinciden con los requeridos en  la declaración de la función
        //Finalmente se le asigna el tipo correspondiente al valor de retorno de la función o Undeclared para evitar que se vaya vacío
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
        else if (context.FunctionTypes.ContainsKey(Id))
        {
            List<ExpressionType> types = context.GetTypesList(Id);
            int x = 0;
            foreach (var item in Arguments)
            {
                if (item.Type != types[x])
                {
                    errors.Add(new CompilingError(Location, ErrorCode.Invalid, String.Format("The type of the argument is incorrect. {0} type were expected and {1} type were given", types[x].ToString(), item.Type.ToString())));
                    check = false;
                }
                x++;
            }
        }
       
        if (context.FunctionDeclared.ContainsKey(Id))
            Type = context.FunctionDeclared[Id].Type;
        else
            Type = ExpressionType.Undeclared;

        return check;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        //Se instancia la función a la que está llamando para poder acceder a sus valores y evaluar las expresiones que contiene.

        FunctionDeclare func = context.GetFunction(Id);

        Dictionary<string, Var> variable = func.Arguments;
        List<string> ids = new();
        foreach (var item in variable)
        {
            ids.Add(item.Key);
        }
        Dictionary<string, Expression> redefine = new();

        //Para poder utilizar los argumentos como variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que 
        //compone su valor antes de añadirla, el nombre se toma de la lista de argumentos de func utilizando como referencia la variable x que aumenta en 
        //cada iteración y el valor de la lista de argumentos sobre la que se está iterando y pertenece al llamado de la función.
        //En caso que una variable con el mismo id ya fuera declarada se guarda el nombre y valor anteriormente guardados y se declara la nueva variable.
        int x = 0;
        foreach (var item in Arguments)
        {
            Expression exp = item;
            if ((item is Var var) && !scope.ContainFuncVar(var.Id))
            {
                exp = scope.Variables[var.Id];
            }
            exp.Evaluate(context, scope);
            exp.Type = item.Type;
            if (scope.ContainFuncVar(ids[x]))
            {
                redefine.Add(ids[x], scope.FuncVars[ids[x]]);
                scope.FuncVars.Remove(ids[x]);
            }
            scope.AddFuncVar(ids[x], exp);
            x++;
        }


        //Se obtiene el cuerpo de la función y se evalúa cada uno de los nodos que la conforman.
        Expression body = func.Statement!;
        if (body is not null)
        {
            body.Evaluate(context, scope);
            Value = body.Value;
        }

        //Al finalizar con la evaluación del cuerpo de la función se eliminan las variables que pertenecen a este llamado y si se tuvieron que redefinir 
        //algunas (como ocurriría en llamados recursivos) se vuelven a añadir con sus valores originales, esto ocurre siempre en el caso de que cada una de estas variables están declaras en un inicio.
        foreach (var item in ids)
        {
            scope.FuncVars.Remove(item);
            if (redefine.ContainsKey(item))
            {
                scope.AddFuncVar(item, redefine[item]);
            }
        }
    }
}