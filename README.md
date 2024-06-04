# GraafschapLeenAuto

# Misschien later nog foutmelding bij inloggen toevoegen

## Authenticatie en autorisatie in Blazor
- In deze les gaan we kijken naar authenticatie en autorisatie in Blazor

## HttpClients
- In de vorige les hebben we een UserHttpClient aangemaakt die de data van de API ophaalt
- Nu gaan we een AuthHttpClient maken, deze is bijna hetzelfde als de UserHttpClient
- Een verschil is dat we voor het inloggen inlog gegevens meegeven in de body van de request

``` c#
public async Task<AuthResponse?> Login(LoginRequest loginRequest)
{
    var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
    var response = await client.PostAsync(string.Empty, content);

    if(!response.IsSuccessStatusCode)
    {
        return null;
    }

    var responseContent = await response.Content.ReadAsStringAsync();
    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, SerializerOptions);

    return authResponse;
}
```

- Verder heb ik in beide clients de http uri aangepast naar de https uri van de API
- Dit heeft te maken met de AuthorizationMessageHandler, die later naar voren komt. 
- Vergeet niet de client in de program.cs te registreren, ```  builder.Services.AddScoped<AuthHttpClient>(); ```

### Uitlezen en bewaren token
- In de AuthResponse zit een token, deze token gaan we gebruiken om de gebruiker te identificeren
- Deze moeten we ergens bewaren, zodat we deze kunnen gebruiken bij calls naar de API
- We kunnen deze token bewaren in de LocalStorage van de browser
- We maken een service die de token kan bewaren en ophalen
- Deze service plaatsen we in een nieuwe folder Services
- We injecteren IJSRuntime in de service, zodat we de LocalStorage kunnen gebruiken

``` c#
public async Task SetItemAsync(string key, string value)
{
    await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
}

public async Task<string> GetItemAsync(string key)
{
    return await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
}

public async Task RemoveItemAsync(string key)
{
    await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
}
```

- We registreren de service in de program.cs, ``` builder.Services.AddScoped<LocalStorageService>(); ```
- Er staan drie methodes in de service, SetItemAsync, GetItemAsync en RemoveItemAsync
- Hiermee kunnen we de token bewaren, ophalen en verwijderen

## State management
- Dan hebben we een service nodig die de authenticatie status van de gebruiker bijhoudt
- Microsoft heeft een ``` AuthenticationStateProvider ``` die authenticatie gegevens van de gebruiker bijhoudt.
- Deze gaan we uitbreiden met onze eigen service, ``` GraafschapLeenAutoAuthenticationStateProvider ```
- Deze service houdt de authenticatie status van de gebruiker bij en kan de status ophalen en updaten

``` c#
public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    var token = await localStorage.GetItemAsync("token");
    var principal = new ClaimsPrincipal();

    if (!string.IsNullOrEmpty(token))
    {
        principal = CreateClaimsPrincipalFromToken(token);
    }

    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

    return new(principal);
}

private static ClaimsPrincipal CreateClaimsPrincipalFromToken(string token)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var identity = new ClaimsIdentity();

    if(tokenHandler.CanReadToken(token))
    {
        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);
        identity = new(
            jwtSecurityToken.Claims, 
            authenticationType: "Bearer token",
            nameType: Claims.Name,
            roleType: Claims.Role);
    }

    return new(identity);
}

public async Task Logout()
{
    await localStorage.RemoveItemAsync("token");
    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
}
```

- De GetAuthenticationStateAsync methode haalt de token op uit de LocalStorage
- Als er een token is, wordt er een ClaimsPrincipal gemaakt met de token in de CreateClaimsPrincipalFromToken methode
- Als er geen token is, wordt er een lege ClaimsPrincipal gemaakt
- Dit betekend dat de gebruiker niet is ingelogd
- Daarom wordt er in de logout methode de token verwijderd en de authenticatie status geupdate met een lege ClaimsPrincipal
- De NotifyAuthenticationStateChanged methode zorgt ervoor dat de UI geupdate wordt als de authenticatie status verandert
- Hoe dit gaat werken met de razor pages, gaan we later zien

- Vergeet niet de service te registreren in de program.cs, 
``` c# 
builder.Services.AddScoped<GraafschapLeenAutoAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<GraafschapLeenAutoAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();
```
- De GraafschapLeenAutoAuthenticationStateProvider moet geregistreerd worden als AuthenticationStateProvider
- We registreren ook de AuthorizatioCore, zodat we later kunnen werken met autorisatie

