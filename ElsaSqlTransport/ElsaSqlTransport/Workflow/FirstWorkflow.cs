using Elsa.Builders;
using Elsa.Models;
using System.Collections.Generic;
using System.Linq;

namespace ElsaSqlTransport.Workflow
{
    public class FirstWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder

                .WithPersistenceBehavior(WorkflowPersistenceBehavior.ActivityExecuted)
                //.WithContextType<Order>()

                .StartWith<Step1>();
        }
    }
}
