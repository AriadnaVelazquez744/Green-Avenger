
public class Print : ASTNode
{
    public Expression? Expression { get; set; }
    public Print(Expression expression, CodeLocation location) : base(location)
    {
        Expression = expression;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        throw new NotImplementedException();
    }

}