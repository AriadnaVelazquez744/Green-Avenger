public class Variable : ASTNode
{
    //Las variables creadas cuentan con un diccionario de las variables que se pueden haber creado para el mismo scope al 
    //haberse separado por comas, y lo que sería como su rango de uso que es todo en lo que se puede utilizar esa variable
    public Dictionary<string, Expression> Variables { get; set; }
    public List<ASTNode> Range { get; set; }
    public Scope Scope { get; set; }
    public Variable(Dictionary<string, Expression> variable, List<ASTNode> range,Scope scope, CodeLocation location) : base(location)
    {
        Variables = variable;
        Range = range;
        Scope = scope;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los elementos que conforman su rango de utilización sean semánticamnete correctos.

        foreach (var item in Range)
        {
            if (!item.CheckSemantic(context, scope, errors))
                return false;
        }

        return true;
    }

    public override void Evaluate()
    {
        //Este diccionario contendrá las variables que se modifican dentro de un scope hijo para que se puedan devolver a su valor original una vez terminado.
        Dictionary<string, object> redefine = new();
        //Para evaluar las variables lo primero es añadirlas al scope para poder llamar sus valores, y se evalúa la expresión que compone su valor antes de añadirla
        foreach (var item in Variables)
        {
            //En caso de que la variable haya sido declarada en un scope padre de añaden los elementos de esa variable al diccionario redefine para guardarla y se elimina del scope para poder incluir la nueva
            if (Scope.ContainsVariable(item.Key))
            {
                redefine.Add(item.Key, Scope.Variables[item.Key]);
                Scope.Variables.Remove(item.Key);
            }
            Scope.AddVariable(item.Key, item.Value.Value!);
        }

        //Lo siguiente es evaluar cada elemento que compone su rango de utilización, en caso de que no tenga rango los 
        //valores de las variables se imprimen en consola (esto incluye si llevaron algún tipo de calculo o evaluación 
        //el resultado final de la operación)
        if (Range is not null)
        {
            foreach (var item in Range)
            {
                item.Evaluate();
            }
        }
        else
        {
            foreach (var item in Variables)
            {
                Console.WriteLine(Scope.Variables[item.Key].ToString());
            }
        }

        //Finalmente se eliminan las variables del scope para que no puedan ser utilizadas en otro entorno que no sea el ya definido y evaluado.
        foreach (var item in Variables)
        {
            Scope.Variables.Remove(item.Key);
            if (redefine.ContainsKey(item.Key))
            {
                Scope.AddVariable(item.Key, redefine[item.Key]);
            }
        }
    }
}