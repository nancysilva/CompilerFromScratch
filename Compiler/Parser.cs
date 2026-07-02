
using System;

namespace CompiladorCocoR {



public class Parser {
	public const int _EOF = 0;
	public const int _lbrace = 1;
	public const int _rbrace = 2;
	public const int _lparen = 3;
	public const int _rparen = 4;
	public const int _lbracket = 5;
	public const int _rbracket = 6;
	public const int _semicolon = 7;
	public const int _comma = 8;
	public const int _colon = 9;
	public const int _assign = 10;
	public const int _plus = 11;
	public const int _minus = 12;
	public const int _times = 13;
	public const int _slash = 14;
	public const int _percent = 15;
	public const int _eq = 16;
	public const int _neq = 17;
	public const int _le = 18;
	public const int _ge = 19;
	public const int _lt = 20;
	public const int _gt = 21;
	public const int _andOp = 22;
	public const int _orOp = 23;
	public const int _notOp = 24;
	public const int _inc = 25;
	public const int _dec = 26;
	public const int _flotante = 27;
	public const int _entero = 28;
	public const int _cadena = 29;
	public const int _ident = 30;
	public const int maxT = 54;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

class ListaCadenas : System.Collections.Generic.List<string> {}

class IdDeclarado
{
    public string Nombre { get; set; }
    public bool EsArreglo { get; set; }
    public int Tam { get; set; }

    public IdDeclarado(string nombre, bool esArreglo, int tam)
    {
        Nombre = nombre;
        EsArreglo = esArreglo;
        Tam = tam;
    }
}

class ListaIdsDeclarados : System.Collections.Generic.List<IdDeclarado> {}

private TablaSimbolos tablaSimbolos = new TablaSimbolos();
public GeneradorCuadruplos generador  = new GeneradorCuadruplos();
public ManejadorErrores errores       = new ManejadorErrores();
public NodoArbol arbolSintactico = new NodoArbol("Programa");

private NodoArbol nodoVariables = new NodoArbol("Variables");
private NodoArbol nodoSentencias = new NodoArbol("Sentencias");

public void ImprimirTablaSimbolos()

{

    tablaSimbolos.Imprimir();

}

public void ImprimirArbolSintactico()

{

    arbolSintactico.Imprimir();

}

private int contTemp = 0;
private string NuevoTemp() => $"t{++contTemp}";

private string scopeActual = "global";

/*==========================================================================
   CHARACTERS
===========================================================================*/


	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void MiLenguaje() {
		arbolSintactico.Agregar(nodoVariables);
		
		arbolSintactico.Agregar(nodoSentencias);
		
		
		Expect(31);
		Expect(32);
		Expect(1);
		SeccionVar();
		DeclFunciones();
		BloquePrincipal();
		Expect(2);
	}

	void SeccionVar() {
		if (la.kind == 34) {
			Get();
			ListaDeclaraciones();
		}
	}

	void DeclFunciones() {
		while (la.kind == 33) {
			DeclaracionFuncion();
		}
	}

	void BloquePrincipal() {
		Expect(39);
		Expect(7);
		ListaSentencias();
		Expect(40);
		Expect(7);
	}

	void DeclaracionFuncion() {
		string nomFunc = ""; string tipoRetorno = "void"; int numParams = 0; 
		Expect(33);
		Expect(30);
		nomFunc = t.val; 
		Expect(3);
		ListaParametros(nomFunc, out numParams);
		Expect(4);
		if (la.kind == 9) {
			Get();
			Tipo(out tipoRetorno);
		}
		scopeActual = nomFunc;
		
		if (!tablaSimbolos.AgregarFuncion(nomFunc, tipoRetorno, numParams, la.line))
		   errores.Error(la.line, $"FunciÃ³n '{nomFunc}' ya declarada.");
		
		generador.RegistrarInicioFuncion(nomFunc);
		
		Expect(1);
		SeccionVar();
		BloquePrincipal();
		generador.Agregar("RETURN", "-", "-", "-");
		
		
		Expect(2);
		scopeActual = "global"; 
	}

	void ListaParametros(string func, out int numParams) {
		numParams = 0; 
		if (la.kind == 30) {
			Parametro(func);
			numParams++; 
			while (la.kind == 8) {
				Get();
				Parametro(func);
				numParams++; 
			}
		}
	}

