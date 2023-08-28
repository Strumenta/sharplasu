cd ./antlr4
java -cp ../../antlr-4.13.0-complete.jar org.antlr.v4.Tool -Dlanguage=CSharp -o ../Generated SimpleLangLexer.g4 SimpleLangParser.g4
cd ..
