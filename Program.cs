LexicalAnalyzer lex = Lexer.LexicalAnalyzer;
Context context = new();
Scope scope = new();

while (true)
{
    Console.Write(">");
    string text = Console.ReadLine()!;
    if(text == "")
        break;
    
    List<CompilingError> errors = new();
    IEnumerable<Token> tokens = lex.GetTokens(text, errors);
    TokenStream stream = new(tokens);
    
    if (errors.Count > 0)
    {
        foreach (var error in errors)
            Console.WriteLine("{0}, {1}, {2}, Lexical Error", error.Location.Column, error.Code, error.Argument);
    }
    else
    {
        Parser parser = new(stream, context, scope);        
        MainProgram program = parser.Parse(errors);
        
        //Si han ocurrido errores durante el proceso de análisis sintáctico se imprimen en consola, si no se pasa a realizar 
        //el análisis semántico, se repite el proceso de que si han habido errores se imprimen y si no se evalúa y devuelven 
        //los valores correspondientes.
        
        if (errors.Count > 0)
        {
            foreach (var error in errors)
                Console.WriteLine("{0}, {1}, {2}, Syntax Error", error.Location.Column, error.Code, error.Argument);
        }
        else
        {
            program.CheckSemantic(context, scope, errors);
        
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Console.WriteLine("{0}, {1}, {2}, Semantic Error", error.Location.Column, error.Code, error.Argument);
            }
            else
            {
                program.Evaluate(context, scope);
            }
            scope.Variables.Clear();
        }
    }
}