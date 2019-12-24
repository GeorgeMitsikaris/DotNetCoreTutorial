using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext dbContext;

        public SQLEmployeeRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public Employee Add(Employee employee)
        {
            dbContext.Add(employee);
            dbContext.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employeeToDelete = dbContext.Employees.Find(id);
            if (employeeToDelete != null)
            {
                dbContext.Employees.Remove(employeeToDelete);
                dbContext.SaveChanges();
            }
            return employeeToDelete;
        }

        public Employee GetEmployee(int id)
        {
            return dbContext.Employees.Find(id);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return dbContext.Employees;
        }

        public Employee Update(Employee employeeChanges)
        {
            var employee = dbContext.Employees.Attach(employeeChanges);
            employee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            dbContext.SaveChanges();
            return employeeChanges;
        }
    }
}
