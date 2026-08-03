namespace SharedLibrary.Application.Authorization;

/// <summary>
/// Application role names expected in identity-provider tokens.
/// </summary>
public readonly record struct ApplicationRoles
{
    private ApplicationRoles(string name)
    {
        Name = name;
    }

    public static ApplicationRoles Customer { get; } = new("Customer");

    public static ApplicationRoles Admin { get; } = new("Admin");

    public string Name => field ?? string.Empty;

    public override string ToString()
    {
        return Name;
    }

    public static implicit operator string(ApplicationRoles role)
    {
        return role.Name;
    }
}
