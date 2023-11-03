public class Context
{
    //Se encuentran los elementos globales que es necesario revisar para evitar repeticiones o errores.
    //Se añaden y llaman las funciones que se van declarando para poder utilizarlas cuando sea necesario una vez comprobado que son semánticamente correctas.
    //Los métodos implementados en esta clase son bastante básicos y con leer el nombre se tiene la idea de que hacen por lo que no los comentaré por separado.
    public List<string> ElementalFunctions { get; set; }
    public Dictionary<string, int> Functions { get; set; }
    public Dictionary<string, FunctionDeclare> FunctionDeclared { get; set; }

    public Context()
    {
        ElementalFunctions = new()
        {
            "sin", "cos", "sqrt", "exp", "PI", "E", "log", "rand"
        };
        Functions = new();
        FunctionDeclared = new();
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
    public void AddFuncExpression(FunctionDeclare newFunc)
    {
        FunctionDeclared.Add(newFunc.Id, newFunc);
    }
    public int GetArgNumber(string name)
    {
        return Functions[name];
    }
    public FunctionDeclare GetFunction(string id)
    {
        return FunctionDeclared[id];
    }
}