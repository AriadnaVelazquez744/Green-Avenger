public class Context
{
    public List<string> ElementalFunctions { get; set; }
    public List<string> Functions { get; set; }

    public Context()
    {
        ElementalFunctions = new List<string>();
        Functions = new List<string>();
    }
}