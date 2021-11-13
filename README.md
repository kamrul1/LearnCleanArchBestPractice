# Architecting ASP.NET Core Applications: Best Practices

These are my course notes following the [pluralsight course](https://app.pluralsight.com/library/courses/architecting-asp-dot-net-core-applications-best-practices/table-of-contents)


## Domain Project
 - Entities
 - Common

Here place the entities for ```Order.cs```, ```Event.cs``` and ```Category.cs``` - PCO objects



## Application
 - Contracts
   - Abstraction
   - Interfaces
 - Messaging
   - MediatR


Place ***generic*** interfaces in Application layer:
```csharp
public interface IAsyncRepository<T> where T: class 
{
    Task<T> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> ListAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);

}
```

Apply interfaces to entity classes, these are just contracts, no implementations, the implementation will be 
in the infrastructure project
```csharp
public interface IOrderRepository : IAsyncRepository<Order>
{
        
}

public interface ICategoryRepository : IAsyncRepository<Category>
{
        
}

public interface IEventRepository: IAsyncRepository<Event>
{
        
}
```
Nuget Packages
- Add AutoMapper, AutoMapper.Extensions.Microsoft.Dependency
- Add MediatR.Extensions.Microsoft.DependencyInjection


Create irequest to return view model:
```csharp
public class GetEventsListQuery: IRequest<List<EventListVm>>
{    
}
```

The view model will not be listing all the event properties but just those to be passed to users.
Implement the handler:
```csharp
public class GetEventsListQueryHandler : IRequestHandler<GetEventsListQuery, List<EventListVm>>
{
    public Task<List<EventListVm>> Handle(GetEventsListQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
```

Create ```MappingProfile.cs```profile for Automapper in Profiles folder, with ReverseMap so that it goes both ways.
```csharp
public class MappingProfile:Profile
{
    public MappingProfile()
    {
        CreateMap<EventArgs, EventListVm>().ReverseMap();
    }
}
```


--Create exention method for service, to for Automapper and MediatR registration
```csharp
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

### Adding simple CQRS 

Re-organise the Command Queries by Features e.g.
> Feature is a verticle slice of the functionality
> > Features folder and subfolders
> > Own the view models they will use, not shared even if identical

```
Features
    Categories
        Commands
        Queries
    Events
        Commands
        Queries
    Orders
        Commands
        Queries
      
```

IRequests such as ```UpdateEventCommand.cs``` that don't return anything use ```Task<Unit>``` in the handler return type:
```csharp
public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
{
    private readonly IMapper mapper;
    private readonly IEventRepository eventRepository;

    public UpdateEventCommandHandler(IMapper mapper, IEventRepository eventRepository)
    {
        this.mapper = mapper;
        this.eventRepository = eventRepository;
    }
    public async Task<Unit> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var eventToUpdate = await eventRepository.GetByIdAsync(request.EventId);
        mapper.Map(request, eventToUpdate, typeof(UpdateEventCommand), typeof(Event));

        await eventRepository.UpdateAsync(eventToUpdate);

        return Unit.Value;
    }
}
```

## Validation Rules on Event

While it is possible to annote the rules into the Enity class, this is ***not recommended***.  This is because they
are domain entities and validation should not be done by domain entities as here:


```csharp
public class Event: AuditableEntity
{
    public Guid EventId {get; set;}

    [Required]
    [StringLength(50)]
    public string Name {get; set;}
    public int Price {get; set;}

    [Required]
    [StringLength(50)]
    public string Artist {get; set;} 
}
```

Further not all business rule can be sorted by data annaotations.

#### Adding Fluent Validation 
Nuget ```FluentValidation.DependencyInjectionExtensions``` Package to the Application Package

Can be used in Core project
- RequestHandler
- Part of the feature folder

You create a custom validator inhereting from abstract class ```AbstractValidator```.  Here is an example for the
```CreateEventCommandValidator.cs```
```csharp
    public class CreateEventCommandValidator: AbstractValidator<CreateEventCommand>
    {
        private readonly IEventRepository eventRepository;

        public CreateEventCommandValidator(IEventRepository eventRepository)
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

            RuleFor(p => p.Date)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .GreaterThan(DateTime.Now);

            RuleFor(e => e)
                .MustAsync(EventNameAndDateUnique)
                .WithMessage("An event with the same name and date already exists");

            RuleFor(p => p.Price)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .GreaterThan(0);
            this.eventRepository = eventRepository;
        }

        private async Task<bool> EventNameAndDateUnique(CreateEventCommand e, CancellationToken token)
        {
            return !(await eventRepository.IsEventNameAndDateUnique(e.Name, e.Date));
        }
    }
