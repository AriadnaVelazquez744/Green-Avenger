public class MainProgram : ASTNode
{
    //Sus propiedades son la lista de nodos a analizar y la ubicación que proviene de su padre.
    public List<ASTNode> Statements { get; set; }
    public MainProgram(List<ASTNode> statements, CodeLocation location) : base(location)
    {
        Statements = statements;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Para revisar la semántica se comprueba que todos los nodos sean semánticamente correctos.

        bool right = true;
        
        foreach (var item in Statements)
        {
            if (!item.CheckSemantic(context, scope, errors))
            {
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
                item.Evaluate();
                Statements.Remove(item);
            }
        }

        //Luego se evalúan ordenadamente los nodos restantes
        foreach (ASTNode item in Statements)
        {
            item.Evaluate();
        }
    }
}