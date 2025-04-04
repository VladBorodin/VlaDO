using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services
{
    public class OrganizationService
    {
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<UserOrganizationRole> _userOrganizationRoleRepository;

        public OrganizationService(
            IGenericRepository<Organization> organizationRepository,
            IGenericRepository<UserOrganizationRole> userOrganizationRoleRepository)
        {
            _organizationRepository = organizationRepository;
            _userOrganizationRoleRepository = userOrganizationRoleRepository;
        }

        public async Task<Guid> CreateOrganizationAsync(Guid createdBy, string name, Guid clientTypeId, string? registrationCode)
        {
            var existingOrg = await _organizationRepository.FindAsync(o => o.Name == name);
            if (existingOrg.Any())
                throw new InvalidOperationException("Организация с таким именем уже существует.");

            var organization = new Organization
            {
                Name = name,
                ClientTypeId = clientTypeId,
                RegistrationCode = registrationCode,
                CreatedBy = createdBy
            };

            await _organizationRepository.AddAsync(organization);
            return organization.Id;
        }

        public async Task AddUserToOrganization(Guid userId, Guid organizationId, Guid roleId)
        {
            var existing = await _userOrganizationRoleRepository.FindAsync(
                uor => uor.UserId == userId && uor.OrganizationId == organizationId
            );

            if (existing.Any())
                throw new InvalidOperationException("Пользователь уже состоит в организации.");

            var userOrganizationRole = new UserOrganizationRole
            {
                UserId = userId,
                OrganizationId = organizationId,
                RoleId = roleId
            };

            await _userOrganizationRoleRepository.AddAsync(userOrganizationRole);
        }

    }
}
