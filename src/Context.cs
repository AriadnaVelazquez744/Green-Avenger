public class Context
{
    //Elementos globales que es necesario revisar para evitar repeticiones o errores
    public List<string> ElementalFunctions { get; set; }
    public List<string> Functions { get; set; }

    public Context()
    {
        ElementalFunctions = new()
        {
            "sin", "cos", "sqrt", "exp", "PI", "E", "log", "rand"
        };
        Functions = new();
    }
}