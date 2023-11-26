public class Variable : ASTNode
{
    //Las variables creadas cuentan con un diccionario de las variables que se pueden haber creado para el mismo scope al 
    //haberse separado por comas, y lo que sería como su rango de uso que es todo en lo que se puede utilizar esa variable
    public Dictionary<string, Expression> Variables { get; set; }
    public List<ASTNode> Range { get; set; }
    public Variable(Dictionary<string, Expression> variable, List<ASTNode> range, CodeLocation location) : base(location)
    {
        Variables = variable;
        Range = range;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los elementos que conforman su rango de utilización sean semánticamente correctos.
        bool x = true;

        //Este diccionario contendrá las variables que se modifican dentro de un scope hijo para que se puedan devolver a su valor original una vez terminado.
        Dictionary<string, Expression> redefine = new();
        
        //Para evaluar las variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que compone su valor antes de añadirla
        foreach (var item in Variables)
        {
            x = item.Value.CheckSemantic(context, scope, errors);
            //En caso de que la variable haya sido declarada en un scope padre de añaden los elementos de esa variable al diccionario redefine para guardarla y se elimina del scope para poder incluir la nueva
            if (scope.ContainsVariable(item.Key))
            {
                redefine.Add(item.Key, scope.Variables[item.Key]);
                scope.Variables.Remove(item.Key);
            }
            scope.AddVariable(item.Key, item.Value!);
        }

        if (Range is not null)
        {
            foreach (var item in Range)
            {
                if (item is Expression expression)
                {
                    x = expression.CheckSemantic(context, scope, errors);
                }
                else if (item is Variable variable)
                {
                    x = variable.CheckSemantic(context, scope, errors);
                }
                else if (item is FunctionCall call)
                {
                    x = call.CheckSemantic(context, scope, errors);
                }
                else if (item is Conditional conditional)
                {
                    x = conditional.CheckSemantic(context, scope, errors);
                }
                else if (item is Print print)
                {
                    x = print.CheckSemantic(context, scope, errors);
                }
            }
        }
        
        if (x is false)
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "At least one of the action where the variables are use has a Semantic problem"));
        
        //Finalmente se eliminan las variables del scope para que no puedan ser utilizadas en otro entorno que no sea el ya definido y evaluado.
        foreach (var item in Variables)
        {
            scope.Variables.Remove(item.Key);
            if (redefine.ContainsKey(item.Key))
            {
                scope.AddVariable(item.Key, redefine[item.Key]);
            }
        }

        return x;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        //Este diccionario contendrá las variables que se modifican dentro de un scope hijo para que se puedan devolver a su valor original una vez terminado.
        Dictionary<string, Expression> redefine = new();
        
        //Para evaluar las variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que compone su valor antes de añadirla
        foreach (var item in Variables)
        {
            //item.Value.Evaluate(context, scope);
            //En caso de que la variable haya sido declarada en un scope padre de añaden los elementos de esa variable al diccionario redefine para guardarla y se elimina del scope para poder incluir la nueva
            if (scope.ContainsVariable(item.Key))
            {
                redefine.Add(item.Key, scope.Variables[item.Key]);
                scope.Variables.Remove(item.Key);
            }
            scope.AddVariable(item.Key, item.Value);
        }

        //Lo siguiente es evaluar cada elemento que compone su rango de utilización, en caso de que no tenga rango los 
        //valores de las variables se imprimen en consola (esto incluye si llevaron algún tipo de calculo o evaluación 
        //el resultado final de la operación)
        if (Range is not null)
        {
            foreach (var item in Range)
            {
                if (item is Expression expression)
                {
                    expression.Evaluate(context, scope);
                    Console.WriteLine(expression.Value!.ToString());
                }
                else if (item is Variable variable)
                {
                    variable.Evaluate(context, scope);
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
        }
        else
        {
            foreach (var item in Variables)
            {
                item.Value.Evaluate(context, scope);
                Console.WriteLine(scope.Variables[item.Key].Value!.ToString());
            }
        }

        //Finalmente se eliminan las variables del scope para que no puedan ser utilizadas en otro entorno que no sea el ya definido y evaluado.
        foreach (var item in Variables)
        {
            scope.Variables.Remove(item.Key);
            if (redefine.ContainsKey(item.Key))
            {
                scope.AddVariable(item.Key, redefine[item.Key]);
            }
        }
    }
}