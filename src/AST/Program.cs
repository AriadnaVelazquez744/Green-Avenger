using System.Linq.Expressions;

public class MainProgram : ASTNode
{
    //Sus propiedades son la lista de nodos a analizar y la ubicación que proviene de su padre.
    public List<ASTNode> Statements { get; set; }
    public Context Context { get; set; }
    public MainProgram(List<ASTNode> statements, Context context, CodeLocation location) : base(location)
    {
        Statements = statements;
        Context = context;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los nodos sean semánticamente correctos.

        bool right = true;
        
        foreach (var item in Statements)
        {
            if (item is Expression)
            {
                if (!((Expression)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
            else if (item is Variable)
            {
                if (!((Variable)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
            else if (item is FunctionCall)
            {
                if (!((FunctionCall)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
            else if (item is Conditional)
            {
                if (!((Conditional)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
            else if (item is Print)
            {
                if (!((Print)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
            else if (item is FunctionDeclare)
            {
                if (!((FunctionDeclare)item).CheckSemantic(context, scope, errors))
                    right = false;
            }
        }

        if (right is false)
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "At least one of the statement has a semantic problem"));

        return right;
    }

    public override void Evaluate()
    {
        //Se evalúan primero las declaraciones de funciones y se eliminan de la lista de nodos para no volver a tomarlas.
        foreach (ASTNode item in Statements)
        {
            if (item is FunctionDeclare)
            {
                Context.AddFuncExpression((FunctionDeclare)item);                
                Statements.Remove(item);
            }
        }

        //Luego se evalúan ordenadamente los nodos restantes
        foreach (ASTNode item in Statements)
        {
            if (item is Expression)
            {
                ((Expression)item).Evaluate();
                Console.WriteLine(((Expression)item).Value!.ToString());
            }
            else if (item is Variable)
            {
                ((Variable)item).Evaluate();
            }
            else if (item is FunctionCall)
            {
                ((FunctionCall)item).Evaluate();
            }
            else if (item is Conditional)
            {
                ((Conditional)item).Evaluate();
            }
            else if (item is Print)
            {
                ((Print)item).Evaluate();
            }
        }
    }
}