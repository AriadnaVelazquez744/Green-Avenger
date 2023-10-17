public abstract class ASTNode
{
    //Nodo del árbol de análisis semántico de mi interpretado, contiene la localización del elemento y una propiedad que hereda a sus hijos que es para comprobar la semántica
    public CodeLocation Location {get; set;}

    public ASTNode(CodeLocation location)
    {
        Location = location;
    }
    
    public abstract bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors);    
}

public class Statement : ASTNode
{
    public Statement(CodeLocation location) : base(location) { }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        return true;
    }
}