```
You will notice, the ```EventNameAndDateUnique``` method, this something that data annotations can do in a nice to mantain way.



Add the rule into the handler before mapping the object:

```csharp
public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
{
    var validator = new CreateEventCommandValidator();
    var validationResult = await validator.ValidateAsync(request);

    //todo: in next section check validationResult for exceptions (see returning exception below)

    var @event = mapper.Map<Event>(request);
    @event = await eventRepository.AddAsync(@event);

    return @event.EventId;
}
```



#### Returning Exceptions
Core should return own set of exceptions
- Can be handled or transformed by consumer

Used Exceptions
- NotFoundException
- BadRequestException
- ValidationException

Create an Exceptions folder in the Application package and add custom exceptions
```
Exception <Folder>
    BadRequestException.cs
    NotFoundException.cs
    ValidationException.cs
```

BadRequestException example, Note ApplicationException is a built in exceptions class.  This would be used for null exception
```csharp
public class BadRequestException: ApplicationException
{
    public BadRequestException(string message): base(message)
    {

    }
        
}
```
NotFoundException example, 
```csharp
public class NotFoundException:ApplicationException
{
    public NotFoundException(string name, object key):base($"{name} ({key} is not found)")
    {
    }
}
````

IRequestHandler used exception ```ValidationException.cs``` class:
```csharp
public class ValidationException: ApplicationException
{
    public List<string> ValidationErrors { get; set; }

    public ValidationException(ValidationResult validationResult)
    {
        ValidationErrors = new List<string>();

        foreach(var valiationError in validationResult.Errors)
        {
            ValidationErrors.Add(valiationError.ErrorMessage);
        }

    }
}
```

Add this exception handing to our IRequest Handler example:

```csharp
public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
{
    var validator = new CreateEventCommandValidator();
    var validationResult = await validator.ValidateAsync(request);

    if (validationResult.Errors.Count > 0)
    {
        throw new ValidationException(validationResult);
    }

    var @event = mapper.Map<Event>(request);
    @event = await eventRepository.AddAsync(@event);

    return @event.EventId;
}
```

#### CommandResponse Class
Create a ```BaseResponse.cs``` for common responses in Responses folder at parent folder level of Application package.:
```csharp
public class BaseResponse
{
    public BaseResponse()
    {
        Success = true;
    }
    public BaseResponse(string message = null)
    {
        Success = true;
        Message = message;
    }

    public BaseResponse(string message, bool success)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; set; }
    public string Message { get; set; }
    public List<string> ValidationErrors { get; set; }

}
```
Use the base response to create a category response:
```csharp
public class CreateCategoryCommandResponse: BaseResponse
{
    public CreateCategoryCommandResponse() : base()
    {

    }

    public CreateCategoryDto Category { get; set; }
}

```
You will note the Category is included in the above response class aswell as success/failure from the ```BaseResponse.cs```.


Here is the completed ```CreateCategoryCommandHandler.cs```: 
```csharp
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryCommandResponse>
{
    private readonly IMapper mapper;
    private readonly ICategoryRepository categoryRepository;

    public CreateCategoryCommandHandler(IMapper mapper, ICategoryRepository categoryRepository)
    {
        this.mapper = mapper;
        this.categoryRepository = categoryRepository;
    }

    public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var createCategoryCommandResponse = new CreateCategoryCommandResponse();

        var validator = new CreateCategoryCommandValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Count > 0)
        {
            createCategoryCommandResponse.Success = false;
            createCategoryCommandResponse.ValidationErrors = new List<string>();
            foreach (var error in validationResult.Errors)
            {
                createCategoryCommandResponse.ValidationErrors.Add(error.ErrorMessage);
            }

        }

        if (createCategoryCommandResponse.Success)
        {
            var category = new Category { Name = request.Name };
            category = await categoryRepository.AddAsync(category);
            createCategoryCommandResponse.Category = mapper.Map<CreateCategoryDto>(category);
        }

        return createCategoryCommandResponse;

    }
}
```



