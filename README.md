# Good for Nothing Compiler in C&#35;
This is a continuation of the old Good for Nothing Compiler presented by Joel Pobar and Joe Duffy at PDC back in 2005! 
The presentation was liked and the article and code posted at https://msdn.microsoft.com/en-us/magazine/cc136756.aspx
has been read and used by many programmers who wanted to learn more about Scanners, Parsers and how to write a somple
compiler in C#.

The code compiles a simple c-like language called "Good for nothing" and has support for variables, simple
inputs/outputs and a for-loop.

The article by the gentlemen mentioned above contains a Language Definition using a metasyntax called EBNF 
(Extended Backus-Naur Form), that should support parsing and compiling of arithmetic expressions, but the 
code doesn't, so I thought I would try and add support for that and learn something along the way. 

**Feel free to spice the code up!**

##Language Specification##
This is the language specification defined in a simple EBNF style:

```
<stmt> := var <ident> = <expr>
	| <ident> = <expr>
	| for <ident> = <expr> to <expr> do <stmt> end
	| read_int <ident>
	| print <expr>
	| <stmt> ; <stmt>

<expr> := <string>
	| <int>
	| <arith_expr>
	| <ident>

<arith_expr> := <expr> <arith_op> <expr>
<arith_op> := + | - | * | /

<ident> := <char> <ident_rest>*
<ident_rest> := <char> | <digit>

<int> := <digit>+
<digit> := 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9

<string> := " <string_elem>* "
<string_elem> := <any char other than ">
```


