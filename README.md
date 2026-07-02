# CompilerFromScratch
 hand-built compiler project featuring custom grammar design, lexical/syntax analysis with Coco/R, and a full test suite.


# Compiler for a Custom Programming Language

A compiler developed in **C#** using **Coco/R** for a custom-designed programming language. The compiler performs lexical, syntactic, and semantic analysis, generates symbol tables and syntax trees, and executes intermediate code.

This project was developed as part of a **Compiler Design** course, where the programming language, grammar, and compiler were designed and implemented from scratch.

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

## Usage

Compile a source file:

```bash
CompiladorCocoR.exe program.txt
```

Display only the generated tokens:

```bash
CompiladorCocoR.exe --tokens program.txt
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

- **Nancy Silva Álvarez**
- **Carolina de los Santos**

---

## License

This project was developed for educational purposes.
