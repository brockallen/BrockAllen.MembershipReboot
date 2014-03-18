namespace BrockAllen.MembershipReboot.Nh.Extensions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using NHibernate.Hql.Ast;
    using NHibernate.Linq;
    using NHibernate.Linq.Functions;
    using NHibernate.Linq.Visitors;

    public class StringComparisonEqualsGenerator : BaseHqlGeneratorForMethod
    {
        public StringComparisonEqualsGenerator()
        {
            SupportedMethods = new[]
                                   {
                                       ReflectionHelper.GetMethodDefinition<string>(
                                           x => x.Equals(null, StringComparison.CurrentCulture))
                                   };
        }

        public override HqlTreeNode BuildHql(
            MethodInfo method,
            Expression targetObject,
            ReadOnlyCollection<Expression> arguments,
            HqlTreeBuilder treeBuilder,
            IHqlExpressionVisitor visitor)
        {
            // Get the StringComparison argument.
            var comparison = (StringComparison)(arguments[1].As<ConstantExpression>().Value);

            if (comparison == StringComparison.CurrentCultureIgnoreCase
                || comparison == StringComparison.InvariantCultureIgnoreCase
                || comparison == StringComparison.OrdinalIgnoreCase)
            {
                // If the comparison calls for us to ignore the case, use SQL LOWER()
                return
                    treeBuilder.Equality(
                        treeBuilder.MethodCall("lower", new[] { visitor.Visit(targetObject).AsExpression() }),
                        treeBuilder.MethodCall("lower", new[] { visitor.Visit(arguments[0]).AsExpression() }));
            }

            // Otherwise use the database's default string comparison mechanism.
            return treeBuilder.Equality(
                visitor.Visit(targetObject).AsExpression(),
                visitor.Visit(arguments[0]).AsExpression());
        }
    }
}