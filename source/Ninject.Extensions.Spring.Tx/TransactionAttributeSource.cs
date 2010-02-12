using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Spring.Transaction.Interceptor;

namespace Ninject.Extensions.Spring.Tx
{
    /// <summary>
    /// Finds and converts <see cref="ITransactionAttribute"/> to <see cref="Ninject"/>
    /// </summary>
    public class TransactionAttributeSource : AbstractFallbackTransactionAttributeSource
    {
        protected override Attribute[] FindAllAttributes(MethodInfo method)
        {
            return Attribute.GetCustomAttributes(method);
        }

        protected override Attribute[] FindAllAttributes(Type targetType)
        {
            return Attribute.GetCustomAttributes(targetType);
        }

        protected override ITransactionAttribute FindTransactionAttribute(Attribute[] attributes)
        {
            if (attributes == null)
            {
                return null;
            }

            var attribute = attributes.FirstOrDefault(attr => attr is TransactionAttribute) as TransactionAttribute;

            if(attribute == null)
            {
                return null;
            }

            var ruleBasedTransactionAttribute = new RuleBasedTransactionAttribute
                                                    {
                                                        PropagationBehavior = attribute.TransactionPropagation,
                                                        TransactionIsolationLevel = attribute.IsolationLevel,
                                                        ReadOnly = attribute.ReadOnly,
                                                        EnterpriseServicesInteropOption =
                                                            attribute.EnterpriseServicesInteropOption
                                                    };

            IEnumerable<Attribute> rollBackFor = attribute.RollbackFor.Select(type => new RollbackRuleAttribute(type) as Attribute);
            IEnumerable<Attribute> noRollBackFor = attribute.NoRollbackFor.Select(type => new NoRollbackRuleAttribute(type) as Attribute);
            IList<Attribute> rollBackRules = rollBackFor.Union(noRollBackFor).ToList();                   
            
            ruleBasedTransactionAttribute.RollbackRules = rollBackRules.ToList();

            return ruleBasedTransactionAttribute;
        }
    }
}