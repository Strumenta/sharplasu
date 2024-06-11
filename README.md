# Sharplasu - AST Library for .NET Standard

Sharplasu supplies the infrastructure to build a custom, possibly mutable, Abstract Syntax Tree (AST) using CSharp.

Sharplasu is part of the [StarLasu](https://starlasu.strumenta.com/) family of libraries. The other libraries provide similar support in other languages. Sharplasu it is at an earlier stage of development compared to other StarLasu libraries.

As of version 0.9, we have ported a basic implementation of the most used features from Kolasu, including support for transformers and symbol resolution. These features have been ported with the objective of supporting building parsers up to our standard. Code generation support is still missing, therefore we do not have good support yet for building transpilers using Sharplasu. 

The original Kolasu API for transformers is well suited for Kotlin, but it is not a perfect fit for C#. The two languages have strong similarities, but C# is more stringent when handling the inheritance of classes using Generics and it is less flexible in general when working with generics. For example, you can use wildcard generics in Kotlin, but not in C#. So, it should be considered in beta status and subject to improvements. You can participate to the discussion in the [issues](https://github.com/Strumenta/sharplasu/issues/12).

Sharplasu is integrated with ANTLR.

## Tests

To run the included tests you need to have ANTLR installed to compile an example parser. You do not need to have the ANTLR tool installed to run the library. You can read the official documentation or [our tutorial](https://tomassetti.me/antlr-mega-tutorial/#chapter11) to learn how to install ANTLR.

## Tutorial

You can read our tutorial on how to use SharpLasu: [Building advanced parsers using Sharplasu](https://tomassetti.me/building-advanced-parsers-using-sharplasu/). In the tutorial we are going to build a parser for Python 3, but the information can applied to parse any language.