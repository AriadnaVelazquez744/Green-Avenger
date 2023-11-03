public class FunctionDeclare : ASTNode
{
    //Se crea un tipo para guardar la estructura de las funciones declaras que contienen el nombre de la función, una lista de los argumentos que 
    //tiene y una lista con los nodos que conforman los comandos que ejecutan.
    public string Id { get; set; }
    public List<string> Arguments { get; set; }
    public List<ASTNode> Statement { get; set; }
    public Context Context { get; set; }
    public FunctionDeclare(string id, List<string> arguments, List<ASTNode> statement, Context context, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
        Statement = statement;
        Context = context;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar que sea correcta semánticamente lo que hay que comprobar es que cada uno de sus nodos lo sean.

        foreach (var item in Statement)
        {
            if (!item.CheckSemantic(context, scope, errors))
            {
                return false;
            }
        }
        
        return true;
    }

    public override void Evaluate()
    {
        FunctionDeclare newFunc = new(Id, Arguments, Statement, Context, Location);
        Context.AddFuncExpression(newFunc);
    }
}


public class FunctionCall : ASTNode
{
    //Sus propiedades son el nombre de la función que llama y  las expresiones que darán valor a los argumentos, también se le atribuyen instancias 
    
    //de Context y Scope para utilizar en su evaluación.
    public string Id { get; set; }
    public List<Expression> Arguments { get; set; }
    public Context Context { get; set; }
    public Scope Scope { get; set; }
    public FunctionCall(string id, List<Expression> arguments, Context context, Scope scope, CodeLocation location) : base(location)
    {
        Id = id;
        Arguments = arguments;
        Context = context;
        Scope = scope;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para asegurarse de su correcta semántica se comprueba si la función fue creada y si cada una de las expresiones de los argumentos son correctas.

        if (!context.ContainFunc(Id))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "This function has not been declare, so it can't be call"));
            return false;
        }

        foreach (var item in Arguments)
        {
            if (!item.CheckSemantic(context, scope, errors))
            {
                errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The arguments are not correctly declared"));
                return false;
            }
        }
        return true;
    }

    public override void Evaluate()
    {
        //Se instancia la función a la que está llamando para poder acceder a sus valores, si solo contiene una expresión se imprime el resultado, de lo 
        //contrario se realiza la evaluación de cada uno de los nodos.

        FunctionDeclare func = Context.GetFunction(Id);

        List<string> variable = func.Arguments;
        Dictionary<string, object> redefine = new();

        //Para poder utilizar los argumentos como variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que 
        //compone su valor antes de añadirla, el nombre se toma de la lista de argumentos de func utilizando como referencia la variable x que aumenta en 
        //cada iteración y el valor de la lista de argumentos sobre la que se está iterando y pertenece al llamado de la función.
        //En caso que una variable con el mismo id ya fuera declarada se guarda el nombre y valor anteriormente guardados y se declara la nueva variable.
        int x = 0;
        foreach (var item in Arguments)
        {
            if (Scope.ContainFuncVar(variable[x]))
            {
                redefine.Add(variable[x], Scope.FuncVars[variable[x]]);
                Scope.FuncVars.Remove(variable[x]);
            }

            Scope.AddFuncVar(variable[x], item.Value!);
            x++;
        }


        //Se obtiene el cuerpo de la función y se evalúa cada uno de los nodos que la conforman.
        List<ASTNode> body = func.Statement;
        if ((body.Count == 1) && (body[0] is Expression))
        {
            foreach (var item in body)
            {
                Console.WriteLine(item.ToString());
            }
        }
        else
        {
            foreach (var item in body)
            {
                item.Evaluate();
            }
        }


        //Al finalizar con la evaluación del cuerpo de la función se eliminan las variables que pertenecen a este llamado y si se tuvieron que redefinir 
        //algunas (como ocurriría en llamados recursivos) se vuelven a añadir con sus valores originales.
        foreach (var item in variable)
        {
            Scope.FuncVars.Remove(item);
            if (redefine.ContainsKey(item))
            {
                Scope.AddFuncVar(item, redefine[item]);
            }
        }
    }
}