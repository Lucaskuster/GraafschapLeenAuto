# GraafschapLeenAuto

## Authenticatie en autorisatie
- In deze les gaan we het hebben over authenticatie.
- In de volgende les gaan we het hebben over autorisatie.
- Authenticatie is het proces waarbij je controleert of iemand is wie hij zegt dat hij is.
- Autorisatie is het proces waarbij je controleert of iemand toegang heeft tot bepaalde resources.

## Toevoegen AuthController en AuthService
- We maken een AuthController aan die de gebruiker kan inloggen en een token teruggeeft.
- Deze class krijgt het Allow Anonymous attribuut zodat iedereen erbij kan. 
  - ```[AllowAnonymous]```
- In deze class wordt de AuthService geïnjecteerd. (Dus zet dit ook in de program file)
  - ```services.AddTransient<AuthService>();```
- De AuthService heeft een Login methode die een token teruggeeft.
  - Eerst wordt gekeken of de gebruiker bestaat en of het wachtwoord klopt.
  - Als dat zo is wordt er een token gemaakt en teruggegeven.
  - Als dat niet zo is wordt er null teruggegeven.
- De AuthController login weet bij null dat er iets niet klopt en geeft dan een 401 terug.
  - ```return Unauthorized();```
- In de AuthService wordt de token gemaakt met de TokenService.

### LoginRequest and AuthResponse
- We maken een LoginRequest class aan die de inlog gegevens van de gebruiker bevat.
- We maken een AuthResponse class aan die de token bevat.
``` csharp 
[Required]
[EmailAddress]
public string Email { get; set; }

[Required]
public string Password { get; set; }
```
- De LoginRequest heeft een Email en een Password.
- De Email is verplicht en moet een email zijn

## Toevoegen TokenService
- De TokenService maakt een token aan met de gegeven claims.
``` csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Email, user.Email),
};
```
- De claims zijn de naam van de gebruiker en de rol van de gebruiker.
- ClaimTypes zijn standaard claims die je kunt gebruiken.
    - Het voordeel van deze standaard claims is dat je ze kunt gebruiken in de Authorize attributen en ze worden automatisch herkend.
- De token wordt ondertekend met de HmacSha256Signature en de key die in de appsettings.json staat.
      - Dit wordt gedaan om te voorkomen dat de key aangepast kan worden / gelezen kan worden door iemand anders.
``` csharp
var singingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
```
- De key is geheim en moet langer zijn dan 256 bits.
- Door de key weet de gebruiker dat de token echt is en niet aangepast is.
- De token wordt gemaakt met de JwtSecurityToken class.
``` csharp
 var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: singingCredentials
        );
```
- De token heeft een issuer, audience, claims, expires en signingCredentials.
- De issuer is de persoon die de token uitgeeft.
- De audience is de persoon die de token mag gebruiken.
- De claims zijn de gegevens die in de token staan.
- De expires is de tijd dat de token geldig is.
- De signingCredentials is de manier waarop de token ondertekend is.

### Jwt
- JWT staat voor JSON Web Token.
- Het is een specifiek type bearer token.

 ## Program 

 ### AddSwaggerGen
 ``` c# 
 options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
{
    In = ParameterLocation.Header,
    Description = "Please insert JWT with Bearer into field",
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey
});
 ```
- Hiermee voeg je een security definition toe aan de swagger ui.
- De naam van de security definition is Bearer.
- Bearer token is een token die je gebruikt om te authenticeren en is vrij gemakklijk te gebruiken vergeleken met andere tokens.

- AddSecurtityRequirement
``` c#
options.AddSecurityRequirement(new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
});
```
- Hiermee voeg je een security requirement toe aan de swagger ui.
- Dit betekent dat je een token nodig hebt om de swagger ui te gebruiken. 

### AddAuthentication
``` c#
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
});
```
- Hiermee voeg je authenticatie toe aan de applicatie. 
- De JwtBearerDefaults.AuthenticationScheme is de standaard authenticatie methode.
- De TokenValidationParameters zijn de parameters die de token moet hebben om geldig te zijn.
- Deze komen als het goed is bekend voor van de TokenService.



 ## Migration admin user
 Ik wilde nog een admin user toevoegen, maar ik had al een migratie gemaakt. 
 - Database Updaten zodat de migratie er niet meer op staat
    - ```Update-Database 0```
 - Pas de Migratie aan en voeg de admin user toe
    ``` migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Email", "Password" },
                values: new object[] { 1, "Admin", "admin@mail.com", "adminpassword" });
    ```
 - Voeg de migratie weer toe ``` Update-Database```

 

 ## denk aan
 ZET bearer voor de token in de swagger ui
