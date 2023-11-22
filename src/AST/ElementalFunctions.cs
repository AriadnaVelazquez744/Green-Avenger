public class ElementalFunction : ASTNode
{    
    //Recibe el nombre de la función a llamar y la lista de los argumentos, tiene como propiedades tres 
    //diccionarios que tienen las funciones definidas y listas para que solo sea pasarles el valor y que 
    //devuelvan el número correspondiente.
    public Dictionary<string, Func<double, double>> MathFunction { get; }
    public Dictionary<string, Func<double>> Constants { get; }
    public Dictionary<string, Func<double, double, double>> Log { get; }
    public string Id { get; }
    List<Expression> Arg { get; set; }
    public double Value { get; set; }
    
    public ElementalFunction(string id, List<Expression> arg, CodeLocation location) : base(location)
    {
        //Los diccionarios de las funciones contienen elementos del tipo Func que se encuentran definidas en este espacio
        //Además se creó el método rand para obtener un valor numérico aleatorio entre 0 y 1 para el llamado del mismo nombre.
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

        Id = id;
        Arg = arg;
    }

    public override bool CheckSemantic(Context context, Scope scope, List<CompilingError> errors)
    {
        bool check = true;
        
        if (Arg.Count == 1 && !MathFunction.ContainsKey(Id))
        {
            check = false;
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The number of arguments don't match"));
        }
        else if (Arg.Count != 2 && Id == "log")
        {
            check = false;
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The number of arguments don't match"));
        }
        else if (Arg.Count == 0 && !Constants.ContainsKey(Id))
        {
            check = false;
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The number of arguments don't match"));
        }
        else if (Arg.Count > 2)
        {
            check = false;
            errors.Add(new CompilingError(Location, ErrorCode.Invalid, "The number of arguments don't match"));
        }

        foreach (var item in Arg)
        {
            check = item.CheckSemantic(context, scope, errors);
        }

        return check;
    }

    public override void Evaluate()
    {
        if (Arg.Count == 0)
        {
            Value = Constants[Id]();
        }
        
        foreach (var item in Arg)
        {
            item.Evaluate();
        }

        List<double> arguments = new();

        foreach (var item in Arg)
        {
            if (!Double.TryParse(item.Value!.ToString(), out double x))
                throw new Exception("It's impossible to use a value different to a double in elemental functions");

            else
                arguments.Add(x);
        }

        if (arguments.Count == 1)
        {
            Value = MathFunction[Id](arguments[0]);
        }
        else if (arguments.Count == 2)
        {
            Value = Log[Id](arguments[0], arguments[1]);
        }
    }
}