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

        public IGraphServiceApplicationsCollectionRequestBuilder Applications => _graphServiceClient.Applications;

        public IGraphServiceAdministrativeUnitsCollectionRequestBuilder AdministrativeUnits => _graphServiceClient.AdministrativeUnits;

        public IGraphServiceAllowedDataLocationsCollectionRequestBuilder AllowedDataLocations => _graphServiceClient.AllowedDataLocations;

        public IGraphServiceAppRoleAssignmentsCollectionRequestBuilder AppRoleAssignments => _graphServiceClient.AppRoleAssignments;

        public IGraphServiceContactsCollectionRequestBuilder Contacts => _graphServiceClient.Contacts;

        public IGraphServiceDevicesCollectionRequestBuilder Devices => _graphServiceClient.Devices;

        public IGraphServiceDomainsCollectionRequestBuilder Domains => _graphServiceClient.Domains;

        public IGraphServiceDomainDnsRecordsCollectionRequestBuilder DomainDnsRecords => _graphServiceClient.DomainDnsRecords;

        public IGraphServiceGroupsCollectionRequestBuilder Groups => _graphServiceClient.Groups;

        public IGraphServiceDirectoryRolesCollectionRequestBuilder DirectoryRoles => _graphServiceClient.DirectoryRoles;

        public IGraphServiceDirectoryRoleTemplatesCollectionRequestBuilder DirectoryRoleTemplates => _graphServiceClient.DirectoryRoleTemplates;

        public IGraphServiceDirectorySettingTemplatesCollectionRequestBuilder DirectorySettingTemplates => _graphServiceClient.DirectorySettingTemplates;

        public IGraphServiceOrganizationCollectionRequestBuilder Organization => _graphServiceClient.Organization;

        public IGraphServiceOauth2PermissionGrantsCollectionRequestBuilder Oauth2PermissionGrants => _graphServiceClient.Oauth2PermissionGrants;

        public IGraphServiceScopedRoleMembershipsCollectionRequestBuilder ScopedRoleMemberships => _graphServiceClient.ScopedRoleMemberships;

        public IGraphServiceServicePrincipalsCollectionRequestBuilder ServicePrincipals => _graphServiceClient.ServicePrincipals;

        public IGraphServiceSettingsCollectionRequestBuilder Settings => _graphServiceClient.Settings;

        public IGraphServiceSubscribedSkusCollectionRequestBuilder SubscribedSkus => _graphServiceClient.SubscribedSkus;

        public IGraphServiceUsersCollectionRequestBuilder Users => _graphServiceClient.Users;

        public IGraphServicePoliciesCollectionRequestBuilder Policies => _graphServiceClient.Policies;

        public IGraphServiceContractsCollectionRequestBuilder Contracts => _graphServiceClient.Contracts;

        public IGraphServiceWorkbooksCollectionRequestBuilder Workbooks => _graphServiceClient.Workbooks;

        public IGraphServiceDrivesCollectionRequestBuilder Drives => _graphServiceClient.Drives;

        public IGraphServiceSharesCollectionRequestBuilder Shares => _graphServiceClient.Shares;

        public IGraphServiceSitesCollectionRequestBuilder Sites => _graphServiceClient.Sites;

        public IGraphServiceSubscriptionsCollectionRequestBuilder Subscriptions => _graphServiceClient.Subscriptions;

        public IGraphServiceIdentityRiskEventsCollectionRequestBuilder IdentityRiskEvents => _graphServiceClient.IdentityRiskEvents;

        public IGraphServiceImpossibleTravelRiskEventsCollectionRequestBuilder ImpossibleTravelRiskEvents => _graphServiceClient.ImpossibleTravelRiskEvents;

        public IGraphServiceLeakedCredentialsRiskEventsCollectionRequestBuilder LeakedCredentialsRiskEvents => _graphServiceClient.LeakedCredentialsRiskEvents;

        public IGraphServiceAnonymousIpRiskEventsCollectionRequestBuilder AnonymousIpRiskEvents => _graphServiceClient.AnonymousIpRiskEvents;

        public IGraphServiceSuspiciousIpRiskEventsCollectionRequestBuilder SuspiciousIpRiskEvents => _graphServiceClient.SuspiciousIpRiskEvents;

        public IGraphServiceUnfamiliarLocationRiskEventsCollectionRequestBuilder UnfamiliarLocationRiskEvents => _graphServiceClient.UnfamiliarLocationRiskEvents;

        public IGraphServiceMalwareRiskEventsCollectionRequestBuilder MalwareRiskEvents => _graphServiceClient.MalwareRiskEvents;

        public IGraphServicePrivilegedRolesCollectionRequestBuilder PrivilegedRoles => _graphServiceClient.PrivilegedRoles;

        public IGraphServicePrivilegedRoleAssignmentsCollectionRequestBuilder PrivilegedRoleAssignments => _graphServiceClient.PrivilegedRoleAssignments;

        public IGraphServicePrivilegedOperationEventsCollectionRequestBuilder PrivilegedOperationEvents => _graphServiceClient.PrivilegedOperationEvents;

        public IGraphServicePrivilegedSignupStatusCollectionRequestBuilder PrivilegedSignupStatus => _graphServiceClient.PrivilegedSignupStatus;

        public IGraphServicePrivilegedApprovalCollectionRequestBuilder PrivilegedApproval => _graphServiceClient.PrivilegedApproval;

        public IGraphServicePrivilegedRoleAssignmentRequestsCollectionRequestBuilder PrivilegedRoleAssignmentRequests => _graphServiceClient.PrivilegedRoleAssignmentRequests;

        public IGraphServiceInvitationsCollectionRequestBuilder Invitations => _graphServiceClient.Invitations;

        public IGraphServiceCommandsCollectionRequestBuilder Commands => _graphServiceClient.Commands;

        public IGraphServicePayloadResponseCollectionRequestBuilder PayloadResponse => _graphServiceClient.PayloadResponse;

        public IGraphServiceTeamsCollectionRequestBuilder Teams => _graphServiceClient.Teams;

        public IGraphServiceGroupLifecyclePoliciesCollectionRequestBuilder GroupLifecyclePolicies => _graphServiceClient.GroupLifecyclePolicies;

        public IGraphServiceIdentityProvidersCollectionRequestBuilder IdentityProviders => _graphServiceClient.IdentityProviders;

        public IGraphServiceFunctionsCollectionRequestBuilder Functions => _graphServiceClient.Functions;

        public IGraphServiceFilterOperatorsCollectionRequestBuilder FilterOperators => _graphServiceClient.FilterOperators;

        public IGraphServiceDataPolicyOperationsCollectionRequestBuilder DataPolicyOperations => _graphServiceClient.DataPolicyOperations;

        public IGraphServiceAgreementsCollectionRequestBuilder Agreements => _graphServiceClient.Agreements;

        public IGraphServiceAgreementAcceptancesCollectionRequestBuilder AgreementAcceptances => _graphServiceClient.AgreementAcceptances;

        public IGraphServiceBookingBusinessesCollectionRequestBuilder BookingBusinesses => _graphServiceClient.BookingBusinesses;

        public IGraphServiceBookingCurrenciesCollectionRequestBuilder BookingCurrencies => _graphServiceClient.BookingCurrencies;

        public IGraphServicePrivilegedAccessCollectionRequestBuilder PrivilegedAccess => _graphServiceClient.PrivilegedAccess;

        public IGraphServiceGovernanceResourcesCollectionRequestBuilder GovernanceResources => _graphServiceClient.GovernanceResources;

        public IGraphServiceGovernanceSubjectsCollectionRequestBuilder GovernanceSubjects => _graphServiceClient.GovernanceSubjects;

        public IGraphServiceGovernanceRoleDefinitionsCollectionRequestBuilder GovernanceRoleDefinitions => _graphServiceClient.GovernanceRoleDefinitions;

        public IGraphServiceGovernanceRoleAssignmentsCollectionRequestBuilder GovernanceRoleAssignments => _graphServiceClient.GovernanceRoleAssignments;

        public IGraphServiceGovernanceRoleAssignmentRequestsCollectionRequestBuilder GovernanceRoleAssignmentRequests => _graphServiceClient.GovernanceRoleAssignmentRequests;

        public IGraphServiceGovernanceRoleSettingsCollectionRequestBuilder GovernanceRoleSettings => _graphServiceClient.GovernanceRoleSettings;

        public IGraphServiceAccessReviewsCollectionRequestBuilder AccessReviews => _graphServiceClient.AccessReviews;

        public IGraphServiceBusinessFlowTemplatesCollectionRequestBuilder BusinessFlowTemplates => _graphServiceClient.BusinessFlowTemplates;

        public IGraphServiceAccessReviewDecisionsCollectionRequestBuilder AccessReviewDecisions => _graphServiceClient.AccessReviewDecisions;

        public IGraphServiceProgramsCollectionRequestBuilder Programs => _graphServiceClient.Programs;

        public IGraphServiceProgramControlsCollectionRequestBuilder ProgramControls => _graphServiceClient.ProgramControls;

        public IGraphServiceProgramControlTypesCollectionRequestBuilder ProgramControlTypes => _graphServiceClient.ProgramControlTypes;

        public ICommsApplicationRequestBuilder App => _graphServiceClient.App;

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

        public IDataClassificationServiceRequestBuilder DataClassification => _graphServiceClient.DataClassification;

        public ISecurityRequestBuilder Security => _graphServiceClient.Security;

        public IOfficeConfigurationRequestBuilder OfficeConfiguration => _graphServiceClient.OfficeConfiguration;
    }        
}
