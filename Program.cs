// =============================================================================
//  Program.cs — Punto de entrada del compilador usando Scanner/Parser de Coco/R
// =============================================================================

using System;
using System.IO;

namespace CompiladorCocoR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            MostrarBanner();

            bool soloTokens = args.Length > 1 && args[0] == "--tokens";
            string archivo = soloTokens ? args[1]
                            : args.Length > 0 ? args[0]
                            : null;

            if (!File.Exists(archivo))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: archivo '{archivo}' no encontrado.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"\n  Compilando: {archivo}\n");

            if (soloTokens)
                MostrarTokens(archivo);
            else
                CompilarArchivo(archivo);
        }

        //  Pipeline usando Coco/R
        static void CompilarArchivo(string archivo)

        {

            try

            {

                var scanner = new Scanner(archivo);

                var parser = new Parser(scanner);

                parser.Parse();

                Console.WriteLine("\nAnálisis terminado.");

                Console.WriteLine($"Errores sintácticos/léxicos detectados por Coco/R: {parser.errors.count}");

                Console.WriteLine($"Errores semánticos detectados: {parser.errores.TotalErrores}");

                if (parser.errors.count == 0 && !parser.errores.HayErrores)

                {

                    Console.ForegroundColor = ConsoleColor.DarkGreen;

                    Console.WriteLine("Compilación finalizada sin errores.");

                    Console.ResetColor();

                    parser.ImprimirTablaSimbolos();

                    Console.WriteLine();
                    Console.WriteLine("\n");
                    Console.WriteLine("\nEstructura del programa");
                    Console.WriteLine("------------------------------------------------------------------------");
                    parser.ImprimirArbolSintactico();

                    parser.generador.Imprimir();

                    parser.generador.Evaluar();

                }

                else

                {

                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("La compilación terminó con errores.");

                    Console.ResetColor();

                    if (parser.errores.TotalErrores > 0)
                        parser.errores.MostrarResumen();

                }

            }

            catch (Exception ex)

            {

                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("Error al ejecutar el compilador:");

                Console.WriteLine(ex.Message);

                Console.ResetColor();

            }

        }
        //  Mostrar tokens usando Scanner generado por Coco/R
        static void MostrarTokens(string archivo)
        {
            try
            {
                var scanner = new Scanner(archivo);

                Console.WriteLine("Tabla de tokens:");
                Console.WriteLine("kind\tline\tcol\tval");
                Console.WriteLine("----------------------------------------");

                Token token;
                do
                {
                    token = scanner.Scan();
                    Console.WriteLine($"{token.kind}\t{token.line}\t{token.col}\t{token.val}");
                }
                while (token.kind != 0);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error al tokenizar:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        static void MostrarBanner()
        {
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(@"
       ╔══════════════════════════════════════════════════════════════╗
       ║                         COMPILADOR                           ║
       ║        Carolina de los Santos y Nancy Silva Alvarez          ║
       ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
    }
}