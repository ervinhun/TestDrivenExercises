using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using tests.Data;
using tests.DTOs;
using tests.Entities;
using tests.Exercises;
using tests.Interfaces;

namespace tests.Tests;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<CompanyDbContext>(factory =>
        {
            var postgreSqlContainer = new PostgreSqlBuilder().Build();
            postgreSqlContainer.StartAsync().GetAwaiter().GetResult();
            var connectionString = postgreSqlContainer.GetConnectionString();
            var options = new DbContextOptionsBuilder<CompanyDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var ctx = new CompanyDbContext(options);
            ctx.Database.EnsureCreated();
            SeedData.SeedAsync(ctx).GetAwaiter().GetResult();
            return ctx;
        });
        services.AddScoped<IEfExercises, EfExercises>();
        services.AddScoped<IEfExercisesIdempotentUpdates, EfExercisesIdempotentUpdates>();
    }

    public class EfExercisesIdempotentUpdates(CompanyDbContext context) : IEfExercisesIdempotentUpdates
    {
        public Task<Employee> UpdateEmployee(UpdateEmployeeDto dto)
        {
            var employeeToUpdate = context.Employees.FirstOrDefault(e => e.Id == dto.Id) ??
                                   throw new ValidationException("Employee not found with id: '" + dto.Id + "'");
            employeeToUpdate.FirstName = dto.FirstName;
            employeeToUpdate.LastName = dto.LastName;
            employeeToUpdate.Email = dto.Email;
            employeeToUpdate.Salary = dto.Salary;
            employeeToUpdate.HireDate = dto.HireDate;
            employeeToUpdate.DepartmentId = dto.DepartmentId;

            var projects = context.Projects.Where(p => dto.ProjectIds.Contains(p.Id)).ToList();
            if (dto.ProjectIds.Count != projects.Count)
                throw new ValidationException("One or more projects not found");

            employeeToUpdate.Projects.Clear();
            foreach (var project in projects)
            {
                employeeToUpdate.Projects.Add(project);
            }
            
            employeeToUpdate.Department = context.Departments.FirstOrDefault(d => d.Id == dto.DepartmentId) ??
                                          throw new ValidationException("Department not found with id: '" +
                                                                        dto.DepartmentId + "'");

            context.SaveChanges();
            return Task.FromResult(employeeToUpdate);
        }

        public Task<Project> UpdateProject(UpdateProjectDto dto)
        {
            var projectToUpdate = context.Projects.FirstOrDefault(p => p.Id.Equals(dto.Id)) ??
                                  throw new ValidationException("Project could not be found with id: '" + dto.Id + "'");
            projectToUpdate.Budget = dto.Budget;
            projectToUpdate.Description = dto.Description;
            projectToUpdate.Name = dto.Name;
            projectToUpdate.EndDate = dto.EndDate;
            projectToUpdate.StartDate = dto.StartDate;

            var employees = context.Employees.Where(e => dto.EmployeeIds.Contains(e.Id)).ToList();
            if (dto.EmployeeIds.Count != employees.Count)
                throw new ValidationException("One or more employees not found");
            projectToUpdate.Employees.Clear();
            foreach (var employee in employees)
            {
                projectToUpdate.Employees.Add(employee);
            }

            context.SaveChanges();
            return Task.FromResult(projectToUpdate);
        }

        public Task<Department> UpdateDepartment(UpdateDepartmentDto dto)
        {
            var departmentToUpdate = context.Departments.FirstOrDefault(d => d.Id.Equals(dto.Id)) ??
                                     throw new ValidationException("Department could not be found with id: '" + dto.Id +
                                                                   "'");

            departmentToUpdate.Budget = dto.Budget;
            departmentToUpdate.Name = dto.Name;
            departmentToUpdate.Location = dto.Location;

            var employees = context.Employees.Where(e => dto.EmployeeIds.Contains(e.Id)).ToList();
            if (dto.EmployeeIds.Count != employees.Count)
                throw new ValidationException("One or more employees not found");
            departmentToUpdate.Employees.Clear();
            foreach (var employee in employees)
            {
                departmentToUpdate.Employees.Add(employee);
            }
            
            context.SaveChanges();
            return Task.FromResult(departmentToUpdate);
        }
    }
}