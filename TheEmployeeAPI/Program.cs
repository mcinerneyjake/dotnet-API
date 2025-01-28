using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TheEmployeeAPI;
using TheEmployeeAPI.Abstractions;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRepository<Employee>, EmployeeRepository>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

var employeeRoute = app.MapGroup("employees");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

employeeRoute.MapGet(string.Empty, (IRepository<Employee> repository) => {
  return Results.Ok(repository.GetAll().Select(employee => new GetEmployeeRequest {
    FirstName = employee.FirstName,
    LastName = employee.LastName,
    Address1 = employee.Address1,
    Address2 = employee.Address2,
    City = employee.City,
    State = employee.State,
    ZipCode = employee.ZipCode,
    PhoneNumber = employee.PhoneNumber,
    Email = employee.Email
  }));
});

employeeRoute.MapGet("{id:int}", (int id, IRepository<Employee> repository) => {
  var employee = repository.GetById(id);
  if (employee == null)
  {
    return Results.NotFound();
  }

  return Results.Ok(new GetEmployeeRequest {
    FirstName = employee.FirstName,
    LastName = employee.LastName,
    Address1 = employee.Address1,
    Address2 = employee.Address2,
    City = employee.City,
    State = employee.State,
    ZipCode = employee.ZipCode,
    PhoneNumber = employee.PhoneNumber,
    Email = employee.Email
  });
});

employeeRoute.MapPost(string.Empty,
async (
  CreateEmployeeRequest employeeRequest,
  IRepository<Employee> repository,
  IValidator<CreateEmployeeRequest> validator
) =>
{
  var validationResults = await validator.ValidateAsync(employeeRequest);
    if (!validationResults.IsValid)
    {
      return Results.ValidationProblem(validationResults.ToDictionary());
    }

  var newEmployee = new Employee
  {
    FirstName = employeeRequest.FirstName!,
    LastName = employeeRequest.LastName!,
    SocialSecurityNumber = employeeRequest.SocialSecurityNumber,
    Address1 = employeeRequest.Address1,
    Address2 = employeeRequest.Address2,
    City = employeeRequest.City,
    State = employeeRequest.State,
    ZipCode = employeeRequest.ZipCode,
    PhoneNumber = employeeRequest.PhoneNumber,
    Email = employeeRequest.Email
  };
  repository.Create(newEmployee);
  return Results.Created($"/employees/{newEmployee.Id}", employeeRequest);
});

employeeRoute.MapPut("{id}", ([FromBody] UpdateEmployeeRequest employeeRequest, int id, IRepository<Employee> repository) => {
  var existingEmployee = repository.GetById(id);
  if (existingEmployee == null)
  {
    return Results.NotFound();
  }

  existingEmployee.Address1 = employeeRequest.Address1;
  existingEmployee.Address2 = employeeRequest.Address2;
  existingEmployee.City = employeeRequest.City;
  existingEmployee.State = employeeRequest.State;
  existingEmployee.ZipCode = employeeRequest.ZipCode;
  existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
  existingEmployee.Email = employeeRequest.Email;

  repository.Update(existingEmployee);
  return Results.Ok(existingEmployee);
});

app.Run();


public partial class Program
{

}