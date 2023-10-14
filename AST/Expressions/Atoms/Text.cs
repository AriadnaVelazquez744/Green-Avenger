public class Text : AtomExpression
{
    public Text(string value, CodeLocation location) : base(location)
    {
        Value = value;
    }

    public override ExpressionType Type 
    { 
        get => ExpressionType.Text;
        set { }
    }

    public override object? Value { get; set; }
}