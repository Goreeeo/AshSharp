using Ash.Lexing;
using System;
using System.Collections.Generic;
using System.IO;

namespace Ash
{
    public class RuntimeError : Exception
    {
        public Token token;
        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }

    public class AshCompiler
    {
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        private static bool printAst = false;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                Console.WriteLine("Usage: ash [file]");
                Environment.Exit(64);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{error.Message}\n[line {error.token.line}]");
            Console.ForegroundColor = ConsoleColor.White;
            hadRuntimeError = true;
        }

        public static Exception Error(Token token, string message)
        {
            if (token.type == TokenType.EoF)
            {
                Report(token.line, "at end", message);
            }
            else
            {
                Report(token.line, $"at '{token.lexeme}'", message);
            }

            return new Exception();
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[line {line}] Error{(where == string.Empty ? "" : $" {where}")}: {message}");
            Console.ForegroundColor = ConsoleColor.White;
            hadError = true;
        }

        private static void RunFile(string path)
        {
            string code = File.ReadAllText(path);
            Run(code);

            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);
        }

        private static void Run(string code)
        {
            List<Token> tokens = new Lexer(code).TokenizeAll();

            if (hadError) return;

            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }
}
