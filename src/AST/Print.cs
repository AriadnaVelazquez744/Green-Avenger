public class Print : ASTNode
{
    public Expression? Expression { get; set; }
    public Print(Expression expression, CodeLocation location) : base(location)
    {
        Expression = expression;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        return Expression!.CheckSemantic(context, scope, errors);
    }

    public override void Evaluate(Context context, Scope scope)
    {
        if (Expression is not null)
        {
            Expression.Evaluate(context, scope);
            Console.WriteLine(Expression.Value!.ToString());
        }
    }
}