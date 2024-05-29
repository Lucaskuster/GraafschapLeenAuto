# GraafschapLeenAuto

## Aanmaken Blazor project

- Voeg in de folder van je andere projecten een nieuw project toe
- Kies voor Blazor WebAssembly Standalone App
- Geef hem een correcte naam, in deze solution is dat GraafschapLeenAuto.Blazor

## Blazor project aanpassen
- Blazor maakt meteen een hele hoop bestanden aan, daar lopen we even doorheen
- wwwroot 
  - Deze map bevat de statische bestanden van de applicatie
  - Denk aan css, images, js, etc.
- Layout
    - Hierin staat de layout van de pagina verdeelt in de body en de nav
    - Hierin staat ook de routing van de pagina's
- pages 
    - Hierin staan de pagina's van de applicatie
    - Waarna geroute wordt vanuit de layout
    - De pagina's Counter en Weather kunnen weg
    - Voor lesdoeleinden laat ik de weather pagina staan
- Dan heb je nog de _Imports.razor, App.razor en Program.cs
	- _Imports.razor
		- Hierin staan de imports die gelden voor de ghele applicatie
	- App.razor
		- De hoofdcomponent van de app met HTML-`<head>` opmaak, de Routescomponent en de Blazor-`<script>` tag. De rootcomponent is de eerste component die de app laadt.
	- Program.cs
		- Hierin staat de main functie van de applicatie

## Herschrijf de Homepagina
- Voor dit voorbeeld gaan we de homepagina herschrijven
``` razor
@page "/"

@using GraafschapLeenAuto.Shared.Dtos;

<PageTitle>Login</PageTitle>

@if (Users != null)
{
    <table class="table">
        <thead>
            <tr>
                <th>Id</th>
                <th>Email</th>
                <th>Role</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var user in Users)
            {
                <tr>
                    <td>@user.Id</td>
                    <td>@user.Name</td>
                    <td>@user.Email</td>
                </tr>
            }
        </tbody>
    </table>
}

@code{
    public UserDto[] Users { get; set; }

    protected override async Task OnInitializedAsync()
    {
    }
}

```

- In de code hierboven wordt de UserHttpClient geïnjecteerd in de pagina (die we dadelijk gaan aanmaken)
- De UserHttpClient is een HttpClient die we zelf hebben aangemaakt
- De UserHttpClient haalt de data op van de API
- De html in de pagina toont de data die is opgehaald van de API
- Verder is de code in de pagina een combinatie van C# en HTML

## Add httpClient
- Voeg een Clients folder toe aan de Shared folder 
- Hier gaan we de UserHttpClient aanmaken, deze kan natuurlijk door de gehele solution gebruikt worden.
- Eerst installeer Microsoft.Extensions.Http met nuget, deze is nodig voor de HttpClientFactory
- We gebruiken een IHttpClientFactory om de UserHttpClient te maken.
- We geven de client de base url van de API mee, deze is te vinden in de launchSettings.json van de API. LET OP dit is de url van de API, niet van de Blazor app.
- Let ook op, deze url is hardcoded, dit is niet de beste manier om dit te doen, maar voor nu is het prima.
- Zorg er ook voor dat je de url van de api pakt en niet die van de swagger ui
``` c#
public UserHttpClient(IHttpClientFactory httpClientFactory)
{
    client = httpClientFactory.CreateClient();
    client.BaseAddress = new Uri("http://localhost:5236/User");

    jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
}
```
- Met de json options kunnen we de data die we ophalen van de API omzetten naar een object
- Het ophalen en omzetten gebeurt in de GetUsers methode
``` c#
public async Task<UserDto[]> GetUsers()
{
    var response = await client.GetAsync(string.Empty);

    if(!response.IsSuccessStatusCode)
    {
        return [];
    }

    var content = await response.Content.ReadAsStringAsync();
    var users = JsonSerializer.Deserialize<UserDto[]>(content, jsonOptions);

    if(users is null)
    {
        return [];
    }

    return users;
}
```
- Hier voeg je alle methodes toe die je nodig hebt voor calls naar de UserController van de API

### gebruiken van httpClient
- Installeer Microsoft.Extensions.Http met nuget in het Blazor project
- Maak een project reference naar het shared project van de solution
- Voeg de volgende code toe aan de program file van je blazor project
``` c# 
builder.Services.AddHttpClient(); 
builder.Services.AddScoped<UserHttpClient>();
```

- Voeg in de Home.razor pagina de volgende code toe
``` c#
@inject UserHttpClient UserHttpClient;

@using GraafschapLeenAuto.Shared.Clients;
```
- En zet in de OnInitializedAsync methode de volgende code
``` c#
Users = await UserHttpClient.GetUsers();
```

## CORS 
- In de API moet je CORS aanzetten om de Blazor app toegang te geven tot de API
``` c#
 services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

app.UseCors();
```

## Zonder authentication and authorization
- Voeg AllowAnonymous toe aan de getUsers methode in de UserController.
- We gebruiken namelijk nog geen authentication en authorization
