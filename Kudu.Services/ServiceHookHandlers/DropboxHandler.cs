﻿using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Kudu.Contracts.Dropbox;
using Kudu.Contracts.Settings;
using Kudu.Contracts.Tracing;
using Kudu.Core;
using Kudu.Core.Deployment;
using Kudu.Core.SourceControl;
using Newtonsoft.Json.Linq;

namespace Kudu.Services.ServiceHookHandlers
{
    public class DropboxHandler : IServiceHookHandler
    {
        private readonly DropboxHelper _dropBoxHelper;

        public DropboxHandler(ITracer tracer,
                              IDeploymentStatusManager status,
                              IDeploymentSettingsManager settings,
                              IEnvironment environment)
        {
            _dropBoxHelper = new DropboxHelper(tracer, status, settings, environment);
        }

        public DeployAction TryParseDeploymentInfo(HttpRequestBase request, JObject payload, string targetBranch, out DeploymentInfo deploymentInfo)
        {
            deploymentInfo = null;
            if (!String.IsNullOrEmpty(payload.Value<string>("NewCursor")))
            {
                var dropboxInfo = new DropboxInfo(payload);
                deploymentInfo = dropboxInfo; 

                // Temporary deployment
                deploymentInfo.TargetChangeset = DeploymentManager.CreateTemporaryChangeSet(
                    authorName: dropboxInfo.DeployInfo.UserName,
                    authorEmail: dropboxInfo.DeployInfo.Email,
                    message: String.Format(CultureInfo.CurrentUICulture, Resources.Dropbox_Synchronizing, dropboxInfo.DeployInfo.Deltas.Count())
                );

                return DeployAction.ProcessDeployment;
            }

            return DeployAction.UnknownPayload;
        }

        public virtual async Task Fetch(IRepository repository, DeploymentInfo deploymentInfo, string targetBranch, ILogger logger)
        {
            // (A)sync with dropbox
            var dropboxInfo = ((DropboxInfo)deploymentInfo);
            deploymentInfo.TargetChangeset = await _dropBoxHelper.Sync(dropboxInfo, targetBranch, logger, repository);
        }

        internal class DropboxInfo : DeploymentInfo
        {
            public DropboxInfo(JObject payload)
            {
                Deployer = DropboxHelper.Dropbox;
                DeployInfo = payload.ToObject<DropboxDeployInfo>();
                // This ensure that the fetch handler provides an underlying Git repository.
                RepositoryType = RepositoryType.Git;
                IsReusable = false;
            }

            public DropboxDeployInfo DeployInfo { get; set; }
        }
    }
}