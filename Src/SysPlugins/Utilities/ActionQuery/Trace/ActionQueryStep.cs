using Ccf.Ck.Libs.ActionQuery;
using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public struct ActionQueryStep
    {
        public int Pc;
        public Instruction Instruction;
        public List<ParameterResolverValue> Arguments;
        public List<ParameterResolverValue> Stack;
        public List<SymbolSet> Symbols;
        public ActionQueryStep( int pc, 
                                Instruction instruction, 
                                ParameterResolverValue[] arguments, 
                                IEnumerable<ParameterResolverValue> stack,
                                IEnumerable<SymbolSet> symbols
                                )
        {
            Instruction = instruction;
            Pc = pc;
            Arguments = arguments.ToList();
            Stack = stack.ToList();
            Symbols = symbols.ToList();
        }

        public override string ToString()
        {
            return $"{Pc}: {Instruction} -----------------------\n" +
                   $"\tArguments:{String.Join(',',Arguments)}\n" +
                   $"\tStack: \n{String.Join("\t\n", Stack)}\n" +
                   $"\tSymbols: \n{String.Join("\t\n", Symbols)}\n" +
                   "-------------------------";
        }
    }
}
