namespace GraafschapLeenAuto.Shared.Interfaces;

public interface ICurrentUserContext
{
    public CurrentUser User { get; }
    public bool IsAuthenticated { get; }
    public bool IsInRole(string role);

    public record CurrentUser(
        int Id,
        string Name,
        string Email,
        List<string> Roles);
}