	void Tipo(out string tipo) {
		tipo = ""; 
		if (la.kind == 35) {
			Get();
			tipo = "int";   
		} else if (la.kind == 36) {
			Get();
			tipo = "float"; 
		} else if (la.kind == 37) {
			Get();
			tipo = "char";  
		} else if (la.kind == 38) {
			Get();
			tipo = "bool";  
		} else SynErr(55);
	}

	void Parametro(string func) {
		string nom = ""; string tipo = ""; 
		Expect(30);
		nom = t.val; 
		Expect(9);
		Tipo(out tipo);
		tablaSimbolos.AgregarVariable(nom, tipo, func, la.line); 
	}

	void ListaDeclaraciones() {
		Declaracion();
		while (la.kind == 30) {
			Declaracion();
		}
	}

	void Declaracion() {
		ListaIdsDeclarados ids = new ListaIdsDeclarados();
		string tipo = ""; 
		ListaIds(ids);
		Expect(9);
		Tipo(out tipo);
		Expect(7);
		foreach (var id in ids)
		{
		 bool ok = id.EsArreglo
		   ? tablaSimbolos.AgregarArreglo(id.Nombre, tipo, id.Tam, scopeActual, la.line)
		   : tablaSimbolos.AgregarVariable(id.Nombre, tipo, scopeActual, la.line);
		
		 if (!ok)
		 {
		   errores.Error(la.line, $"Variable '{id.Nombre}' ya declarada en este scope.");
		 }
		 else if (scopeActual == "global")
		 {
		   string textoVar = id.EsArreglo
		     ? $"{id.Nombre}[{id.Tam}] : {tipo}"
		     : $"{id.Nombre} : {tipo}";
		
		   nodoVariables.Agregar(new NodoArbol(textoVar));
		 }
		}
		
	}

	void ListaIds(ListaIdsDeclarados ids) {
		IdConDimension(ids);
		while (la.kind == 8) {
			Get();
			IdConDimension(ids);
		}
	}

	void IdConDimension(ListaIdsDeclarados ids) {
		string nombre = ""; int tam = 0; bool esArr = false; 
		Expect(30);
		nombre = t.val; 
		if (la.kind == 5) {
			Get();
			Expect(28);
			tam = int.Parse(t.val); esArr = true; 
			Expect(6);
		}
		ids.Add(new IdDeclarado(nombre, esArr, tam));
		
	}

	void ListaSentencias() {
		while (StartOf(1)) {
			Sentencia();
		}
	}

	void Sentencia() {
		switch (la.kind) {
		case 30: {
			SentenciaIdent();
			break;
		}
		case 41: {
			SentenciaIf();
			break;
		}
		case 44: {
			SentenciaWhile();
			break;
		}
		case 46: {
			SentenciaFor();
			break;
		}
		case 47: {
			SentenciaWrite();
			break;
		}
		case 48: {
			SentenciaInput();
			break;
		}
		case 49: {
			SentenciaReturn();
			break;
		}
		default: SynErr(56); break;
		}
	}

