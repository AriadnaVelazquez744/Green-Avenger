public abstract class ASTNode
{
    public CodeLocation Location {get; set;}

    public ASTNode(CodeLocation location)
    {
        Location = location;
    }
    
    public abstract bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors);    
}