## CurentUserContext 
- We hebben een service nodig die de huidige gebruiker bijhoudt
- Deze service gaan we gebruiken om de huidige gebruiker op te halen en te bewaren
- We maken een nieuwe service, ``` CurrentUserContext ```

``` c#
public ICurrentUserContext.CurrentUser User { get; set; }

    public bool IsAuthenticated { get; set; }

    public CurrentUserContext(GraafschapLeenAutoAuthenticationStateProvider authenticationStateProvider)
    {
        authenticationStateProvider.AuthenticationStateChanged += (state) =>
        {
            var authState = state.Result;
            IsAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;
            if (IsAuthenticated == true)
            {
                var claims = authState.User;
                var id = claims.FindFirst(Claims.Id)!.Value;
                var name = claims.FindFirst(Claims.Name)!.Value;
                var email = claims.FindFirst(Claims.Email)!.Value;
                var roles = claims.FindAll(Claims.Role).Select(r => r.Value).ToList();
                User = new ICurrentUserContext.CurrentUser(int.Parse(id), name, email, roles);
            }
        };
    }

    public bool IsInRole(string roleName)
    {
        return User.Roles.Contains(roleName);
    }
```
- De CurrentUserContext heeft een User property en een IsAuthenticated property
- De User property is een ICurrentUserContext.CurrentUser object
- De IsAuthenticated property is een boolean die aangeeft of de gebruiker is ingelogd
- In de constructor van de CurrentUserContext luisteren we naar de AuthenticationStateChanged event van de GraafschapLeenAutoAuthenticationStateProvider
- Als de authenticatie status verandert, wordt de User en IsAuthenticated property geupdate
- Als de gebruiker is ingelogd, worden de claims van de gebruiker opgehaald en in de User property gezet
- De IsInRole methode kijkt of de gebruiker een bepaalde rol heeft

### Interface ICurrentUserContext
- Vergeet niet de interface in het shared project toe te voegen, deze kan namelijk ook gebruikt worden in de api
``` c#
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
```
- Voeg nu de CurrentUserContext toe aan de DI container in de program.cs
``` c#
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
```

## AuthorizationMessageHandler
- We willen nu bij elke request naar de API de token meesturen
- Daarom maken we deze AuthorizationMessageHandler, deze inherited van DelegatingHandler
- Dat betekend dat we de SendAsync methode kunnen overriden, deze wordt aangeroepen bij elke request
- We willen deze overriden zodat we de token kunnen meesturen
- De token wordt opgehaald met de localstorage service

``` c#
protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    var bearerToken = await localStorage.GetItemAsync("token");

    if (!string.IsNullOrEmpty(bearerToken))
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
    } else
    {
        throw new ArgumentNullException(nameof(request), "No token found in local storage");
    }

    return await base.SendAsync(request, cancellationToken);
}
```

- Vergeet niet de AuthorizationMessageHandler te registreren in de program.cs
``` c#
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient(nameof(UserHttpClient)).AddHttpMessageHandler<AuthorizationMessageHandler>();
```
- Nu wordt bij elke request van de UserHttpClient de token meegestuurd

## Layout 
### MainLayout
- We gaan de layout aanpassen zodat niet ingelogde gebruikers een andere layout zien dan ingelogde gebruikers
- Voor de MainLayout voegen we een uitlog knop toe als de gebruiker is ingelogd
``` c#
<AuthorizeView>
    <Authorized>
        Welcome! <a href="#" @onclick=(Logout) class="btn btn-primary">Logout</a>
    </Authorized>
</AuthorizeView>
```
- Met de AuthorizeView component kunnen we authorisatie regelen
- Met de Authorized component kunnen we bepaalde delen van de layout laten zien aan gebruikers die zijn ingelogd

- Verder moeten we de implementatie van de logout methode nog maken
``` c#
public async Task Logout()
{
    await AuthenticationStateProvider.Logout();
    NavigationManager.NavigateTo("/login");
}
```
- De logout methode roept de logout methode van de AuthenticationStateProvider aan
- Daarna wordt de gebruiker doorgestuurd naar de login pagina

### NavMenu
- We gaan de NavMenu aanpassen zodat niet ingelogde gebruikers geen menu kunnen zien 
- Dit doen we door weer de AuthorizeView en Authorized component te gebruiken
``` c#
<AuthorizeView>
    <Authorized>
        <nav class="flex-column">
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                    <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
                </NavLink>
            </div>
        </nav>
    </Authorized>
</AuthorizeView>
```

