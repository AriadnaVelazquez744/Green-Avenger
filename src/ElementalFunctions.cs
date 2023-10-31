public class ElementalFunction
{    
    //No hereda de nadie a pesar de formar parte de la semántica del interpretador, está formado con dict formados por las id de las funciones y objetos llamados Func que determinan que hace cada una de estas funciones predeterminadas en el lenguaje
    public Dictionary<string, Func<double, double>> MathFunction { get; }
    public Dictionary<string, Func<double>> Constants { get; }
    public Dictionary<string, Func<double, double, double>> Log { get; }
    
    public ElementalFunction()
    {
        MathFunction = new()
        {
            { "sin", (double argument) => Math.Sin(argument) },
            { "cos", (double argument) => Math.Cos(argument) },
            { "sqrt", (double argument) => Math.Sqrt(argument) },
            { "exp", (double argument) => Math.Exp(argument) }
        };

        Constants = new()
        {
            { "PI", () => Math.PI },
            { "E", () => Math.E },
            { "rand", Rand }
        };

        Log = new()
        {
            { "log", (double arg1, double arg2) => Math.Log(arg1, arg2) }
        };

        static double Rand()
        {
            Random random = new();
            double randomNumber = random.NextDouble();
            return randomNumber;
        }
    }
}