## Infrastructure Project
This should contain the implementation of:

All external or I/O components
- Database
  - EF Core
  - DbContext
- Files
- Service Bus
- Service Client
- Logging



Add the nuget packages for EFCore:
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Design


Create ````GlobalTicketDbContext.cs```` class, with seed data.  Instead of using entity attribute to build database entity
use a Configurations classes to do it.  In the ```OnModelCreating(ModelBuilder modelBuilder)``` method add following:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(GloboTicketDbContext).Assembly);
```
This will cause it to search teh assembly for EventConfiguration to include for entity types.  
For example for ```Event.cs```

```csharp
public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
    }
}
```

Also, note when saving, tracker information is added in the override method of ````GlobalTicketDbContext.cs````
```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
{
    foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedDate = DateTime.Now;
                break;
            case EntityState.Modified:
                entry.Entity.LastModifiedDate = DateTime.Now;
                break;
        }
    }
    return base.SaveChangesAsync(cancellationToken);
}
```
 ###Repositories folder

The implementation of IAsyncRepository\<T> is in the BaseRepository\<T>, The generic T allows for any 
entity to be specified.

It is important to make the context protected, as this class will be implemented by children classes:
```csharp
protected readonly GloboTicketDbContext dbContext;
```

### Adding Support for Email

>New requirement to send an email on creating an event

Create a contract for IEmailService in the Application package.  As it's not presistance, add it to Mail Folder.
```csharp
public interface IEmailService
{
    Task<bool> SendEmail(Email email);
}
```
Create a Models folder in the parent Application package. Place Mail folder and ```Email.cs``` and ```EmailSetting.cs``` classes in it.

In the ```CreateEventCommandHandler.cs```, inject the IEmailService in the contractor and in the handle method send the email. 

```csharp
var email = new Email
{
    To="gill@snowball.be", Body=$"A new event was created {request}", Subject="A new event was created"
};

try
{
    await emailService.SendEmail(email);
}
catch (Exception ex)
{

    //this shouldn't stop the API from doing anything else so that can be logged
}
```
Note there is no implementation of the service here just us of the contract/interface.
In this project were are using [SendGrid](https://app.sendgrid.com/) to send the email. So you'll need to register and get an API from the service.
Here is the implementation of the service in the infrastructure project:
```csharp
public class EmailService : IEmailService
{
    public EmailSettings emailSettings { get; set; }
    public EmailService(IOptions<EmailSettings> mailSettings)
    {
        this.emailSettings = mailSettings.Value;
    }

    public async Task<bool> SendEmail(Email email)
    {
        var client = new SendGridClient(emailSettings.APiKey);

        var subject = email.Subject;
        var to = new EmailAddress(email.To);
        var emailBody = email.Body;

        var from = new EmailAddress
        {
            Email = emailSettings.FromAddress,
            Name = emailSettings.FromName
        };

        var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);
        var response = await client.SendEmailAsync(sendGridMessage);

        if(response.StatusCode==System.Net.HttpStatusCode.Accepted 
            || response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }
}
```
----

## Adding the API

Create a SendGrid Account

Use [Secret Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows) 
to hid the ApiKey secret in the appsetting.json

Navigate to the API project folder in terminal, the location of the appsettings.json and run:
```cmd
dotnet user-secrets init
```
Hide the appsettings element with:
```cmd
dotnet user-secrets set "EmailSettings:ApiKey" "YOUR_REAL_KEY_GOES_HERE"
```
In the appsetting, leave the key vaue empty:
```json
  "EmailSettings": {
    "FromAddress": "gill@test.com",
    "ApiKey": "",
    "FromName": "Gill"
  }
