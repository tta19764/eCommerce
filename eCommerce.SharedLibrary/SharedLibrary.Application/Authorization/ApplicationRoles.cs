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

    /// <summary>Customer role.</summary>
    public static ApplicationRoles Customer { get; } = new("Customer");

    /// <summary>Seller role.</summary>
    public static ApplicationRoles Seller { get; } = new("Seller");

    /// <summary>Administrator role.</summary>
    public static ApplicationRoles Admin { get; } = new("Admin");

    /// <summary>Gets the role name string.</summary>
    public string Name => field ?? string.Empty;

    /// <summary>
    /// Executes the ToString operation.
    /// </summary>
    public override string ToString()
    {
        return Name;
    }

    /// <summary>Implicit conversion to string representation.</summary>
    public static implicit operator string(ApplicationRoles role)
    {
        return role.Name;
    }
}

