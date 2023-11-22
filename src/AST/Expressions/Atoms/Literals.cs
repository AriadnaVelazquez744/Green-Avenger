public class BoolLiteral : AtomExpression
{
    public BoolLiteral(bool value, CodeLocation location) : base(location)
    { 
        Value = value;
    }

    public override ExpressionType Type 
    { 
        get => ExpressionType.Boolean; 
        set { } 
    }
    public override object? Value { get; set; }

    public bool IsBool
    {
        get
        {
            bool a;
            return bool.TryParse(Value!.ToString(), out a);
        }
    }
}