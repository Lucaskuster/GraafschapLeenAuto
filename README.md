# GraafschapLeenAuto

## Authenticatie en autorisatie
- In deze les gaan we het hebben over authorisatie.
- Autorisatie is het proces waarbij je controleert of iemand toegang heeft tot bepaalde resources.
- In de vorige les hebben we gekeken naar authenticatie.
- Authenticatie is het proces waarbij je controleert of iemand is wie hij zegt dat hij is.
 
## Migratie fout vorige les
- In de vorige les hebben we een fout gemaakt bij het toevoegen van de migratie.
- In de migratie hebben we gelijk aangemaakt en met de migrationBuilder de data toegevoegd.
``` csharp 
migrationBuilder.InsertData(
    table: "Users",
    columns: new[] { "Id", "Name", "Email", "Password" },
    values: new object[] { 1, "Admin", "admin@mail.com", "adminpassword" });
```
- Dit is niet de juiste manier om data toe te voegen.
- De juist manier is om de data toe te voegen in de Seed methode.
- Dit zet ik in de dbContext file.
``` csharp 
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = 2,
            Name = "User",
            Email = "user@example.com",
            Password = "UserPassword"
        });
}
```
- Door nu een nieuwe migratie aan te maken gebeurt het toevoegen van de gebruiker wel op de juiste manier.
``` csharp
public partial class CreateDefaultUser : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "Email", "Name", "Password" },
            values: new object[] { 2, "user@example.com", "User", "UserPassword" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Users",
            keyColumn: "Id",
            keyValue: 2);
    }
}
```


## Toevoegen Authorization builder
- Voeg in de program file the AddAuthorizationBuilder toe.
- Hiermee voeg kun je authorizatie services toevoegen.
``` csharp 
 services.AddAuthorizationBuilder()
           .SetFallbackPolicy(new AuthorizationPolicyBuilder()
           .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
           .RequireAuthenticatedUser()
           .Build());
```
- Er wordt een AuthorizationBuilder toegevoegd.
- Er wordt een fallback policy toegevoegd.
- De fallback policy is de policy die gebruikt wordt als er geen andere policy is.
- De fallback policy vereist dat de gebruiker geauthenticeerd is.
- De fallback policy vereist dat de gebruiker geauthenticeerd is met de JwtBearerDefaults.AuthenticationScheme.

- Tot slot voegen we aan de app de UseAuthorization toe.
``` csharp
 app.UseAuthorization();
```

## Toevoegen Entiteit Role en Seeden
- We voegen een Role entiteit toe.
- De entiteit Role heeft een Id, Name en een virtuele lijst van Users.
``` csharp
public class Role
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public virtual List<User> Users { get; set; } = new List<User>();
}
```

- Deze virtuele lijst van Users is een navigatie property.
- Een navigatie property is een property die een relatie heeft met een andere entiteit.
- In dit geval heeft de Role entiteit een relatie met de User entiteit.

- Door aan de User entiteit een navigatie property toe te voegen naar de Role entiteit, kunnen we de relatie tussen de User en Role entiteit vastleggen.
- Doordat dit twee lijsten zijn die naar elkaar verwijzen, is dit een many-to-many relatie.
- Entity Framework maakt hier automatisch een tussentabel voor aan.
- Dit is dadelijk te zien in de migratie
- Voeg de navigatie property toe aan de User entiteit.
``` csharp
public class User
{
	...
    public virtual List<Role> Roles { get; set; } = new List<Role>();
}
```

- Voeg dan de Role entiteit toe aan de dbContext.
``` csharp
public DbSet<Role> Roles { get; set; }
```
- En seed meteen de data in de dbContext.
``` csharp
modelBuilder.Entity<Role>().HasData(
    new Role
    {
        Id = 1,
        Name = "Admin"
    },
    new Role
    {
        Id = 2,
        Name = "User"
    });
```

- Als de migratie is aangemaakt, voeg je aan het migratie bestand de data toe voor de kopel tabel.
``` csharp
migrationBuilder.InsertData(
    table: "RoleUser",
    columns: new[] { "RolesId", "UsersId" },
    values: new object[,] 
    {
        { 1, 1 },
        { 2, 2 }
    }
    );
```

# Autoriseren!

## Tokens
- In de tokens gaan we bijhouden welke rechten een gebruiker heeft.
- Dit doen we door de claims toe te voegen aan de token.
- Claims zijn stukjes informatie die je aan de token kan toevoegen.
- Aan de token voegen we de claim "roles" toe.  
``` csharp
var claims = new List<Claim>
{
    ...
    new Claim("roles", getRoles(user))
};

private string getRoles(User user)
{
    var roles = new StringBuilder();

    if (user.Roles.Any(r => r.Name == nameof(UserRole.Admin)))
    {
        roles.Append(nameof(UserRole.Admin));
    }

    if (user.Roles.Any(r => r.Name == nameof(UserRole.User)))
    {
        roles.Append(nameof(UserRole.User));
    }

    return roles.ToString();
}
```
- Zo weet de Authorize attribute dadelijk welke rechten de gebruiker heeft.
    - ``` [Authorize(Roles = nameof(UserRole.Admin))] ```

