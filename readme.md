# Mermaid for .net
How many times, you open an `old big` project and you have to search from scratch what is going on ?  

This project comes to give an end to this mess, point the assembly then you got a flowchart diagram that `visualizes` each method dependencies (what methods uses). The existing `solutions` do provide such functionality, but miss the simplicity. Yeah, talking about :  
* [Zen.ndepend](https://www.ndepend.com/docs/class-dependency-diagram) - needed user interaction, clicks-clicks-clicks...
* [Fatesoft.Program Flow chart Generator](https://www.fatesoft.com/) - codeflow representation, accepts only 1 source code / graph.
* vscode - [kr.Code to Flowchart](https://github.com/karthyick/code2flowchart) - no so accurate on csharp ([sample](https://i.imgur.com/09ZkdHO.jpeg))  

![Image](https://github.com/user-attachments/assets/26ccdfef-c407-4474-bcb6-2143bbb074dd)

![Image](https://github.com/user-attachments/assets/f2f8deec-1c63-4c0d-aeba-c7b70b43d3e1)  

Without [mermaid](https://mermaid.js.org/) could not be possible!  

This project uses : 
* [Mono.Cecil](https://github.com/jbevain/cecil) - to read assembly entities
* [mermaidJS](https://mermaid.js.org/) - to display the flowchart   

> [!TIP]  
> draw.io - can import mermaid - use "Arrange" > "Insert" > "Advanced" > "Mermaid"  

references : 
* <https://github.com/samsmithnz/MermaidDotNet> - .NET `wrapper` to create Mermaid
* <https://github.com/jespervandijk/mermaid-class-diagram-generator> - `UML`
* <https://github.com/itn3000/Cs2Mermaid> - .NET `syntax tree` to mermaid diagram
    * [antlr](https://www.antlr.org/)
    * stackoverflow - [AST with sharpdevelop](https://stackoverflow.com/a/4553345)
    * stackoverflow - [CSharpSyntaxTree](https://stackoverflow.com/a/44043018)  

## This project is no longer maintained
Copyright (c) 2025 [PipisCrew](http://pipiscrew.com)  
Licensed under the [MIT license](http://www.opensource.org/licenses/mit-license.php)