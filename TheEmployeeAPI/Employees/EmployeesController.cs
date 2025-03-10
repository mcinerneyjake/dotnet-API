using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TheEmployeeAPI.Abstractions;

namespace TheEmployeeAPI.employees;

public class EmployeesController : BaseController
{
  private readonly IRepository<Employee> _repository;
  
  private readonly ILogger<EmployeesController> _logger;

  public EmployeesController(
    IRepository<Employee> repository,
    ILogger<EmployeesController> logger
  )
  {
    _repository = repository;
    this._logger = logger;
  }

  /// <summary>
  /// Gets all of the employees in the system.
  /// </summary>
  /// <returns>Returns the employee in a JSON array.</returns>
  [HttpGet]
  [ProducesResponseType(typeof(IEnumerable<GetEmployeeRequest>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public IActionResult GetAllEmployees()
  {
    var employees = _repository.GetAll().Select(EmployeeToGetEmployeeRequest);

    return Ok(employees);
  }

  /// <summary>
  /// Gets an employee by ID.
  /// </summary>
  /// <param name="id">The ID of the employee.</param>
  /// <returns>The single employee record.</returns>
  [HttpGet("{id:int}")]
  [ProducesResponseType(typeof(GetEmployeeRequest), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public IActionResult GetEmployeeById(int id)
  {
    var employee = _repository.GetById(id);
    if (employee == null)
    {
      return NotFound();
    }

    var employees = EmployeeToGetEmployeeRequest(employee);

    return Ok(employees);
  }

  /// <summary>
  /// Creates a new employee.
  /// </summary>
  /// <param name="employeeRequest">The employee to be created.</param>
  /// <returns>A link to the employee that was created.</returns>
  [HttpPost]
  [ProducesResponseType(typeof(GetEmployeeRequest), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest employeeRequest)
  {
    await Task.CompletedTask;
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


  /// <summary>
  /// Updates an employee.
  /// </summary>
  /// <param name="id">The ID of the employee to update.</param>
  /// <param name="employeeRequest">The employee data to update.</param>
  /// <returns></returns>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(GetEmployeeRequest), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public IActionResult UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest employeeRequest)
  {
    _logger.LogInformation("Updating employee with ID: {EmployeeId}", id);

    var existingEmployee = _repository.GetById(id);
    if (existingEmployee == null)
    {
      _logger.LogWarning("Employee with ID: {EmployeeId} not found", id);

      return NotFound();
    }

    _logger.LogDebug("Updating employee details for ID: {EmployeeId}", id);
    existingEmployee.Address1 = employeeRequest.Address1;
    existingEmployee.Address2 = employeeRequest.Address2;
    existingEmployee.City = employeeRequest.City;
    existingEmployee.State = employeeRequest.State;
    existingEmployee.ZipCode = employeeRequest.ZipCode;
    existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
    existingEmployee.Email = employeeRequest.Email;

    try
    {
      _repository.Update(existingEmployee);
      _logger.LogInformation("Employee with ID: {EmployeeId} successfully updated", id);
      return Ok(existingEmployee);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred while updating employee with ID: {EmployeeId}", id);
      return StatusCode(500, "An error occurred while updating the employee");
    }
  }

  /// <summary>
  /// Gets the benefits for an employee.
  /// </summary>
  /// <param name="employeeId">The ID to get the benefits for.</param>
  /// <returns>The benefits for that employee.</returns>
  [HttpGet("{employeeId}/benefits")]
  [ProducesResponseType(typeof(IEnumerable<GetEmployeeRequestEmployeeBenefit>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public IActionResult GetBenefitsForEmployee(int employeeId)
  {
    var employee = _repository.GetById(employeeId);
    if (employee == null)
    {
      return NotFound();
    }
    return Ok(employee.Benefits.Select(BenefitToBenefitResponse));
  }

  private GetEmployeeRequest EmployeeToGetEmployeeRequest(Employee employee)
  {
    return new GetEmployeeRequest
    {
      FirstName = employee.FirstName,
      LastName = employee.LastName,
      Address1 = employee.Address1,
      Address2 = employee.Address2,
      City = employee.City,
      State = employee.State,
      ZipCode = employee.ZipCode,
      PhoneNumber = employee.PhoneNumber,
      Email = employee.Email,
      Benefits = employee.Benefits.Select(benefit => new GetEmployeeRequestEmployeeBenefit
      {
        Id = benefit.Id,
        EmployeeId = benefit.EmployeeId,
        BenefitType = benefit.BenefitType,
        Cost = benefit.Cost
      }).ToList()
    };
  }

  private static GetEmployeeRequestEmployeeBenefit BenefitToBenefitResponse(EmployeeBenefits benefit)
  {
    return new GetEmployeeRequestEmployeeBenefit
    {
      Id = benefit.Id,
      EmployeeId = benefit.EmployeeId,
      BenefitType = benefit.BenefitType,
      Cost = benefit.Cost
    };
  }
}