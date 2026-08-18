namespace StajProjesi.API.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(
            int userId,
            string permissionName);
    }
}