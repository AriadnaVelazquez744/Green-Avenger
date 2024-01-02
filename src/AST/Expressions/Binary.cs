public abstract class BinaryExpression : Expression
{
    // No se declaraa directamente ningun elemento como BinaryExpression por los q su objetivo es establecer un patron semantico general para las operaciones que presentan esta estructura matematica.
    public BinaryExpression(CodeLocation location) : base(location) { }
    
    public Expression? Left { get; set; }
    public Expression? Right { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> error)
    {
        bool right = Right!.CheckSemantic(context, scope, error);
        bool left = Left!.CheckSemantic(context, scope, error);

        if ((Right.Type != ExpressionType.Number && Right.Type != ExpressionType.Undeclared) || (Left.Type != ExpressionType.Number && Left.Type != ExpressionType.Undeclared))
        {
            error.Add(new CompilingError(Location, ErrorCode.Invalid, "We don't do that here... "));
            Type = ExpressionType.ErrorType;
            return false;
        }

        if (Right is Var varR && Right.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varR.Id].Type = ExpressionType.Number;
        }
        if (Left is Var varL && Left.Type == ExpressionType.Undeclared)
        {
            scope.FuncVars[varL.Id].Type = ExpressionType.Number;
        }
        Right.Type = Left.Type = ExpressionType.Number;

        Type = ExpressionType.Number;
        return right && left;
    }
}