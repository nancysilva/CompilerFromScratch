// =============================================================================
//  TablaSimbolos.cs — Tabla de Símbolos del Compilador
//  Registrar variables, funciones y arreglos con su tipo y scope.
// =============================================================================

using System;
using System.Collections.Generic;

namespace CompiladorCocoR
{
    public class EntradaSimbolo
    {
        public string Nombre     { get; set; }  // nombre del identificador
        public string Tipo       { get; set; }  // int | float | char | bool
        public string Scope      { get; set; }  // "global" o nombre de función
        public string Valor      { get; set; }  // valor actual (para constantes)
        public bool   EsArreglo  { get; set; }  // true si tiene dimensión
        public int    Tamano     { get; set; }  // tamaño del arreglo
        public int    Linea      { get; set; }  // línea de declaración

        public EntradaSimbolo(string nombre, string tipo, string scope, int linea,
                              bool esArreglo = false, int tamano = 0)
        {
            Nombre    = nombre;
            Tipo      = tipo;
            Scope     = scope;
            Valor     = "";
            EsArreglo = esArreglo;
            Tamano    = tamano;
            Linea     = linea;
        }

        public override string ToString()
            => $"| {Nombre,-15} | {Tipo,-8} | {Scope,-12} | {Valor,-10} | {(EsArreglo ? $"[{Tamano}]" : "—"),-8} | Línea {Linea}";
    }

    public class EntradaFuncion
    {
        public string       Nombre       { get; set; }
        public string       TipoRetorno  { get; set; }
        public int NumParametros { get; set; }
        public int          Linea        { get; set; }

        public EntradaFuncion(string nombre, string tipoRetorno, int numParametros, int linea)
        {
            Nombre      = nombre;
            TipoRetorno = tipoRetorno;
            NumParametros = numParametros;
            Linea       = linea;
        }

        public override string ToString()
            => $"| {Nombre,-15} | {TipoRetorno,-8} | params={NumParametros/*TiposParams.Count*/} | Línea {Linea}";
    }

    public class TablaSimbolos
    {
        // Diccionario: clave = "scope::nombre"
        private Dictionary<string, EntradaSimbolo> _variables  = new Dictionary<string, EntradaSimbolo>();
        private Dictionary<string, EntradaFuncion> _funciones  = new Dictionary<string, EntradaFuncion>();

        public bool AgregarVariable(string nombre, string tipo, string scope, int linea)
        {
            string clave = Clave(nombre, scope);
            if (_variables.ContainsKey(clave))
                return false;   // ya declarada → error semántico

            _variables[clave] = new EntradaSimbolo(nombre, tipo, scope, linea);
            return true;
        }

        public bool AgregarArreglo(string nombre, string tipo, int tamano, string scope, int linea)
        {
            string clave = Clave(nombre, scope);
            if (_variables.ContainsKey(clave))
                return false;

            _variables[clave] = new EntradaSimbolo(nombre, tipo, scope, linea,
                                                    esArreglo: true, tamano: tamano);
            return true;
        }

        public bool AgregarFuncion(string nombre, string tipoRetorno, int numParams, int linea)
        {
            if (_funciones.ContainsKey(nombre))
                return false;   // función duplicada

            _funciones[nombre] = new EntradaFuncion(nombre, tipoRetorno, numParams, linea);
            return true;
        }

        //  Buscar variable: primero en scope local, luego en global
        public EntradaSimbolo Buscar(string nombre, string scope)
        {
            // Scope local primero
            string claveLocal  = Clave(nombre, scope);
            if (_variables.TryGetValue(claveLocal, out var entradaLocal))
                return entradaLocal;

            // Luego global
            string claveGlobal = Clave(nombre, "global");
            if (_variables.TryGetValue(claveGlobal, out var entradaGlobal))
                return entradaGlobal;

            return null;   // no encontrada
        }

        public EntradaFuncion BuscarFuncion(string nombre)
        {
            _funciones.TryGetValue(nombre, out var f);
            return f;
        }

        public bool ExisteFuncion(string nombre)
        {

            return _funciones.ContainsKey(nombre);

        }

        //  Actualizar valor de una variable (para constantes / evaluación)
        public void SetValor(string nombre, string scope, string valor)
        {
            string claveLocal = Clave(nombre, scope);
            if (_variables.ContainsKey(claveLocal))
            {
                _variables[claveLocal].Valor = valor;
                return;
            }
            string claveGlobal = Clave(nombre, "global");
            if (_variables.ContainsKey(claveGlobal))
                _variables[claveGlobal].Valor = valor;
        }

        //  Imprimir tabla de variables
        public void Imprimir()
        {
            Console.WriteLine("\n");
            Console.WriteLine("                                    Variables                                     ");
            Console.WriteLine("----------------------------------------------------------------------------------");
            Console.WriteLine($"  {"Nombre",-15}   {"Tipo",-8}   {"Scope",-12}   {"Valor",-10}   {"Dim",-8}");
            Console.WriteLine("----------------------------------------------------------------------------------");
            foreach (var kv in _variables)
                Console.WriteLine("  " + kv.Value.ToString());

            if (_funciones.Count > 0)
            {
                Console.WriteLine("\n");
                Console.WriteLine("                                Funciones                                ");
                Console.WriteLine("-------------------------------------------------------------------------");
                Console.WriteLine($"  {"Nombre",-15}   {"Retorno",-8}   {"Params"}");
                Console.WriteLine("-------------------------------------------------------------------------");
                foreach (var kv in _funciones)
                    Console.WriteLine("  " + kv.Value.ToString());
            }
        }

        private string Clave(string nombre, string scope) => $"{scope}::{nombre}";
    }
}
