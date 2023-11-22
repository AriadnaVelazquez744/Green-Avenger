public class Concat : BinaryExpression
{
    public Concat(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (!(Left.Type == ExpressionType.Number || Left.Type == ExpressionType.Text || Left.Type == ExpressionType.Boolean) && !(Right.Type == ExpressionType.Number || Right.Type == ExpressionType.Text || Right.Type == ExpressionType.Boolean))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "Impossible to realize the concatenation of the expressions"));
            Type = ExpressionType.ErrorType;
            return false;
        }

        Type = ExpressionType.Text;
        return right && left;
    }

    public override void Evaluate()
    {
        Right!.Evaluate();
        Left!.Evaluate();

        Value = String.Format("{0}{1}", Left.Value!.ToString(), Right.Value!.ToString());
    }

    public override string? ToString()
    {
        if (Value == null)
        {
            return String.Format("({0} @ {1})", Left, Right);
        }
        return Value.ToString();
    }
}