## AuthController
- Voor het login endpoint heb ik de AllowAnonymous attribute verplaatst, voor een voorbeeld endpoint dat wel geauthoriseerd is.
- In tegenstelling tot de vorige les, toen de AllowAnonymous vooral voor de duidelijkheid was, is het nu noodzakelijk door de fallback policy in de program file.  
``` csharp
[AllowAnonymous]
[HttpPost]
public IActionResult Login([FromBody] LoginRequest request)
{
    var response = authService.Login(request);

    if (response == null)
    {
        return Unauthorized();
    }

    return Ok(response);
}
````

## User Controller en user service
### Toekennen van rollen
- In de user service heb ik een methode toegevoegd om een rol toe te kennen aan een gebruiker.
- Deze methode controleert of de gebruiker al een rol heeft en voegt de rol toe aan de gebruiker.
- Als de gebruiker al een rol heeft, wordt er een ArgumentException gegooid.
- Als de gebruiker of de rol niet bestaat, wordt er null gereturned.
- Als de rol is toegevoegd, wordt er een AssignRoleResponse gereturned.
``` csharp
public AssignRoleResponse? AssignRole(AssignRoleRequest request)
{
    var user = dbContext.Users
        .Include(x => x.Roles)
        .FirstOrDefault(x => x.Id == request.UserId);
    var role = dbContext.Roles.Find(request.RoleId);

    if (user == null || role == null)
    {
        return null;
    }

    if(user.Roles.Contains(role))
    {
        throw new ArgumentException("User already assigned to role");
    }

    user.Roles.Add(role);
    dbContext.SaveChanges();

    return new AssignRoleResponse
    {
        UserName = user.Name,
        RoleName = role.Name,
    };
}
```

### User Controller
- In de user controller heb ik een endpoint toegevoegd om een rol toe te kennen aan een gebruiker.
- Deze methode roept de AssignRole methode aan in de userService.
- Als de userService null teruggeeft, wordt er een NotFound gereturned.
- Als de userService een AssignRoleResponse teruggeeft, wordt er een Ok gereturned.
- Als de userService een ArgumentException gooit, wordt er een BadRequest gereturned, door het throwen van de exception in de userService.
``` csharp
[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.User))]
[HttpPatch("assign-role")]
public IActionResult AssignRole([FromBody] AssignRoleRequest request)
{
    var result = userService.AssignRole(request);

    if (result == null)
    {
        return NotFound();
    }

    return Ok(result);
}
```

- Hier is trouwens te zien dat er een UserRole enum is toegevoegd.
- Dit is een enum met de rollen die een gebruiker kan hebben en dus overal in de applicatie hetzelfde is.
``` csharp
public enum UserRole
{
    Admin = 1,
    User = 2,
}
```

### Controleren van rollen UserController
- In de UserController heb ik voor elk endpoinit een Authorize attribute toegevoegd.
- Hiermee wordt gecontroleerd of de gebruiker de juiste rol heeft om het endpoint te gebruiken.
- Als de gebruiker niet de juiste rol heeft, wordt er een 403 Forbidden gereturned.
``` csharp
[Authorize(Roles = nameof(UserRole.User))]
[HttpPut("{id}")]
public IActionResult UpdateUser(int id, [FromBody] User user)
{
    throw new NotImplementedException();
}
```
- Als er geen rol is toegevoegd aan de gebruiker, wordt er een 401 Unauthorized gereturned.
    - Dit komt weer door de fallback policy in de program file.
- Als het endpoint alleen ``` [Authorize] ``` heeft, wordt er gecontroleerd of de gebruiker **geauthenticeerd** is.
- ALs het endpoint ``` [AllowAnonymous] ``` heeft, wordt er niet gecontroleerd of de gebruiker **geauthenticeerd** is.
- ALs het endpoint geen Authorize attribute heeft, wordt er door de fallback policy gecontroleerd of de gebruiker **geauthenticeerd** is.
- Door de rollen in de authorize attribute te zetten, wordt er gecontroleerd of de gebruiker de juiste rol heeft.
- Als je meerdere rollen wilt toestaan, kun je deze scheiden met een komma.
``` csharp
[Authorize(Roles = nameof(UserRole.Admin) + "," + nameof(UserRole.User))]
```


## shared folder
- Tot slot heb ik de shared folder opgeruimd en folders aangemaakt om alles overzichtelijk te houden.
- Verder heb ik voor de niewe requests en responses nieuwe files aangemaakt.

## Demo 
TODO
- stappen voor demo 
- powerpoint presentatie