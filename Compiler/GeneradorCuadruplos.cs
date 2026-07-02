// =============================================================================
//  GeneradorCuadruplos.cs — Generación de Código Intermedio
//                (operador, arg1, arg2, resultado)
// =============================================================================

using System;
using System.Collections.Generic;

namespace CompiladorCocoR
{

    public class Cuadruplo
    {
        public int Indice { get; set; }
        public string Operador { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Resultado { get; set; }
        public bool EsEtiqueta { get; set; }  // para labels como L1:

        public Cuadruplo(int idx, string op, string a1, string a2, string res, bool esEtiqueta = false)
        {
            Indice = idx;
            Operador = op;
            Arg1 = a1;
            Arg2 = a2;
            Resultado = res;
            EsEtiqueta = esEtiqueta;
        }

        public override string ToString()
        {
            if (EsEtiqueta)
                return $"{Resultado}:";          // ej: L1:

            return $"({Indice:D3})  ({Operador,-8}, {Arg1,-10}, {Arg2,-10}, {Resultado})";
        }
    }

    public class GeneradorCuadruplos
    {
        private List<Cuadruplo> _cuadruplos = new List<Cuadruplo>();
        private int _contador = 0;  // índice del siguiente cuádruplo
        private int _contEtiq = 0;

        private Dictionary<string, int> _inicioFunciones = new Dictionary<string, int>();

        public void Agregar(string operador, string arg1, string arg2, string resultado)
        {
            _cuadruplos.Add(new Cuadruplo(_contador++, operador, arg1, arg2, resultado));
        }

        public void AgregarEtiqueta(string etiqueta)
        {
            _cuadruplos.Add(new Cuadruplo(_contador, "-", "-", "-", etiqueta, esEtiqueta: true));
        }

        public string NuevaEtiqueta() => $"L{_contEtiq++}";

        //  Retorna el índice del próximo cuádruplo (para parches de saltos)
        public int ProximoIndice() => _contador;

        public List<Cuadruplo> ObtenerCuadruplos() => _cuadruplos;

        public void Imprimir()
        {
            Console.WriteLine("\n");
            Console.WriteLine("                              Cuádruplos                                ");
            Console.WriteLine("------------------------------------------------------------------------");

            if (_cuadruplos.Count == 0)
            {
                Console.WriteLine("  (No se generaron cuádruplos)");
                return;
            }

            foreach (var c in _cuadruplos)
                Console.WriteLine("  " + c.ToString());

            Console.WriteLine("------------------------------------------------------------------------");
        }

