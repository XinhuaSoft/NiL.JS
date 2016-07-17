﻿using System;
using System.Collections.Generic;
using NiL.JS.Core;
using NiL.JS.Expressions;

namespace NiL.JS.Statements
{
#if !(PORTABLE || NETCORE)
    [Serializable]
#endif
    public sealed class LabeledStatement : CodeNode
    {
        private CodeNode statement;
        private string label;

        public CodeNode Statement { get { return statement; } }
        public string Label { get { return label; } }

        internal static CodeNode Parse(ParseInfo state, ref int index)
        {
            int i = index;
            if (!Parser.ValidateName(state.Code, ref i, state.strict))
                return null;
            int l = i;
            if (i >= state.Code.Length || (!Parser.Validate(state.Code, " :", ref i) && state.Code[i++] != ':'))
                return null;

            var label = state.Code.Substring(index, l - index);
            state.Labels.Add(label);
            int oldlc = state.LabelsCount;
            state.LabelsCount++;
            var stat = Parser.Parse(state, ref i, 0);
            state.Labels.Remove(label);
            state.LabelsCount = oldlc;
            if (stat is FunctionDefinition)
            {
                if (state.message != null)
                    state.message(MessageLevel.CriticalWarning, CodeCoordinates.FromTextPosition(state.Code, stat.Position, stat.Length), "Labeled function. Are you sure?");
            }
            var pos = index;
            index = i;
            return new LabeledStatement()
            {
                statement = stat,
                label = label,
                Position = pos,
                Length = index - pos
            };
        }

        public override JSValue Evaluate(Context context)
        {
            var res = statement.Evaluate(context);
            if ((context.executionMode == AbortReason.Break) && (context.executionInfo != null) && (context.executionInfo.oValue as string == label))
            {
                context.executionMode = AbortReason.None;
                context.executionInfo = JSValue.notExists;
            }
            return res;
        }

        protected internal override CodeNode[] getChildsImpl()
        {
            return new[] { statement };
        }

        public override bool Build(ref CodeNode _this, int expressionDepth, Dictionary<string, VariableDescriptor> variables, CodeContext codeContext, CompilerMessageCallback message, FunctionInfo stats, Options opts)
        {
            Parser.Build(ref statement, expressionDepth, variables, codeContext, message, stats, opts);
            if (statement == null)
            {
                _this = null;
            }

            return false;
        }

        public override void Optimize(ref CodeNode _this, Expressions.FunctionDefinition owner, CompilerMessageCallback message, Options opts, FunctionInfo stats)
        {
            statement.Optimize(ref statement, owner, message, opts, stats);
            if (statement == null)
            {
                _this = null;
            }
        }

        public override T Visit<T>(Visitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override string ToString()
        {
            return label + ": " + statement;
        }

        public override void Decompose(ref CodeNode self)
        {
            statement.Decompose(ref statement);
        }

        public override void RebuildScope(FunctionInfo functionInfo, Dictionary<string, VariableDescriptor> transferedVariables, int scopeBias)
        {
            statement.RebuildScope(functionInfo, transferedVariables, scopeBias);
        }
    }
}