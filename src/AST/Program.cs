public class ElementalProgram : ASTNode
{
    public List<CompilingError> Errors { get; set; }
    public List<ASTNode> Elements { get; set; }

    public ElementalProgram(List<ASTNode> elements, CodeLocation location) : base(location)
    {
        Errors = new();
        Elements = elements;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        throw new NotImplementedException();
    }

    public override void Evaluate()
    {
        throw new NotImplementedException();
    }
}