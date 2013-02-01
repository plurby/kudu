using Kudu.Contracts.Settings;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Kudu.Core.Deployment.Generator
{
    public class BasicBuilder : BaseBasicBuilder
    {
        public BasicBuilder(IEnvironment environment, IDeploymentSettingsManager settings, IBuildPropertyProvider propertyProvider, string repositoryPath, string projectPath)
            : base(environment, settings, propertyProvider, repositoryPath, projectPath, "--basic")
        {
        }

        public override Task Build(DeploymentContext context)
        {
            context.Logger.Log("Basic Deployment");
            return Task.FromResult<object>(null);
        }
    }
}
