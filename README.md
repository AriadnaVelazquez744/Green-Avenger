# Green-Avenger

Funcionamiento básico de Green-Avenger:
1.	Abrir el proyecto: se puede hacer de dos formas, ejecutando el proyecto desde un editor de código o ejecutando desde la consola de comando el script Green-Avenger.sh
2.	Escribir el código: esperar hasta que salga el carácter ‘>’ , luego escribir la línea de comando que se desea ejecutar en la consola y presionar “Enter”; esto hará que el intérprete procese el comando y devuelva la respuesta correspondiente.
3.	Escribir un nuevo comando o finalizar la ejecución: una vez finalizado el análisis del comando anterior, sale nuevamente el carácter ‘>’ para introducir una nueva línea, si se desea detener el programa solo es necesario dar enter con la línea en blanco.

Características del interpretador de Green-Avenger:
- La utilización del símbolo booleano ‘!’ tiene la particularidad de que si se está utilizando en una condición compuesta por el ‘!’ es un miembro y otra expresión en el otro es necesario que la parte que pertenece a ‘!’ se encuentre encerrada entre paréntesis para su correcta lectura, ejemplo:  (!...) & …
- Los errores se imprimirán es la consola indicando la ubicación que responde al número del primer carácter del elemento en caso de error léxico, del token en caso de error sintáctico o de uno de los primeros elementos de la estructura en caso de error semántico; el tipo de error que podría ser un elemento esperado que no se encontró (Expected), una instrucción inválida (Invalid) o un error desconocido (Unknown); y una breve especificación de en qué consistió el error, que será tan explícita como el caso lo permita.
- Cada comando ejecutado devuelve un valor con excepción de la declaración de las funciones que si se hace sola no devuelve nada.
- No está permitido utilizar el mismo nombre para el argumento de dos funciones diferentes, en caso de que ocurra se notificará d este suceso como un error semántico; además, en este caso es posible intentar nuevamente definir la función con la misma estructuración y nombre cambiando la variable problemática.

