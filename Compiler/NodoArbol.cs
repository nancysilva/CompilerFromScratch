using System;
using System.Collections.Generic;

namespace CompiladorCocoR
{
    public class NodoArbol
    {
        public string Nombre { get; set; }
        public List<NodoArbol> Hijos { get; set; } = new List<NodoArbol>();

        public NodoArbol(string nombre)
        {
            Nombre = nombre;
        }

        public void Agregar(NodoArbol hijo)
        {
            Hijos.Add(hijo);
        }

        public void Imprimir(string prefijo = "", bool esUltimo = true)
        {
            Console.Write(prefijo);
            Console.Write(esUltimo ? "└── " : "├── ");
            Console.WriteLine(Nombre);

            for (int i = 0; i < Hijos.Count; i++)
            {
                Hijos[i].Imprimir(
                    prefijo + (esUltimo ? "    " : "│   "),
                    i == Hijos.Count - 1
                );
            }
        }
    }
}