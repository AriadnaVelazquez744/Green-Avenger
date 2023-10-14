
public abstract class BooleanExpression : Expression
{
    public BooleanExpression(CodeLocation location) : base(location) { }

    public Expression? Left { get; set; }
    public Expression? Right { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (!(Left.Type == ExpressionType.Number || Left.Type == ExpressionType.Text || Left.Type == ExpressionType.Boolean) && !(Right.Type == ExpressionType.Number || Right.Type == ExpressionType.Text || Right.Type == ExpressionType.Boolean))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "That's not a correct boolean declaration"));
            Type = ExpressionType.ErrorType;
            return false;
        }

        Type = ExpressionType.Boolean;
        return right && left;
    }
}