## Login pagina
- Nu staat er een uitlog knop in de layout, maar de gebruiker kan nog niet inloggen
- We gaan een login pagina maken
- We maken een nieuwe pagina, Login.razor
- In deze pagina maken we een form met een input voor email en password
- Als de gebruiker op de submit knop drukt, wordt de login methode aangeroepen
- Deze methode roept de Login methode van de AuthHttpClient aan
- Als de login succesvol is, wordt de token opgeslagen in de LocalStorage en wordt de gebruiker doorgestuurd naar de home pagina
- De Request property is een LoginRequest object, deze bevat de email en password van de gebruiker (zie shared project)

``` c#
@page "/login"

...

<EditForm Model="Request" OnSubmit="LoginAsync">
    <PageHeader Size="PageHeader.Sizes.H2">Please Login!</PageHeader>

    <div class="form-floating">
        <InputText id="email" class="form-control" @bind-Value="Request.Email" placeholder="Email" />
        <label for="email">Email address</label>
    </div>

    <div class="form-floating">
        <InputText id="password" class="form-control" @bind-Value="Request.Password" placeholder="Password" type="password" />
        <label for="password">Password</label>
    </div>

    <br />

    <button class="btn btn-primary w-100 py-2" type="submit">Login</button>
</EditForm>

@code {
    private readonly LoginRequest Request = new();

    private async Task LoginAsync()
    {
        var response = await AuthHttpClient.Login(Request);

        if(response is null)
        {
            return;   
        }

        await LocalStorageService.SetItemAsync("token", response.Token);
        await AuthenticationStateProvider.GetAuthenticationStateAsync();

        NavigationManager.NavigateTo("/");
    }
}
```

- Vergeet niet de AuthHttpClient, LocalStorageService en AuthenticationStateProvider te injecteren in de Login pagina
- Vergeet ook niet de Request property te initialiseren
- In het code blok van de Login pagina maken we een LoginAsync methode
- Deze methode roept de Login methode van de AuthHttpClient aan
- Als de login succesvol is, wordt de token opgeslagen in de LocalStorage en wordt de gebruiker doorgestuurd naar de home pagina
- Ook wordt de huidige authenticatie status opgehaald

### App en Redirect naar login pagina
- Als de gebruiker niet is ingelogd, willen we dat de gebruiker wordt doorgestuurd naar de login pagina
- Dit kunnen we doen door de AuthorizeView component te gebruiken in de MainLayout
- 
``` c#
<CascadingAuthenticationState>
    ...
    <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
        <NotAuthorized>
            @if (Context.IsAuthenticated)
            {
                <p>You're not authorized to view this page.</p>
            }
            else
            {
                <LoginRedirect />
            }
        </NotAuthorized>
        <Authorizing>
            <div class="spinner-border text-primary" role="status">
                <span class="sr-only"></span>
            </div>
        </Authorizing>
    </AuthorizeRouteView>
    ...
</CascadingAuthenticationState>

```
- Met de CascadingAuthenticationState component kunnen we de huidige authenticatie status doorgeven aan de hele applicatie
- De AuthorizeRouteView en NotAuthorized component worden gebruikt om te bepalen of de gebruiker is ingelogd en geauthoriseerd
- De NotAuthorized component wordt aangeroepen als de gebruiker niet is ingelogd, maar ook niet geauthoriseerd is
- Door deze if else statement kunnen we bepalen of de gebruiker is ingelogd of niet en of de gebruiker geauthoriseerd is of niet

- Het redirect component ziet er als volgt uit
``` c#
@inject NavigationManager uriHelper

@code {
protected override void OnInitialized()
    {
        uriHelper.NavigateTo("login");
    }
}
```
- Dit staat in een eigen component, LoginRedirect.razor omdat je misschien op andere plekken ook wilt redirecten naar de login pagina

## Home pagina
- Tot slot geef ik op de home pagina aan dat je daar alleen mag komen als je geauthoriseerd bent
- Dit doe ik met het Authorize attribute boven de pagina
``` c#
@attribute [Authorize]
```


## Claims
- Verder heb ik de claims in een enum gezet in de shared project

## ==================== Extra ====================

## Claims
- Ik heb de claims in een enum gezet in de shared project
- Dit is handig omdat we de claims nu kunnen gebruiken in de hele applicatie
- Verder heb ik de email en role claim een andere naam gegeven, omdat deze nu anders zijn dan de standaard claims
- De httpContextAccessor in de API geeft de claims namelijk terug met een andere naam (De microsoft Claims)
``` c#
public const string Email = "LeenAuto/email";
public const string Role = "LeenAuto/roles";
```

## Backend API currentUser
