using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Membership.Services
{
    public interface IGraphApplicationClient : IGraphServiceClient
    {
    }

    public interface IGraphDelegatedClient : IGraphServiceClient
    {
    }


    public class GraphClient : IGraphApplicationClient, IGraphDelegatedClient
    {
        private readonly IGraphServiceClient _graphServiceClient;

        public GraphClient(IGraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public IAuthenticationProvider AuthenticationProvider => _graphServiceClient.AuthenticationProvider;

        public string BaseUrl => _graphServiceClient.BaseUrl;

        public IHttpProvider HttpProvider => _graphServiceClient.HttpProvider;

        public IGraphServiceSchemaExtensionsCollectionRequestBuilder SchemaExtensions => _graphServiceClient.SchemaExtensions;

        public IGraphServiceDirectoryObjectsCollectionRequestBuilder DirectoryObjects => _graphServiceClient.DirectoryObjects;

        public IGraphServiceDevicesCollectionRequestBuilder Devices => _graphServiceClient.Devices;

        public IGraphServiceDomainsCollectionRequestBuilder Domains => _graphServiceClient.Domains;

        public IGraphServiceDomainDnsRecordsCollectionRequestBuilder DomainDnsRecords => _graphServiceClient.DomainDnsRecords;

        public IGraphServiceGroupsCollectionRequestBuilder Groups => _graphServiceClient.Groups;

        public IGraphServiceDirectoryRolesCollectionRequestBuilder DirectoryRoles => _graphServiceClient.DirectoryRoles;

        public IGraphServiceDirectoryRoleTemplatesCollectionRequestBuilder DirectoryRoleTemplates => _graphServiceClient.DirectoryRoleTemplates;

        public IGraphServiceOrganizationCollectionRequestBuilder Organization => _graphServiceClient.Organization;

        public IGraphServiceSubscribedSkusCollectionRequestBuilder SubscribedSkus => _graphServiceClient.SubscribedSkus;

        public IGraphServiceUsersCollectionRequestBuilder Users => _graphServiceClient.Users;

        public IGraphServiceContractsCollectionRequestBuilder Contracts => _graphServiceClient.Contracts;

        public IGraphServiceWorkbooksCollectionRequestBuilder Workbooks => _graphServiceClient.Workbooks;

        public IGraphServiceDrivesCollectionRequestBuilder Drives => _graphServiceClient.Drives;

        public IGraphServiceSharesCollectionRequestBuilder Shares => _graphServiceClient.Shares;

        public IGraphServiceSitesCollectionRequestBuilder Sites => _graphServiceClient.Sites;

        public IGraphServiceSubscriptionsCollectionRequestBuilder Subscriptions => _graphServiceClient.Subscriptions;

       public IGraphServiceInvitationsCollectionRequestBuilder Invitations => _graphServiceClient.Invitations;

        public IGraphServiceTeamsCollectionRequestBuilder Teams => _graphServiceClient.Teams;

        public IGraphServiceGroupLifecyclePoliciesCollectionRequestBuilder GroupLifecyclePolicies => _graphServiceClient.GroupLifecyclePolicies;

        public IGraphServiceIdentityProvidersCollectionRequestBuilder IdentityProviders => _graphServiceClient.IdentityProviders;

        public IUserRequestBuilder Me => _graphServiceClient.Me;

        public IDirectoryRequestBuilder Directory => _graphServiceClient.Directory;

        public IDriveRequestBuilder Drive => _graphServiceClient.Drive;

        public IPlannerRequestBuilder Planner => _graphServiceClient.Planner;

        public IAuditLogRootRequestBuilder AuditLogs => _graphServiceClient.AuditLogs;

        public IDeviceManagementRequestBuilder DeviceManagement => _graphServiceClient.DeviceManagement;

        public IDeviceAppManagementRequestBuilder DeviceAppManagement => _graphServiceClient.DeviceAppManagement;

        public IReportRootRequestBuilder Reports => _graphServiceClient.Reports;

        public IAppCatalogsRequestBuilder AppCatalogs => _graphServiceClient.AppCatalogs;

        public IEducationRootRequestBuilder Education => _graphServiceClient.Education;

        public ISecurityRequestBuilder Security => _graphServiceClient.Security;

        public IGraphServiceGroupSettingsCollectionRequestBuilder GroupSettings => throw new NotImplementedException();

        public IGraphServiceGroupSettingTemplatesCollectionRequestBuilder GroupSettingTemplates => throw new NotImplementedException();

        public IGraphServiceDataPolicyOperationsCollectionRequestBuilder DataPolicyOperations => throw new NotImplementedException();

        public Func<IAuthenticationProvider> PerRequestAuthProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }        
}
