﻿using System;
using NiL.JS.Core;

namespace NiL.JS.Expressions
{
    [Serializable]
    public sealed class LessOrEqual : More
    {
        protected internal override PredictedType ResultType
        {
            get
            {
                return PredictedType.Bool;
            }
        }

        public LessOrEqual(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override JSObject Evaluate(Context context)
        {
            return base.Evaluate(context).iValue == 0;
        }

        public override string ToString()
        {
            return "(" + first + " <= " + second + ")";
        }
    }
}