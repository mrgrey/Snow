using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Cleancode.Snow
{
    abstract class UnsupportedActionException : Exception
    {
        public enum ConflictType
        {
            Unknown,
            ReturnType,
            ParamType
        }

        abstract public ConflictType Conflict
        { get; }
        public MethodInfo ActionMethod
        { get { return _actionMethod; } }
        protected MethodInfo _actionMethod;

        private UnsupportedActionException() { }
        protected UnsupportedActionException(string message, MethodInfo actionMethod)
            : base(message) { _actionMethod = actionMethod; }
    }

    class UnsupportedActionReturnTypeException : UnsupportedActionException
    {
        public override UnsupportedActionException.ConflictType Conflict
        {
            get { return ConflictType.ReturnType; }
        }

        public UnsupportedActionReturnTypeException(MethodInfo actionMethod)
            : base(String.Format("Unsupported action({0}) return type: ", actionMethod.Name, actionMethod.ReturnType), actionMethod)
        { }
    }

    class UnsupportedActionParamException : UnsupportedActionException
    {

        public override UnsupportedActionException.ConflictType Conflict
        {
            get { return ConflictType.ParamType; }
        }

        private ParameterInfo _actionMethodParam;
        public ParameterInfo ActionMethodParam
        { get { return _actionMethodParam; } }

        public UnsupportedActionParamException(MethodInfo actionMethod, ParameterInfo actionMethodParam)
            : base(String.Format("Unsupported action({0}) param({1}) type: {2} ", actionMethod.ReflectedType, actionMethod.Name, actionMethodParam.Name, actionMethodParam.ParameterType), actionMethod)
        {
            _actionMethodParam = actionMethodParam;
        }

    }
}
