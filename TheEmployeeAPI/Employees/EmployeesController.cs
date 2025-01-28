using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TheEmployeeAPI.Abstractions;

namespace TheEmployeeAPI.employees;

public class EmployeesController : BaseController
{
    private readonly IRepository<Employee> _repository;

    public EmployeesController(IRepository<Employee> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult GetAllEmployees()
    {
      return Ok(_repository.GetAll().Select(employee => new GetEmployeeRequest {
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
    }

    [HttpGet("{id:int}")]
    public IActionResult GetEmployeeById(int id)
    {
      var employee = _repository.GetById(id);
      if (employee == null)
      {
        return NotFound();
      }

      return Ok(new GetEmployeeRequest {
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
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest employeeRequest)
    {
      var validationResults = await ValidateAsync(employeeRequest);
      if (!validationResults.IsValid)
      {
        return BadRequest(validationResults.ToModelStateDictionary());
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
      _repository.Create(newEmployee);
      return Created($"/employees/{newEmployee.Id}", employeeRequest);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest employeeRequest)
    {
      var existingEmployee = _repository.GetById(id);
      if (existingEmployee == null)
      {
        return NotFound();
      }

      existingEmployee.Address1 = employeeRequest.Address1;
      existingEmployee.Address2 = employeeRequest.Address2;
      existingEmployee.City = employeeRequest.City;
      existingEmployee.State = employeeRequest.State;
      existingEmployee.ZipCode = employeeRequest.ZipCode;
      existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
      existingEmployee.Email = employeeRequest.Email;

      _repository.Update(existingEmployee);
      return Ok(existingEmployee);
    }
}