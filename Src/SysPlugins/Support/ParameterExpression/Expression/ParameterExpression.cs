using System;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.Managers;

namespace Ccf.Ck.SysPlugins.Support.ParameterExpression.Expression
{
    public class ParameterExpression : ResolverExpression<ParameterResolverValue, IParameterResolverContext>
    {

        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> GetResolver(string name)
        {
            return ParameterResolversManager.Instance.GetResolver(name);
        }


        #region Literal values in the expression
        /// <summary>
        /// Generates instruction pushing False and True literals.
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushBool(bool Value)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(Value,EValueDataType.Boolean), 0, string.Format("pushbool[{0}]", Value));
        }
        /// <summary>
        /// Generate instruction pushing double numeric literals.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushDouble(double v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v, EValueDataType.Real, EValueDataSize.Normal), 0, string.Format("pushdouble[{0}]", v));
        }
        /// <summary>
        /// Generates instruction that pushes integer literals.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushInt(int v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v, EValueDataType.Int, EValueDataSize.Normal), 0, string.Format("pushint[{0}]", v));
        }
        /// <summary>
        /// Generates instruction pushing string literals
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushString(string v)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(v, EValueDataType.Text, EValueDataSize.Normal), 0, string.Format("pushstring[{0}]", v));
        }
        /// <summary>
        /// Generates instruction pushing null literals
        /// </summary>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushNull()
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => new ParameterResolverValue(null,EValueDataType.any), 0, "pushnull");
        }
        #endregion

        #region Special values
        /// <summary>
        /// Generates instruction pushing the 'name' special value.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Generates instruction pushing the 'Value' special value
        /// </summary>
        /// <returns></returns>
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

        #region Recursive call to other expressions (other params)
        /// <summary>
        /// Generates instruction that invokes calculation of another parameter by name and pushes the scalar result.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> PushParam(string name)
        {
            return new ResolverDelegate<ParameterResolverValue, IParameterResolverContext>((c, args) => {
                var ra = args as ResolverArguments<ParameterResolverValue>;
                if (ra != null)
                {
                    if (ra.Recursions > 2)
                    {
                        throw new Exception("Too many recursions - only up to 2 are allowed");
                    }
                }
                return c.Evaluate(name, args); // TODO: What exactly this would like
            }, 0, string.Format("pushparam[{0}]", name));
        }
        #endregion


        #region Validation (not needed in this case)

        protected override ResolverDelegate<ParameterResolverValue, IParameterResolverContext> ValidationChecker()
        {
            throw new NotImplementedException("Parameter expressions do not support validation oriented usage.");
        }
        #endregion
    }
}