	void SentenciaIdent() {
		string nombre = ""; string tipoExpr = "";
		string res = ""; string idx = "";
		string op = "";
		var info = (EntradaSimbolo)null; 
		Expect(30);
		nombre = t.val; 
		if (la.kind == 3) {
			Get();
			Expect(4);
			if (!tablaSimbolos.ExisteFuncion(nombre))
			 errores.Error(la.line, $"FunciÃ³n '{nombre}' no declarada.");
			
			generador.Agregar("CALL", nombre, "-", "-");
			nodoSentencias.Agregar(new NodoArbol("LlamadaFuncion"));
			
			Expect(7);
		} else if (la.kind == 5 || la.kind == 10) {
			if (la.kind == 5) {
				Get();
				Expresion(out idx, out tipoExpr);
				Expect(6);
			}
			Expect(10);
			Expresion(out res, out tipoExpr);
			info = tablaSimbolos.Buscar(nombre, scopeActual);
			
			if (info == null)
			 errores.Error(la.line, $"Variable '{nombre}' no declarada.");
			else if (info.Tipo != tipoExpr && tipoExpr != "int" && tipoExpr != "float")
			 errores.Error(la.line, $"Tipo incompatible: '{tipoExpr}' asignado a '{info.Tipo}'.");
			
			string destino = string.IsNullOrEmpty(idx) ? nombre : $"{nombre}[{idx}]";
			generador.Agregar(":=", res, "-", destino);
			
			nodoSentencias.Agregar(new NodoArbol("Asignacion"));
			
			if (info != null)
			 tablaSimbolos.SetValor(nombre, scopeActual, res);
			
			Expect(7);
		} else if (la.kind == 25 || la.kind == 26) {
			if (la.kind == 25) {
				Get();
				op = "+"; 
			} else {
				Get();
				op = "-"; 
			}
			info = tablaSimbolos.Buscar(nombre, scopeActual);
			
			if (info == null)
			 errores.Error(la.line, $"Variable '{nombre}' no declarada.");
			
			string tmp = NuevoTemp();
			generador.Agregar(op, nombre, "1", tmp);
			generador.Agregar(":=", tmp, "-", nombre);
			
			string operadorTexto = op == "+" ? "++" : "--";
			nodoSentencias.Agregar(new NodoArbol("IncrementoDecremento"));
			
			Expect(7);
		} else SynErr(57);
	}

	void SentenciaIf() {
		string cond = ""; string tipoCond = "";
		string etFalse = ""; string etFin = ""; 
		Expect(41);
		Expect(3);
		Expresion(out cond, out tipoCond);
		Expect(4);
		if (tipoCond != "bool")
		 errores.Error(la.line,
		   "La condiciÃ³n del IF debe ser booleana (resultado de comparaciÃ³n o lÃ³gica).");
		
		etFalse = generador.NuevaEtiqueta();
		etFin   = generador.NuevaEtiqueta();
		generador.Agregar("GOTOF", cond, "-", etFalse);
		
		nodoSentencias.Agregar(new NodoArbol("If"));
		
		Expect(42);
		Expect(1);
		ListaSentencias();
		Expect(2);
		generador.Agregar("GOTO", "-", "-", etFin);
		generador.AgregarEtiqueta(etFalse);
		
		if (la.kind == 43) {
			Get();
			Expect(1);
			ListaSentencias();
			Expect(2);
		}
		generador.AgregarEtiqueta(etFin);
		
	}

	void SentenciaWhile() {
		string cond = ""; string tipoCond = "";
		string etIni = ""; string etFin = ""; 
		etIni = generador.NuevaEtiqueta();
		generador.AgregarEtiqueta(etIni);
		
		Expect(44);
		Expect(3);
		Expresion(out cond, out tipoCond);
		Expect(4);
		if (tipoCond != "bool")
		 errores.Error(la.line, "La condiciÃ³n del WHILE debe ser booleana.");
		
		etFin = generador.NuevaEtiqueta();
		generador.Agregar("GOTOF", cond, "-", etFin);
		
		nodoSentencias.Agregar(new NodoArbol("While"));
		
		Expect(45);
		Expect(1);
		ListaSentencias();
		Expect(2);
		generador.Agregar("GOTO", "-", "-", etIni);
		generador.AgregarEtiqueta(etFin);
		
	}

	void SentenciaFor() {
		string nomVar = ""; string init = ""; string tipoInit = "";
		string cond = ""; string tipoCond = "";
		string etIni = ""; string etFin = ""; string opInc = "+"; 
		Expect(46);
		Expect(3);
		Expect(30);
		nomVar = t.val;
		var info = tablaSimbolos.Buscar(nomVar, scopeActual);
		if (info == null)
		 errores.Error(la.line, $"Variable '{nomVar}' no declarada."); 
		Expect(10);
		Expresion(out init, out tipoInit);
		generador.Agregar(":=", init, "-", nomVar);
		etIni = generador.NuevaEtiqueta();
		generador.AgregarEtiqueta(etIni);
		
		Expect(7);
		Expresion(out cond, out tipoCond);
		if (tipoCond != "bool")
		 errores.Error(la.line, "La condiciÃ³n del FOR debe ser booleana.");
		
		etFin = generador.NuevaEtiqueta();
		generador.Agregar("GOTOF", cond, "-", etFin);
		
		nodoSentencias.Agregar(new NodoArbol("For"));
		
		Expect(7);
		Expect(30);
		string varInc = t.val; 
		if (la.kind == 25) {
			Get();
			opInc = "+"; 
		} else if (la.kind == 26) {
			Get();
			opInc = "-"; 
		} else SynErr(58);
		Expect(4);
		Expect(1);
		ListaSentencias();
		Expect(2);
		string tmp = NuevoTemp();
		generador.Agregar(opInc, nomVar, "1", tmp);
		generador.Agregar(":=", tmp, "-", nomVar);
		generador.Agregar("GOTO", "-", "-", etIni);
		generador.AgregarEtiqueta(etFin);
		
	}

