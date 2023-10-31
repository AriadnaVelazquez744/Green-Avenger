public class Context
{
    //Elementos globales que es necesario revisar para evitar repeticiones o errores
    public List<string> ElementalFunctions { get; set; }
    public Dictionary<string, int> Functions { get; set; }

    public Context()
    {
        ElementalFunctions = new()
        {
            "sin", "cos", "sqrt", "exp", "PI", "E", "log", "rand"
        };
        Functions = new();
    }

    public bool ContainsElemFunc(string name)
    {
        return ElementalFunctions.Contains(name);
    }
    public bool ContainFunc(string name)
    {
        return Functions.ContainsKey(name);
    }

    public void AddFunc(string name, int n)
    {
        Functions.Add(name, n);
    }
    public int GetArgNumber(string name)
    {
        return Functions[name];
    }
}