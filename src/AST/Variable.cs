public class Variable : Expression
{
    //Las variables creadas cuentan con un diccionario de las variables que se pueden haber creado para el mismo scope al 
    //haberse separado por comas, y lo que sería como su rango de uso que es todo en lo que se puede utilizar esa variable
    //y que para esta version del hulk es una unica expresion.
    public Dictionary<string, Expression> Variables { get; set; }
    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public Expression? Range { get; set; }
    public Variable(Dictionary<string, Expression> variable, Expression range, CodeLocation location) : base(location)
    {
        Variables = variable;
        Range = range;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que la expresión que conforma su rango de utilización sea semánticamente correcta.
        bool check = true;
        bool x = true; 

        //Este diccionario contendrá las variables que se modifican dentro de un scope hijo para que se puedan devolver a su valor original una vez terminado.
        Dictionary<string, Expression> redefine = new();
        
        //Para revisar las variables lo primero es añadirlas al scope para poder determinar l existencia de sus valores.
        foreach (var item in Variables)
        {
            check = item.Value.CheckSemantic(context, scope, errors);
            if (check is false) x = false;
            //En caso de que la variable haya sido declarada en un scope padre de añaden los elementos de esa variable al diccionario redefine para guardarla y se elimina del scope para poder incluir la nueva
            if (scope.ContainsVariable(item.Key))
            {
                redefine.Add(item.Key, scope.Variables[item.Key]);
                scope.Variables.Remove(item.Key);
            }
            scope.AddVariable(item.Key, item.Value!);
        }

        if (Range is null)
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The expression that use the declared variables can't be null"));
            x = false;
        }
        else
        {
            check = Range!.CheckSemantic(context, scope, errors);
            if (check is false)  x = false;

            Type = Range.Type;
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

        return check && x;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        //Este diccionario contendrá las variables que se modifican dentro de un scope hijo para que se puedan devolver a su valor original una vez terminado.
        Dictionary<string, Expression> redefine = new();
        
        //Para evaluar las variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que compone su valor antes de añadirla
        foreach (var item in Variables)
        {
            //En caso de que la variable haya sido declarada en un scope padre de añaden los elementos de esa variable al diccionario redefine para guardarla y se elimina del scope para poder incluir la nueva
            if (scope.ContainsVariable(item.Key))
            {
                redefine.Add(item.Key, scope.Variables[item.Key]);
                scope.Variables.Remove(item.Key);
            }
            scope.AddVariable(item.Key, item.Value);
        }

        //Se evalúa la expresión que compone su rango de utilización y ese es el valor que toma el nodo
        Range!.Evaluate(context, scope);
        Value = Range.Value;
        
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