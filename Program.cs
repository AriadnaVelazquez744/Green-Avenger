﻿//Lo primero que hace es inicializar el analizador léxico para que genere los tipos y valores de tokens y poder 
//luego iterar por el IEnumerable que devuelve su método principal que genera los tokens.
//Se lee la línea introducida en la consola que se va a analizar.

LexicalAnalyzer lex = Lexer.LexicalAnalyzer;

string text = Console.ReadLine()!;

//Se inicializa el IEnumerable de los tokens llamando el método correspondiente del lex con el texto
//Se inicializan también las clases TokenStream, Parser para poder utilizar sus métodos y propiedades, la lista 
//de errores para ir guardándolos y el nodo de programa elemental que es el que evaluará todas las estructuras que se creen

IEnumerable<Token> tokens = lex.GetTokens(text, new List<CompilingError>());

TokenStream stream = new(tokens);
Parser parser = new(stream);
List<CompilingError> errors = new();

MainProgram program = parser.Parse(errors);


//Si han ocurrido errores durante el proceso de análisis sintáctico se imprimen en consola, si no se pasa a realizar 
//el análisis semántico, se repite el proceso de que si han habido errores se imprimen y si no se evalúa y devuelven 
//los valores correspondientes.

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