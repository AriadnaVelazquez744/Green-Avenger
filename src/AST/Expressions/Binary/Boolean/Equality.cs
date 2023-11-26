public class Equality : BinaryExpression
{
    public Equality(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }
    public string? Op { get; set; }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        //Se comprueba si ambos lados tienen una correcta semántica, se hace la diferenciación de si solo está el miembro derecho por el caso de Not que no contiene ambos miembros.
        bool right = Right!.CheckSemantic(context, scope, errors);
        bool left = Left!.CheckSemantic(context, scope, errors);

        if (! Right.Type.Equals(Left.Type))
        {
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "It's impossible to equal different types of elements"));
            Type = ExpressionType.ErrorType;
            return false;
        }
        
        Type = ExpressionType.Boolean;
        return right && left;
    }

    public override void Evaluate(Context context, Scope scope)
    {
        Right!.Evaluate(context, scope);
        Left!.Evaluate(context, scope);

        bool eq = Left.Value!.Equals(Right.Value!);
        
        if (Op == "==")
            Value = eq;
        else
            Value = !eq;
    }

    public override string ToString()
    {
        if (Value == null)
            return String.Format("({0} {1} {2})", Left, Op, Right);
        return Value.ToString()!;
    }
}