        // Ejecutar el programa
        public Dictionary<string, object> Evaluar()
        {
            var memoria = new Dictionary<string, object>();  // nombre → valor
            var salida = new List<string>();
            var parametrosPendientes = new List<object>();               // salidas del WRITE
            int pc = BuscarInicioMain();                                      // program counter

            // Construir mapa de etiquetas para saltos rápidos
            var etiquetas = new Dictionary<string, int>();
            for (int i = 0; i < _cuadruplos.Count; i++)
                if (_cuadruplos[i].EsEtiqueta)
                    etiquetas[_cuadruplos[i].Resultado] = i;

            Console.WriteLine("\n");
            Console.WriteLine("                       EJECUCIÓN DEL PROGRAMA                           ");
            Console.WriteLine("------------------------------------------------------------------------");

            while (pc < _cuadruplos.Count)
            {
                var c = _cuadruplos[pc];

                if (c.EsEtiqueta) { pc++; continue; }

                switch (c.Operador)
                {
                    // ── Asignación ──────────────────────────────────────
                    case ":=":
                        {

                            object valor = ObtenerValor(c.Arg1, memoria);

                            string destino = ResolverAccesoArreglo(c.Resultado, memoria);

                            memoria[destino] = valor;

                            break;

                        }

                    // ── Aritméticos ──────────────────────────────────────
                    case "+":
                        memoria[c.Resultado] = Sumar(ObtenerValor(c.Arg1, memoria),
                                                     ObtenerValor(c.Arg2, memoria));
                        break;
                    case "-":
                        memoria[c.Resultado] = Restar(ObtenerValor(c.Arg1, memoria),
                                                      ObtenerValor(c.Arg2, memoria));
                        break;
                    case "*":
                        memoria[c.Resultado] = Multiplicar(ObtenerValor(c.Arg1, memoria),
                                                           ObtenerValor(c.Arg2, memoria));
                        break;
                    case "/":
                        memoria[c.Resultado] = Dividir(ObtenerValor(c.Arg1, memoria),
                                                       ObtenerValor(c.Arg2, memoria));
                        break;
                    case "%":
                        memoria[c.Resultado] = Modulo(ObtenerValor(c.Arg1, memoria),
                                                      ObtenerValor(c.Arg2, memoria));
                        break;

                    // ── Negación ──────────────────────────────────
                    case "NEG":
                        var vn = ObtenerValor(c.Arg1, memoria);
                        memoria[c.Resultado] = vn is double d ? (object)(-d) : (object)(-(int)vn);
                        break;

                    // ── Relacionales ─────────────────────────────────────
                    case "<":
                        memoria[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoria)) <
                                               ToDouble(ObtenerValor(c.Arg2, memoria));
                        break;
                    case ">":
                        memoria[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoria)) >
                                               ToDouble(ObtenerValor(c.Arg2, memoria));
                        break;
                    case "<=":
                        memoria[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoria)) <=
                                               ToDouble(ObtenerValor(c.Arg2, memoria));
                        break;
                    case ">=":
                        memoria[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoria)) >=
                                               ToDouble(ObtenerValor(c.Arg2, memoria));
                        break;
                    case "==":
                        memoria[c.Resultado] = ObtenerValor(c.Arg1, memoria)?.ToString() ==
                                               ObtenerValor(c.Arg2, memoria)?.ToString();
                        break;
                    case "!=":
                        memoria[c.Resultado] = ObtenerValor(c.Arg1, memoria)?.ToString() !=
                                               ObtenerValor(c.Arg2, memoria)?.ToString();
                        break;

                    // ── Lógicos ──────────────────────────────────────────
                    case "&&":
                        memoria[c.Resultado] = ToBool(ObtenerValor(c.Arg1, memoria)) &&
                                               ToBool(ObtenerValor(c.Arg2, memoria));
                        break;
                    case "||":
                        memoria[c.Resultado] = ToBool(ObtenerValor(c.Arg1, memoria)) ||
                                               ToBool(ObtenerValor(c.Arg2, memoria));
                        break;
                    case "!":
                        memoria[c.Resultado] = !ToBool(ObtenerValor(c.Arg1, memoria));
                        break;

                    // ── Saltos ───────────────────────────────────────────
                    case "GOTO":
                        if (etiquetas.TryGetValue(c.Resultado, out int destGoto))
                        { pc = destGoto; continue; }
                        break;

                    case "GOTOF":   // saltar si condición es FALSE
                        bool condF = ToBool(ObtenerValor(c.Arg1, memoria));
                        if (!condF && etiquetas.TryGetValue(c.Resultado, out int destF))
                        { pc = destF; continue; }
                        break;

                    // ── Entrada y Salida ──────────────────────────────────────────────
                    case "WRITE":
                        string salStr = FormatearSalida(ObtenerValor(c.Arg1, memoria));
                        Console.WriteLine("  → " + salStr);
                        salida.Add(salStr);
                        break;

                    case "INPUT":
                        Console.Write($"  Ingrese {c.Resultado}: ");
                        string inp = Console.ReadLine() ?? "0";
                        if (int.TryParse(inp, out int vi)) memoria[c.Resultado] = vi;
                        else if (double.TryParse(inp, System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture, out double vd))
                            memoria[c.Resultado] = vd;
                        else memoria[c.Resultado] = inp;
                        break;

                    // ── Funciones ────────────────────────────────────────
                    case "PARAM":
                        parametrosPendientes.Add(ObtenerValor(c.Arg1, memoria));
                        break;
                    case "CALL":
                        object retorno = EjecutarFuncion(c.Arg1, parametrosPendientes, memoria, etiquetas);
                        parametrosPendientes.Clear();

                        if (!string.IsNullOrEmpty(c.Resultado) && c.Resultado != "-")
                        {
                            memoria[c.Resultado] = retorno;
                        }

                        break;
                    case "RETURN":
                        return memoria;

