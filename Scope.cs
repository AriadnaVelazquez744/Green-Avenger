public class Scope
{
    public Scope? Parent { get; set; }
    public List<string> Variables { get; set; }
    public List<string> Functions { get; set; }
    public List<string> ElementalFunctions { get; set; }

    public Scope()
    {
        Functions = new List<string>();
        Variables = new List<string>();
        ElementalFunctions = new List<string>();
    }

    public Scope CreateChild()
    {
        Scope child = new()
        {
            Parent = this
        };

        return child;
    }
}