	void SentenciaWrite() {
		string val = ""; string tipo = ""; 
		Expect(47);
		Expect(3);
		Expresion(out val, out tipo);
		generador.Agregar("WRITE", val, "-", "-"); 
		nodoSentencias.Agregar(new NodoArbol("Write"));
		
		Expect(4);
		Expect(7);
	}

	void SentenciaInput() {
		string nomVar = ""; 
		Expect(48);
		Expect(3);
		Expect(30);
		nomVar = t.val;
		if (tablaSimbolos.Buscar(nomVar, scopeActual) == null)
		 errores.Error(la.line, $"Variable '{nomVar}' no declarada."); 
		Expect(4);
		Expect(7);
		generador.Agregar("INPUT", "-", "-", nomVar); 
		nodoSentencias.Agregar(new NodoArbol("Input"));
		
	}

	void SentenciaReturn() {
		string val = ""; string tipo = ""; 
		Expect(49);
		Expresion(out val, out tipo);
		Expect(7);
		var infoFunc = tablaSimbolos.BuscarFuncion(scopeActual);
		
		if (scopeActual == "global")
		{
		   errores.Error(la.line, "La sentencia return solo puede usarse dentro de una funciÃ³n.");
		}
		else if (infoFunc != null)
		{
		   if (infoFunc.TipoRetorno == "void")
		   {
		       errores.Error(la.line, $"La funciÃ³n '{scopeActual}' no debe retornar un valor porque es void.");
		   }
		   else if (infoFunc.TipoRetorno != tipo)
		   {
		       errores.Error(la.line, $"Tipo de retorno incompatible: se esperaba '{infoFunc.TipoRetorno}' y se obtuvo '{tipo}'.");
		   }
		}
		
		generador.Agregar("RETURN", val, "-", "-");
		nodoSentencias.Agregar(new NodoArbol("Return"));
		
	}

	void Expresion(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; res = ""; tipo = ""; 
		ExpresionAND(out res, out tipo);
		while (la.kind == 23 || la.kind == 50) {
			if (la.kind == 23) {
				Get();
			} else {
				Get();
			}
			ExpresionAND(out r2, out t2);
			if (tipo != "bool" || t2 != "bool")
			 errores.Error(la.line, "La operaciÃ³n OR requiere expresiones booleanas.");
			
			string tmp = NuevoTemp();
			generador.Agregar("||", res, r2, tmp);
			res = tmp;
			tipo = "bool";
			
		}
	}

	void ExpresionAND(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; res = ""; tipo = ""; 
		ExpresionIgualdad(out res, out tipo);
		while (la.kind == 22 || la.kind == 51) {
			if (la.kind == 22) {
				Get();
			} else {
				Get();
			}
			ExpresionIgualdad(out r2, out t2);
			if (tipo != "bool" || t2 != "bool")
			 errores.Error(la.line, "La operaciÃ³n AND requiere expresiones booleanas.");
			
			string tmp = NuevoTemp();
			generador.Agregar("&&", res, r2, tmp);
			res = tmp;
			tipo = "bool";
			
		}
	}

	void ExpresionIgualdad(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; string op = ""; res = ""; tipo = ""; 
		ExpresionRelacional(out res, out tipo);
		while (la.kind == 16 || la.kind == 17) {
			if (la.kind == 16) {
				Get();
				op = "=="; 
			} else {
				Get();
				op = "!="; 
			}
			ExpresionRelacional(out r2, out t2);
			string tmp = NuevoTemp();
			generador.Agregar(op, res, r2, tmp);
			res = tmp;
			tipo = "bool";
			
		}
	}

