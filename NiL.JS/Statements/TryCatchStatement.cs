﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.JS.Core;
using NiL.JS.Core.BaseTypes;
using NiL.JS.Statements;

namespace NiL.JS.Statements
{
    internal class TryCatchStatement : Statement, IOptimizable
    {
        private Statement body;
        private Statement catchBody;
        private string exptName;

        public TryCatchStatement()
        {
        }

        public static ParseResult Parse(string code, ref int index)
        {
            int i = index;
            if (!Parser.Validate(code, "try", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            var b = CodeBlock.Parse(code, ref i).Statement;
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, "catch (", ref i))
                throw new ArgumentException("code (" + i + ")");
            int s = i;
            if (!Parser.ValidateName(code, ref i, true))
                throw new ArgumentException("code (" + i + ")");
            string exptn = code.Substring(s, i - s);
            while (char.IsWhiteSpace(code[i])) i++;
            if (!Parser.Validate(code, ")", ref i))
                throw new ArgumentException("code (" + i + ")");
            while (char.IsWhiteSpace(code[i])) i++;
            var cb = CodeBlock.Parse(code, ref i).Statement;
            index = i;
            return new ParseResult()
            {
                IsParsed = true,
                Message = "",
                Statement = new TryCatchStatement()
                {
                    body = b,
                    catchBody = cb,
                    exptName = exptn
                }
            };
        }

        public override IContextStatement Implement(Context context)
        {
            return new ContextStatement(context, this);
        }

        public override JSObject Invoke(Context context)
        {
            try
            {
                body.Invoke(context);
            }
            catch(Exception e)
            {
                var eo = context.Define(exptName);
                eo.ValueType = ObjectValueType.Object;
                eo.GetField("message").Assign(e.Message);
                catchBody.Invoke(context);
            }
            return null;
        }

        public override JSObject Invoke(Context context, JSObject _this, IContextStatement[] args)
        {
            throw new NotImplementedException();
        }

        public bool Optimize(ref Statement _this, int depth, System.Collections.Generic.HashSet<string> varibles)
        {
            Parser.Optimize(ref body, depth + 1, varibles);
            Parser.Optimize(ref catchBody, depth + 1, varibles);
            return false;
        }
    }
}
