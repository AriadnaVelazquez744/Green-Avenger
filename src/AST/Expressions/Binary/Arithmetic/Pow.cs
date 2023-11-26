public class Pow :BinaryExpression
{
    public Pow(CodeLocation location) : base(location) { }

    public override ExpressionType Type { get; set; }
    public override object? Value { get; set; }

    public override void Evaluate(Context context, Scope scope)
    {
        Left!.Evaluate(context, scope);
        Right!.Evaluate(context, scope);

        Value = Math.Pow((double)Left.Value!, (double)Right.Value!);
    }

    public override string? ToString()
    {
        if (Value == null)
            return String.Format("({0} ^ {1})", Left, Right);

        return Value!.ToString();
    }
}