	void ExpresionRelacional(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; string op = ""; res = ""; tipo = ""; 
		ExpresionAditiva(out res, out tipo);
		while (StartOf(2)) {
			if (la.kind == 20) {
				Get();
				op = "<";  
			} else if (la.kind == 21) {
				Get();
				op = ">";  
			} else if (la.kind == 18) {
				Get();
				op = "<="; 
			} else {
				Get();
				op = ">="; 
			}
			ExpresionAditiva(out r2, out t2);
			string tmp = NuevoTemp();
			generador.Agregar(op, res, r2, tmp);
			res = tmp;
			tipo = "bool";
			
		}
	}

	void ExpresionAditiva(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; string op = ""; res = ""; tipo = ""; 
		ExpresionMultiplicativa(out res, out tipo);
		while (la.kind == 11 || la.kind == 12) {
			if (la.kind == 11) {
				Get();
				op = "+"; 
			} else {
				Get();
				op = "-"; 
			}
			ExpresionMultiplicativa(out r2, out t2);
			string tmp = NuevoTemp();
			generador.Agregar(op, res, r2, tmp);
			res = tmp;
			tipo = (tipo == "float" || t2 == "float") ? "float" : "int";
			
		}
	}

	void ExpresionMultiplicativa(out string res, out string tipo) {
		string r2 = ""; string t2 = ""; string op = ""; res = ""; tipo = ""; 
		ExpresionUnaria(out res, out tipo);
		while (la.kind == 13 || la.kind == 14 || la.kind == 15) {
			if (la.kind == 13) {
				Get();
				op = "*"; 
			} else if (la.kind == 14) {
				Get();
				op = "/"; 
			} else {
				Get();
				op = "%"; 
			}
			ExpresionUnaria(out r2, out t2);
			string tmp = NuevoTemp();
			generador.Agregar(op, res, r2, tmp);
			res = tmp;
			tipo = (tipo == "float" || t2 == "float") ? "float" : "int";
			
		}
	}

	void ExpresionUnaria(out string res, out string tipo) {
		res = ""; tipo = ""; 
		if (la.kind == 24) {
			Get();
			ExpresionUnaria(out res, out tipo);
			if (tipo != "bool")
			 errores.Error(la.line, "El operador '!' requiere expresiÃ³n booleana.");
			
			string tmp = NuevoTemp();
			generador.Agregar("!", res, "-", tmp);
			res = tmp;
			tipo = "bool";
			
		} else if (la.kind == 12) {
			Get();
			ExpresionUnaria(out res, out tipo);
			string tmp = NuevoTemp();
			generador.Agregar("NEG", res, "-", tmp);
			res = tmp;
			
		} else if (StartOf(3)) {
			Atomo(out res, out tipo);
		} else SynErr(59);
	}

	void Atomo(out string res, out string tipo) {
		res = ""; tipo = "";
		string idx = ""; string tIdx = "";
		ListaCadenas args = new ListaCadenas();
		ListaCadenas tiposArgs = new ListaCadenas(); 
		switch (la.kind) {
		case 28: {
			Get();
			res = t.val; tipo = "int"; 
			break;
		}
		case 27: {
			Get();
			res = t.val; tipo = "float"; 
			break;
		}
		case 52: {
			Get();
			res = "true"; tipo = "bool"; 
			break;
		}
		case 53: {
			Get();
			res = "false"; tipo = "bool"; 
			break;
		}
		case 29: {
			Get();
			res = t.val; tipo = "char"; 
			break;
		}
		case 30: {
			Get();
			string nom = t.val;
			var info = tablaSimbolos.Buscar(nom, scopeActual);
			
			if (la.kind == 3) {
				Get();
				ArgumentosFuncion(args, tiposArgs);
				Expect(4);
				var infoFunc = tablaSimbolos.BuscarFuncion(nom);
				
				if (infoFunc == null)
				 errores.Error(la.line, $"FunciÃ³n '{nom}' no declarada.");
				
				string tmp = NuevoTemp();
				
				foreach (var a in args)
				 generador.Agregar("PARAM", a, "-", "-");
				
				generador.Agregar("CALL", nom, args.Count.ToString(), tmp);
				
				res = tmp;
				tipo = infoFunc?.TipoRetorno ?? "int";
				
			} else if (la.kind == 5) {
				Get();
				Expresion(out idx, out tIdx);
				Expect(6);
				if (info == null)
				 errores.Error(la.line, $"Arreglo '{nom}' no declarado.");
				
				res = $"{nom}[{idx}]";
				tipo = info?.Tipo ?? "int";
				
			} else if (StartOf(4)) {
				if (info == null)
				 errores.Error(la.line, $"Variable '{nom}' no declarada.");
				
				res = nom;
				tipo = info?.Tipo ?? "int";
				
			} else SynErr(60);
			break;
		}
		case 3: {
			Get();
			Expresion(out res, out tipo);
			Expect(4);
			break;
		}
		default: SynErr(61); break;
		}
	}

