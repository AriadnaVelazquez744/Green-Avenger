public class ElementalFunctions
{
    public Dictionary<string, Token[]> Functions { get; set; }
    public Dictionary<string, Func<Number, double>> MathFunction { get; }
    public Dictionary<string, Func<double>> Constants { get; }
    public Dictionary<string, Func<double, double, double>> Log { get; }
    public Dictionary<string, Func<object, object>> Print { get; }

    public ElementalFunctions()
    {
        Functions = new();

        MathFunction = new()
        {
            { "sin", (Number argument) => Math.Sin((double)argument.Value!) },
            { "cos", (Number argument) => Math.Cos((double)argument.Value!) },
            { "sqrt", (Number argument) => Math.Sqrt((double)argument.Value!) },
            { "exp", (Number argument) => Math.Exp((double)argument.Value!) }
        };

        Constants = new()
        {
            { "PI", () => Math.PI },
            { "E", () => Math.E }
        };

        Log = new()
        {
            { "log", Math.Log }
        };

        Print = new()
        {
            { "print", PrintReturn }
        };

        object PrintReturn(object argument)
        {
            Console.WriteLine(argument.ToString());
            return argument;
        }
    }

    // public void AddFunction(string name, Token[] code)
    // {
    //     Functions.Add(name, code);
    // }
}