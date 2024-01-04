# Green-Avenger

Funcionamiento b�sico de Green-Avenger:
1.	Abrir el proyecto: se puede hacer de dos formas, ejecutando el proyecto desde un editor de c�digo o ejecutando desde la consola de comando el script Green-Avenger.sh
2.	Escribir el c�digo: esperar hasta que salga el car�cter �>� , luego escribir la l�nea de comando que se desea ejecutar en la consola y presionar �Enter�; esto har� que el int�rprete procese el comando y devuelva la respuesta correspondiente.
3.	Escribir un nuevo comando o finalizar la ejecuci�n: una vez finalizado el an�lisis del comando anterior, sale nuevamente el car�cter �>� para introducir una nueva l�nea, si se desea detener el programa solo es necesario dar enter con la l�nea en blanco.

Caracter�sticas del interpretador de Green-Avenger:
�	La utilizaci�n del s�mbolo booleano �!� tiene la particularidad de que si se est� utilizando en una condici�n compuesta por el �!� es un miembro y otra expresi�n en el otro es necesario que la parte que pertenece a �!� se encuentre encerrada entre par�ntesis para su correcta lectura, ejemplo:  (!...) & �
�	Los errores se imprimir�n es la consola indicando la ubicaci�n que responde al n�mero del primer car�cter del elemento en caso de error l�xico, del token en caso de error sint�ctico o de uno de los primeros elementos de la estructura en caso de error sem�ntico; el tipo de error que podr�a ser un elemento esperado que no se encontr� (Expected), una instrucci�n inv�lida (Invalid) o un error desconocido (Unknown); y una breve especificaci�n de en qu� consisti� el error, que ser� tan expl�cita como el caso lo permita.
�	Cada comando ejecutado devuelve un valor con excepci�n de la declaraci�n de las funciones que si se hace sola no devuelve nada.
�	No est� permitido utilizar el mismo nombre para el argumento de dos funciones diferentes, en caso de que ocurra se notificar� d este suceso como un error sem�ntico; adem�s, en este caso es posible intentar nuevamente definir la funci�n con la misma estructuraci�n y nombre cambiando la variable problem�tica.