                    default:
                        break;
                }
                pc++;
            }

            return memoria;
        }

        private int BuscarInicioMain()
        {
            int ultimoReturn = -1;

            for (int i = 0; i < _cuadruplos.Count; i++)
            {
                if (_cuadruplos[i].Operador == "RETURN")
                    ultimoReturn = i;
            }

            return ultimoReturn + 1;
        }

        public void RegistrarInicioFuncion(string nombreFuncion)
        {

            _inicioFunciones[nombreFuncion] = _contador;

        }

        private object EjecutarFuncion(
            string nombreFuncion,
            List<object> parametros,
            Dictionary<string, object> memoriaGlobal,
            Dictionary<string, int> etiquetas)
        {
            int pcFuncion = BuscarInicioFuncion(nombreFuncion);

            if (pcFuncion < 0)
                return 0;

            var memoriaLocal = memoriaGlobal;

            if (nombreFuncion == "suma")
            {
                if (parametros.Count > 0) memoriaLocal["a"] = parametros[0];
                if (parametros.Count > 1) memoriaLocal["b"] = parametros[1];
            }

            while (pcFuncion < _cuadruplos.Count)
            {
                var c = _cuadruplos[pcFuncion];

                if (c.EsEtiqueta)
                {
                    pcFuncion++;
                    continue;
                }

                switch (c.Operador)
                {
                    case "<":
                        memoriaLocal[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoriaLocal)) <
                                                    ToDouble(ObtenerValor(c.Arg2, memoriaLocal));
                        break;

                    case ">":
                        memoriaLocal[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoriaLocal)) >
                                                    ToDouble(ObtenerValor(c.Arg2, memoriaLocal));
                        break;

                    case "<=":
                        memoriaLocal[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoriaLocal)) <=
                                                    ToDouble(ObtenerValor(c.Arg2, memoriaLocal));
                        break;

                    case ">=":
                        memoriaLocal[c.Resultado] = ToDouble(ObtenerValor(c.Arg1, memoriaLocal)) >=
                                                    ToDouble(ObtenerValor(c.Arg2, memoriaLocal));
                        break;

                    case "==":
                        memoriaLocal[c.Resultado] = ObtenerValor(c.Arg1, memoriaLocal)?.ToString() ==
                                                    ObtenerValor(c.Arg2, memoriaLocal)?.ToString();
                        break;

                    case "!=":
                        memoriaLocal[c.Resultado] = ObtenerValor(c.Arg1, memoriaLocal)?.ToString() !=
                                                    ObtenerValor(c.Arg2, memoriaLocal)?.ToString();
                        break;

                    case ":=":
                        memoriaLocal[c.Resultado] = ObtenerValor(c.Arg1, memoriaLocal);
                        break;

                    case "+":
                        memoriaLocal[c.Resultado] = Sumar(
                            ObtenerValor(c.Arg1, memoriaLocal),
                            ObtenerValor(c.Arg2, memoriaLocal)
                        );
                        break;

                    case "-":
                        memoriaLocal[c.Resultado] = Restar(
                            ObtenerValor(c.Arg1, memoriaLocal),
                            ObtenerValor(c.Arg2, memoriaLocal)
                        );
                        break;

                    case "*":
                        memoriaLocal[c.Resultado] = Multiplicar(
                            ObtenerValor(c.Arg1, memoriaLocal),
                            ObtenerValor(c.Arg2, memoriaLocal)
                        );
                        break;

                    case "/":
                        memoriaLocal[c.Resultado] = Dividir(
                            ObtenerValor(c.Arg1, memoriaLocal),
                            ObtenerValor(c.Arg2, memoriaLocal)
                        );
                        break;

                    case "%":
                        memoriaLocal[c.Resultado] = Modulo(
                            ObtenerValor(c.Arg1, memoriaLocal),
                            ObtenerValor(c.Arg2, memoriaLocal)
                        );
                        break;

                    case "WRITE":
                        string salStr = FormatearSalida(ObtenerValor(c.Arg1, memoriaLocal));
                        Console.WriteLine("  → " + salStr);
                        break;

                    case "RETURN":
                        return ObtenerValor(c.Arg1, memoriaLocal);

                    case "GOTO":
                        if (etiquetas.TryGetValue(c.Resultado, out int destGoto))
                        {
                            pcFuncion = destGoto;
                            continue;
                        }
                        break;

                    case "GOTOF":
                        bool condF = ToBool(ObtenerValor(c.Arg1, memoriaLocal));
                        if (!condF && etiquetas.TryGetValue(c.Resultado, out int destF))
                        {
                            pcFuncion = destF;
                            continue;
                        }
                        break;
                }

                pcFuncion++;
            }

            return 0;
        }

        private int BuscarInicioFuncion(string nombreFuncion)

        {

            {

                if (_inicioFunciones.TryGetValue(nombreFuncion, out int inicio))

                    return inicio;

                Console.WriteLine($"  [ERROR] No se encontró el inicio de la función '{nombreFuncion}'.");

                return -1;

            }

        }

        private object ObtenerValor(string arg, Dictionary<string, object> memoria)

        {

            if (arg == null || arg == "-") return 0;

            // ¿Literal numérico?

            if (int.TryParse(arg, out int i)) return i;

            if (double.TryParse(arg, System.Globalization.NumberStyles.Any,

                System.Globalization.CultureInfo.InvariantCulture, out double d))

                return d;

            // ¿Literal booleano?

            if (arg == "true") return true;

            if (arg == "false") return false;

            // ¿String?

            if (arg.StartsWith("\"") && arg.EndsWith("\""))

                return arg.Substring(1, arg.Length - 2);

            // ¿Acceso a arreglo? 

            if (EsAccesoArreglo(arg))

            {

                string nombreReal = ResolverAccesoArreglo(arg, memoria);

                if (memoria.TryGetValue(nombreReal, out var valorArreglo))

                    return valorArreglo;

                return 0;

            }

            // ¿Variable normal?

            if (memoria.TryGetValue(arg, out var val))

                return val;

            return 0;

        }

        private bool EsAccesoArreglo(string texto)

        {

            return texto.Contains("[") && texto.Contains("]");

        }

        private string ResolverAccesoArreglo(string texto, Dictionary<string, object> memoria)

        {

            if (!EsAccesoArreglo(texto))

                return texto;

            int posAbre = texto.IndexOf("[");

            int posCierra = texto.IndexOf("]");

            if (posAbre < 0 || posCierra < 0 || posCierra <= posAbre)

                return texto;

            string nombreArreglo = texto.Substring(0, posAbre);

            string indiceTexto = texto.Substring(posAbre + 1, posCierra - posAbre - 1);

            object valorIndice = ObtenerValor(indiceTexto, memoria);

            int indice = Convert.ToInt32(valorIndice);

            return $"{nombreArreglo}[{indice}]";

        }

        private double ToDouble(object v)
        {
            if (v is bool b) return b ? 1 : 0;
            return Convert.ToDouble(v);
        }

        private bool ToBool(object v)
        {
            if (v is bool b) return b;
            if (v is int i) return i != 0;
            if (v is double d) return d != 0;
            return false;
        }

        private object Sumar(object a, object b)
        {
            if (a is double || b is double)
                return Convert.ToDouble(a) + Convert.ToDouble(b);
            return (int)a + (int)b;
        }

        private object Restar(object a, object b)
        {
            if (a is double || b is double)
                return Convert.ToDouble(a) - Convert.ToDouble(b);
            return (int)a - (int)b;
        }

        private object Multiplicar(object a, object b)
        {
            if (a is double || b is double)
                return Convert.ToDouble(a) * Convert.ToDouble(b);
            return (int)a * (int)b;
        }

        private object Dividir(object a, object b)
        {
            double denom = Convert.ToDouble(b);
            if (denom == 0) { Console.WriteLine("  [ERROR] División por cero."); return 0; }
            return Convert.ToDouble(a) / denom;
        }

        private object Modulo(object a, object b)
        {
            int denom = (int)b;
            if (denom == 0) { Console.WriteLine("  [ERROR] Módulo por cero."); return 0; }
            return (int)a % denom;
        }

        private string FormatearSalida(object v)
        {
            if (v is string s && s.StartsWith("\""))
                return s.Substring(1, s.Length - 2);
            return v?.ToString() ?? "";
        }
    }
}
