// =============================================================================
//  ManejadorErrores.cs — Control de Errores del Compilador
//  Maneja errores léxicos, sintácticos y semánticos.
// =============================================================================

using System;
using System.Collections.Generic;

namespace CompiladorCocoR
{
    public enum TipoError
    {
        Lexico,
        Sintactico,
        Semantico
    }

    public class RegistroError
    {
        public TipoError Tipo   { get; set; }
        public int       Linea  { get; set; }
        public string    Mensaje { get; set; }

        public RegistroError(TipoError tipo, int linea, string mensaje)
        {
            Tipo    = tipo;
            Linea   = linea;
            Mensaje = mensaje;
        }

        public override string ToString()
        {
            string tipoStr = Tipo switch
            {
                TipoError.Lexico     => "Error léxico",
                TipoError.Sintactico => "Error sintáctico",
                TipoError.Semantico  => "Error semántico",
                _                    => "Error"
            };
            return $"  {tipoStr} [Línea {Linea}]: {Mensaje}";
        }
    }
   
    public class ManejadorErrores
    {
        private List<RegistroError> _errores = new List<RegistroError>();

        public int TotalErrores    => _errores.Count;
        public bool HayErrores     => _errores.Count > 0;

        public void ErrorLexico(int linea, string mensaje)
        {
            var err = new RegistroError(TipoError.Lexico, linea, mensaje);
            _errores.Add(err);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(err);
            Console.ResetColor();
        }

        public void Error(int linea, string mensaje)
        {
            // Por defecto: semántico (usado desde el parser)
            var err = new RegistroError(TipoError.Semantico, linea, mensaje);
            _errores.Add(err);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(err);
            Console.ResetColor();
        }

        public void ErrorSintactico(int linea, string mensaje)
        {
            var err = new RegistroError(TipoError.Sintactico, linea, mensaje);
            _errores.Add(err);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(err);
            Console.ResetColor();
        }

        public void MostrarResumen()
        {
            Console.WriteLine("\n");
            Console.WriteLine("                        RESUMEN DE COMPILACIÓN                          ");
            Console.WriteLine("------------------------------------------------------------------------");

            if (!HayErrores)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  Compilación exitosa — Sin errores.");
                Console.ResetColor();
                return;
            }

            int lex = 0, sin = 0, sem = 0;
            foreach (var e in _errores)
            {
                switch (e.Tipo)
                {
                    case TipoError.Lexico:     lex++; break;
                    case TipoError.Sintactico: sin++; break;
                    case TipoError.Semantico:  sem++; break;
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  Total de errores: {TotalErrores}");
            Console.ResetColor();
            Console.WriteLine($"     Léxicos:{lex}");
            Console.WriteLine($"     Sintácticos:{sin}");
            Console.WriteLine($"     Semánticos:{sem}");

            Console.WriteLine("\n  Detalle:");
            foreach (var e in _errores)
                Console.WriteLine(e);
        }

        //  Obtener todos los errores (para pruebas unitarias)
        public List<RegistroError> ObtenerErrores() => _errores;
    }
}
