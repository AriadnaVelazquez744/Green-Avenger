public class ElementalProgram : ASTNode
{
    public List<CompilingError> Errors { get; set; }
    //public Dictionary<string, object> Variable { get; set; }
    public Dictionary<string, Function> Functions { get; set; }
    public ElementalFunction ElementalFunction { get; }

    public ElementalProgram(CodeLocation location) : base(location)
    {
        Errors = new();
        //Variable = new();
        Functions = new();
        ElementalFunction = new();
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        throw new NotImplementedException();
    }
}