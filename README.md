# GraafschapLeenAuto

## Toevoegen EF voor sqlite
- Package manager console 
- Let op juiste project '.api'
- Run het volgende command ```Install-Package Microsoft.EntityFrameworkCore.Sqlite```
    - Kan ook via Tools -> Nuget for solution en dan zoeken op Microsoft.EntityFrameworkCore
    - Hier zie je ook dat er andere database providers zijn
        - zoals SQL server
        - Wij gaan gebruik maken van sqlite 

## Aanmaken Model en dbcontext
``` c#
 public class LeenAutoDbContext : DbContext
 {
    public DbSet<User> Users { get; set; }

    public LeenAutoDbContext(DbContextOptions<LeenAutoDbContext> options)
         : base(options)
     {
     }

 }
```
- Uiteraard eerst map aanmaken waar de database context komt te staan
- Dan maken we een class met die de dbContext van EFcore erft
- Maak de constructor
- En voeg je Entiteiten toe aan de database set.



## Toevoegen aan Program 
``` c#
 // Add database context
 services.AddDbContext<LeenAutoDbContext>(options =>
 {
     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
 });
```
- Omdat ik telkens hetzelfde moet typen zet ik builder.Services in een services variable
- We gaan een service toevoegen die de db context toevoegd aan de applicatie
    - Als optie geven we aan dat we een sqlite database gaan gebruiken en we voegen vast de plek toe waar de connectie string komt te staan. (in de configuratie)
- Voeg de connectie string toe aan de appsettings
``` json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=leenauto.db;"
}
```

## Migrations
- Om de database te vullen maken we migraties aan deze kunnen we aanmaken met een andere package van Microsoft.EntityFrameWorkCore, namelijke de tools package. Met deze Package kun je migraties beheren en de database updaten 
``` Install-Package Microsoft.EntityFrameworkCore.Tools```
- Dan kunnen we meteen een migratie aan maken om de User eniteit op de database te plaatsen.
```Add Migration InitialCreate```

- Dit gaat fout, kan iemand vertellen waarom?
- We hebben de user entiteit nog niet behandeld als database entiteit.
- Eerst gaan we voor nieuwe entiteiten een folder maken.
- Voeg hieraan de User entiteit toe.
- Stel de primary key in en we kunnen aangeven dat de andere velden verplicht zijn
- We willen meerdere namen kunnen toevoegen, dus gebruiken we een Id als PK
``` c#
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
```
- zet de import van de User goed in de andere projecten.
- Probeer opnieuw de migratie aan te maken. 
- Zie dat er een nieuwe map is met Migraties.
    - Gegenereerd, een hoop werk
    - Je kunt dus nu code blijven schrijven en je hoeft de database niet te editen
- Als de Migratie zo ver is kun je de database updaten zodat deze migratie er op staat
``` Update-Database```
- Mooi de database met een tabel staat!

## Gebruik maken van de database
- Maak een service laag om het niet alles in je controllers te doen.
- Maak een UserService aan om met je db te communiceren.
- Schoon je Controller op.
- Maak endpoints voor Users
``` c# 
 public UserController()
 {
 }

 [HttpGet]
 public IActionResult GetUsers()
 {
    
     return Ok();
 }

 [HttpGet("{id}")]
 public IActionResult GetUser(int id)
 {
     return Ok();
 }

 [HttpPost]
 public IActionResult CreateUser([FromBody] User user)
 {
     return Ok();
 }

 [HttpPut("{id}")]
 public IActionResult UpdateUser(int id, [FromBody] User user)
 {
     return Ok();
 }
```

- Dan gaan we voor deze endpoints database calls maken in de UserService
- Daarvoor hebben we de DbContext weer nodig, dus die halen we op.
``` c#
private readonly LeenAutoDbContext dbContext;

public UserService(LeenAutoDbContext dbContext)
{
    this.dbContext = dbContext;
}
```
- Omdat we nu ook met Id's werken is het handig om deze toe te voegen aan de dto
- Maak de verschillende service handelingen voor de endpoints
``` c#
 public class UserService
 {
     private readonly LeenAutoDbContext dbContext;

     public UserService(LeenAutoDbContext dbContext)
     {
         this.dbContext = dbContext;
     }

     public IEnumerable<UserDto> GetUsers()
     {
         return dbContext.Users.Select(x => new UserDto
         {
             Id = x.Id,
             Name = x.Name,
             Email = x.Email
         });
     }

     public UserDto? GetUserById(int id)
     {
         var user = dbContext.Users.Find(id);

         if (user == null)
         {
             return null;
         }

         return new UserDto
         {
             Id = user.Id,
             Name = user.Name,
             Email = user.Email
         };
     }

     public UserDto CreateUser(User user)
     {
         dbContext.Users.Add(user);
         dbContext.SaveChanges();

         return new UserDto
         {
             Id = user.Id,
             Name = user.Name,
             Email = user.Email
         };
     }

     public UserDto? UpdateUser(int id, User user)
     {
         throw new NotImplementedException();
     }
 }
```

- In de controller voegen we de UserService toe.
``` c# 
private readonly UserService userService;

public UserController(UserService userService)
{
    this.userService = userService;
}
```
- Implement GetUsers and CreateUsers

## Dependancy injection
- Dan kunnen we proberen een User toe toegen, alleen krijgen we een error. Weet iemand waarom?
- We hebben hem nog niet geïnjecteerd. 
    - Dit doen we in de Program file.
    ```c# 
     // Add services
    services.AddTransient<UserService>();
    ``
- Probeer het nu opnieuw en het werkt.

Dependency injection zorgt voor losgekoppeld code wat je dan kan laten zien door niet de heletijd new new new new te doen.
```c# 
 this.userService = new UserService();
```
 
Daarnaast heb je dan de verschillende scopes voor een dependency ookwel dependency lifetimes:
 
Transient: Wordt altijd opnieuw aangemaakt kun je zien als, new();

Scoped: Per scope per request. Denk aan dbcontext die is scoped zodat we in meerdere classen dezelfde context kunnen gebruiken waardoor bijv de savechanges werkt als een unit of work.

Singleton: blijft de hele levensduur van de applicatie bestaan. En is voor iedereen hetzelfde vanaf het moment dat die geregistreerd is. Je zou daar bijv logging kunnen gebruiken, moet overal beschikbaar zijn en doet eigenlijk altijd hetzelfde.


# Afsluiten
In principe kun je nu alles al maken met uitzondering van de webapplicatie/ windows forms applicatie en authenticatie en authorisatie maar dat heb je in dit stadium nog niet nodig. Doormiddel van swagger kun je nu de gehele werking in de backend al programmeren zonder daar een interface aan te hangen.




## TODO
Code first

