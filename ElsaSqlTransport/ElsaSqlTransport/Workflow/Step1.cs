using Elsa.ActivityResults;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ElsaSqlTransport.Workflow
{
    public class Step1 : Activity
    {
        private readonly ILogger<Step1> logger;

        public Step1(ILogger<Step1> logger)
        {
            this.logger = logger;
        }

        protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
        {
            logger.LogInformation($"Executing {nameof(Step1)} for {context.ContextId}");

            try
            {
                await Task.Delay(2000);

                return Done();
            }
            catch (Exception ex)
            {
                return Fault(ex);
            }
        }
    }

}
