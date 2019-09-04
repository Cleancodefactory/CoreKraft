using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using System;

namespace Ccf.Ck.SysPlugins.Support.ParameterValidation
{
    public class ValidationExpression : ResolverExpression<ParameterResolverValue, IParameterResolverContext>
    {
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver(string name)
        {
            throw new NotImplementedException();
        }

        #region Literal values in the expression
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushBool(bool Value)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(Value), 0, string.Format("pushbool[{0}]", Value));
        }

        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushDouble(double v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v), 0, string.Format("pushdouble[{0}]", v));
        }

        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushInt(int v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v), 0, string.Format("pushint[{0}]", v));
        }
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushString(string v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v), 0, string.Format("pushstring[{0}]", v));
        }
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushNull()
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(null), 0, "pushnull");
        }
        #endregion

        #region Special values
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushName()
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(
                (c, args) => {
                    var rargs = args as ResolverArguments<ParameterResolverValue>;
                    return rargs.Name;
                },
                0,
                "pushname"
            );
        }
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushValue()
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(
                (c, args) => {
                    var rargs = args as ResolverArguments<ParameterResolverValue>;
                    return rargs.Value;
                },
                0,
                "pushvalue"
            );
        }
        #endregion

        
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushParam(string name)
        {
            throw new NotImplementedException();
        }

        

        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> ValidationChecker()
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>(
                (c, args) => {
                    var ra = args as ResolverArguments<ParameterResolverValue>;
                    ParameterResolverValue valueToValidate = args[0];
                    if (valueToValidate.Value is bool && !((bool)valueToValidate.Value))
                    {
                        ra.StopExecution = true;
                    }
                    return valueToValidate;
                },
                1, 
                string.Format("validationcheck"));
        }
    }
}
