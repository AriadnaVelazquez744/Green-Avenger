public class Concat : BinaryExpression
{
    // Al tener la concatenacion un simbolo particualr es posible unir cualquier tipo de expresion de manera tal que se convierta en texto.
    public Concat(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (!(Left.Type == ExpressionType.Number || Left.Type is not ExpressionType.Undeclared || Left.Type == ExpressionType.Text || Left.Type == ExpressionType.Boolean) && !(Right.Type == ExpressionType.Number || Right.Type == ExpressionType.Text || Right.Type == ExpressionType.Boolean || Right.Type is not ExpressionType.Undeclared))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "Impossible to realize the concatenation of these expressions"));
            Type = ExpressionType.ErrorType;
            return false;
        }

        Type = ExpressionType.Text;
        return right && left;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Right!.Evaluate(context, scope);
        Left!.Evaluate(context, scope);

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