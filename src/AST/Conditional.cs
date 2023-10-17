
public class Conditional : ASTNode
{
    public BooleanExpression Condition { get; set; }
    public Statement IfBody { get; set; }
    public Statement ElseBody { get; set; }
    public Conditional(BooleanExpression condition, Statement ifBody, Statement elseBody, CodeLocation location) : base(location)
    {
        Condition = condition;
        IfBody = ifBody;
        ElseBody = elseBody;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        if (!Condition.CheckSemantic(context, scope, errors))
            return false;
        
        if (!IfBody.CheckSemantic(context, scope, errors))
            return false;

        if (ElseBody != null && !ElseBody.CheckSemantic(context, scope, errors))
            return false;

        return true;
    }
}