```

### Setup ```Startup.cs```

Add the service entensions for dependency injections:
```csharp
services.AddApplicationServices();
services.AddInfrastructureService(Configuration);
services.AddPresistenceServices(Configuration);
```

Add cross origin support:
```csharp
services.AddCors(options =>
{
    options.AddPolicy(
        "Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
        ); 
});
```

### Create migrations for DbContext

Set the API project as the startup project, in Package Manager Console, 
select the GlobalTicket.TicketManagement.Persistence package and run:
```powershell
PM> add-migration InitialMigration
```
This will now create a migrations folder in the GlobalTicket.TicketManagement.Persistence project
Add the changes to the database connection string defined in the appsetting.json, by running:
```powershell
update-database
```

### Not using the simple but Heavy Controller

Typically these things are carried out in a controller:
- Validate incoming data using model binding
- Execute logic
- Create resposne type
- Return status code with response

A better approach is to make the contoller lighter using a view service:
- Logic is moved to a separate class
- Called from the contoller
- Views service connects with business logic
- Returns DTO
- Returns a more lightweight controllers

An even more better option is to use MediatR, as below:
```csharp
[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator mediator;

    public CategoryController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet("all", Name ="GetAllCategories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryListVm>>> GetAllCategories()
    {
        var dto = await mediator.Send(new GetCategoriesListQuery());
        return Ok(dto); 
    } 

    [HttpGet("allwithevents", Name ="GetCategoriesWithEvents")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryEventListVm>>> GetCategoriesWithEvents(bool includeHistory)
    {
        var getCategoriesListWithEventsQuery = new GetCategoriesListWithEventsQuery() { IncludeHistory = includeHistory };
        var dtos = await mediator.Send(getCategoriesListWithEventsQuery);

        return Ok(dtos);
    }


    [HttpPost(Name ="AddCategory")]
    public async Task<ActionResult<CreateCategoryCommandResponse>> Create([FromBody] CreateCategoryCommand createCategoryCommand)
    {
        var response = await mediator.Send(createCategoryCommand);
        return Ok(response); 
    }

```

### Deciding Objects to Return

A lot of times to much data is being send back by way of properties.  It is better to use a 
```BaseResponse.cs``` class.


## Adding support for CSV file

Contract in Application project
Implementation in the Infrastructure project

Add the handler and contract for ICsvExporter in the application project:
```csharp
public class GetEventsExportQueryHandler: IRequestHandler<GetEventsExportQuery, EventExportFileVm>
{
    private readonly IMapper mapper;
    private readonly IAsyncRepository<Event> eventRepository;
    private readonly ICsvExporter csvExporter;

    public GetEventsExportQueryHandler(IMapper mapper, IAsyncRepository<Event> eventRepository, ICsvExporter csvExporter)
    {
        this.mapper = mapper;
        this.eventRepository = eventRepository;
        this.csvExporter = csvExporter;
    }

    public async Task<EventExportFileVm> Handle(GetEventsExportQuery request, CancellationToken cancellationToken)
    {
        var allEvents = mapper.Map<List<EventExportDto>>((await eventRepository.ListAllAsync()).OrderBy(x => x.Date));
        var fileData = csvExporter.ExportEventsToCsv(allEvents);

        var eventExportFileDto = new EventExportFileVm() { ContentType = "text/csv", Data = fileData, EventExportFileName = $"{Guid.NewGuid()}.csv" };

        return eventExportFileDto;

            
    }
}
```

Here is also the code for the ```CSVExporter.cs```, in the infrastructure project:
```csharp
    public class CsvExporter : ICsvExporter
    {
        public byte[] ExportEventsToCsv(List<EventExportDto> eventExportDtos)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using var csvWriter = new CsvWriter(streamWriter, CultureInfo.CurrentCulture);
                csvWriter.WriteRecords(eventExportDtos);
            }
            return memoryStream.ToArray();
        }
    }
```
Add support for file to Swagger:
```csharp
c.MapType<FileContentResult>(() => new OpenApiSchema { Type = "file" });
```

## Testing the application code

Three types of tests:
- Unit Test
- Integration Test
- Functional Test

### Unit Test

> Is a automated test that will check behaviour of a unit of code.

### Integration Test

Test infrastructure code
Interaction between different layers

### Functional Test

> Test behaviour from the users perspective, often involves UI Testing























 
















