LexicalAnalyzer lex = Lexer.LexicalAnalyzer;

string text = Console.ReadLine()!;

IEnumerable<Token> tokens = lex.GetTokens(text, new List<CompilingError>());

TokenStream stream = new(tokens);
Parser parser = new(stream);
List<CompilingError> errors = new();

ElementalProgram program = parser.Parse(errors);

if (errors.Count > 0)
{
    foreach (var error in errors)
        Console.WriteLine("{0}, {1}, {2}", error.Location.Column, error.Code, error.Argument);
}
else
{
    Context context = new();
    Scope scope = new();

    program.CheckSemantic(context, scope, errors);

    if (errors.Count > 0)
    {
        foreach (var error in errors)
            Console.WriteLine("{0}, {1}, {2}", error.Location.Column, error.Code, error.Argument);
    }
    else
    {
        program.Evaluate();
        //Console.WriteLine(program);
    }
}