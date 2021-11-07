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




## Infrastructure
- Specific repository implementation 


