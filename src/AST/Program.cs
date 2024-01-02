using System.Linq.Expressions;

public class MainProgram : ASTNode
{
    //Sus propiedades son la lista de nodos a analizar.
    public List<ASTNode> Statements { get; set; }
    public MainProgram(List<ASTNode> statements, CodeLocation location) : base(location)
    {
        Statements = statements;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los nodos sean semánticamente correctos.

        bool check = true;
        bool x = true;

        foreach (ASTNode item in Statements)
        {
            if (item is FunctionDeclare declare)
            {
                check = declare.CheckSemantic(context, scope, errors);
                if (check is false)  x = false;
            }
        }
        
        foreach (var item in Statements)
        {
            if (item is FunctionDeclare)
            {    
                continue;
            }
            else
            {
                check = ((Expression)item).CheckSemantic(context, scope, errors);
            }
            
            if (check is false)  x = false;
        }

        if (x is false)
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "At least one of the statement has a semantic problem"));

        return check && x;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        foreach (ASTNode item in Statements)
        {
            if (item is FunctionDeclare declare)
            {
                context.AddFuncExpression(declare);                
            }
        }

        //Luego se evalúan ordenadamente los nodos restantes
        foreach (ASTNode item in Statements)
        {
            if (item is Expression expression)
            {
                expression.Evaluate(context, scope);
                Console.WriteLine(expression.Value!.ToString());
            }
            else if (item is FunctionDeclare)
            { 
                continue;
            }
        }
    }
}