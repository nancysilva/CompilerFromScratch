# MiniLang Compiler — Compiler for a Custom Programming Language

A compiler developed in **C#** using **Coco/R** for **MiniLang**, a custom-designed programming language. The compiler performs lexical, syntactic, and semantic analysis, generates symbol tables and syntax trees, and executes intermediate code.

This project was developed as part of the **Compiler Design** course at **Tecnológico de Monterrey**, where the programming language, grammar, and compiler were designed and implemented from scratch.

---

## Features

- Lexical analysis using Coco/R Scanner
- Syntax analysis using Coco/R Parser
- Semantic analysis with error detection
- Symbol table generation
- Syntax tree visualization
- Intermediate code generation
- Intermediate code execution
- Detailed lexical, syntactic, and semantic error reporting

### Supported Language Features

- Variable declarations
- Arithmetic expressions
- Conditional statements (`if`)
- Iterative structures (`while` and `for`)
- User-defined functions
- Matrix declaration
- Matrix multiplication
- Expression evaluation

---

## Technologies

- **C#**
- **.NET**
- **Coco/R**
- Compiler Construction
- Parsing Techniques
- Semantic Analysis

---

## Compiler Pipeline
```
Source Code
      │
      ▼
Lexical Analysis
      │
      ▼
Syntax Analysis
      │
      ▼
Semantic Analysis
      │
      ▼
Symbol Table
      │
      ▼
Syntax Tree
      │
      ▼
Intermediate Code Generation
      │
      ▼
Execution
```

---

## Project Structure

```
CompiladorCocoR/
│
├── Scanner.cs
├── Parser.cs
├── Program.cs
├── Errors.cs
├── SymbolTable.cs
├── CodeGenerator.cs
├── Grammar.atg
└── TestPrograms/
```

*(The exact structure may vary depending on your implementation.)*

---

## Build

Requires the [.NET SDK](https://dotnet.microsoft.com/download) installed.

```bash
dotnet build
```

---

## Usage

Compile a source file:

```bash
dotnet CompiladorCocoR.dll program.txt
```

Display only the generated tokens:

```bash
dotnet CompiladorCocoR.dll --tokens program.txt
```

---

## Example

```text
int a = 10;
int b = 20;
function sum(x, y){
    return x + y;
}
if (a < b){
    print(sum(a,b));
}
for(i = 0; i < 5; i++){
    print(i);
}
```

---

## Output

The compiler is capable of producing:

- Token list
- Lexical error report
- Syntax error report
- Semantic error report
- Symbol table
- Syntax tree
- Intermediate code
- Program execution

---

## Learning Outcomes

Through this project we implemented the fundamental phases of compiler construction, including:

- Lexical analysis
- Parsing
- Semantic checking
- Symbol table management
- Syntax tree construction
- Intermediate code generation
- Error handling and reporting

---

## Authors

- **Nancy Silva Alvarez**
- **Carolina de los Santos**

Developed for the Compiler Design course at **Tecnológico de Monterrey**.

---

## License

This project was developed for educational purposes.
