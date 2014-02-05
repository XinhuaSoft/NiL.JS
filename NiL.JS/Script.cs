﻿using NiL.JS.Core;
using NiL.JS.Statements;
using NiL.JS.Core.BaseTypes;
using System;
using System.Threading;

namespace NiL.JS
{
    [Serializable]
    public sealed class Script
    {
        private Statement root;

        public string Code { get; private set; }
        public Context Context { get; private set; }

        public Script(string code)
        {
            Context = new Context(NiL.JS.Core.Context.globalContext);
            Code = code;
            int i = 0;
            string c = "{" + Tools.RemoveComments(Code) + "}";
            root = CodeBlock.Parse(new ParsingState(c), ref i).Statement;
            if (i != c.Length)
                throw new System.ArgumentException("Invalid char");
            Parser.Optimize(ref root, new System.Collections.Generic.Dictionary<string, Statement>());
        }

        public void Invoke()
        {
            var lm = System.Runtime.GCSettings.LatencyMode;
            System.Runtime.GCSettings.LatencyMode = System.Runtime.GCLatencyMode.Interactive;
            try
            {
                Context.ValidateThreadID();
                root.Invoke(Context);
            }
            finally
            {
                System.Runtime.GCSettings.LatencyMode = lm;
                Context.currentRootContext = null;
            }
        }
    }
}