	void ArgumentosFuncion(ListaCadenas args, ListaCadenas tipos) {
		string r = ""; string t = ""; 
		if (StartOf(5)) {
			Expresion(out r, out t);
			args.Add(r); tipos.Add(t); 
			while (la.kind == 8) {
				Get();
				Expresion(out r, out t);
				args.Add(r); tipos.Add(t); 
			}
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		MiLenguaje();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _T,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x},
		{_x,_x,_x,_x, _T,_x,_T,_T, _T,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x},
		{_x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "lbrace expected"; break;
			case 2: s = "rbrace expected"; break;
			case 3: s = "lparen expected"; break;
			case 4: s = "rparen expected"; break;
			case 5: s = "lbracket expected"; break;
			case 6: s = "rbracket expected"; break;
			case 7: s = "semicolon expected"; break;
			case 8: s = "comma expected"; break;
			case 9: s = "colon expected"; break;
			case 10: s = "assign expected"; break;
			case 11: s = "plus expected"; break;
			case 12: s = "minus expected"; break;
			case 13: s = "times expected"; break;
			case 14: s = "slash expected"; break;
			case 15: s = "percent expected"; break;
			case 16: s = "eq expected"; break;
			case 17: s = "neq expected"; break;
			case 18: s = "le expected"; break;
			case 19: s = "ge expected"; break;
			case 20: s = "lt expected"; break;
			case 21: s = "gt expected"; break;
			case 22: s = "andOp expected"; break;
			case 23: s = "orOp expected"; break;
			case 24: s = "notOp expected"; break;
			case 25: s = "inc expected"; break;
			case 26: s = "dec expected"; break;
			case 27: s = "flotante expected"; break;
			case 28: s = "entero expected"; break;
			case 29: s = "cadena expected"; break;
			case 30: s = "ident expected"; break;
			case 31: s = "\"program\" expected"; break;
			case 32: s = "\"main\" expected"; break;
			case 33: s = "\"function\" expected"; break;
			case 34: s = "\"var\" expected"; break;
			case 35: s = "\"int\" expected"; break;
			case 36: s = "\"float\" expected"; break;
			case 37: s = "\"char\" expected"; break;
			case 38: s = "\"bool\" expected"; break;
			case 39: s = "\"begin\" expected"; break;
			case 40: s = "\"end\" expected"; break;
			case 41: s = "\"if\" expected"; break;
			case 42: s = "\"then\" expected"; break;
			case 43: s = "\"else\" expected"; break;
			case 44: s = "\"while\" expected"; break;
			case 45: s = "\"do\" expected"; break;
			case 46: s = "\"for\" expected"; break;
			case 47: s = "\"write\" expected"; break;
			case 48: s = "\"input\" expected"; break;
			case 49: s = "\"return\" expected"; break;
			case 50: s = "\"or\" expected"; break;
			case 51: s = "\"and\" expected"; break;
			case 52: s = "\"true\" expected"; break;
			case 53: s = "\"false\" expected"; break;
			case 54: s = "??? expected"; break;
			case 55: s = "invalid Tipo"; break;
			case 56: s = "invalid Sentencia"; break;
			case 57: s = "invalid SentenciaIdent"; break;
			case 58: s = "invalid SentenciaFor"; break;
			case 59: s = "invalid ExpresionUnaria"; break;
			case 60: s = "invalid Atomo"; break;
			case 61: s = "invalid Atomo"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}