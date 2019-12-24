using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Models
{
    public class InMemoryEmployeeRepository : IEmployeeRepository
    {
        List<Employee> _employeeList;

        public InMemoryEmployeeRepository()
        {
            _employeeList = new List<Employee>
            {
                new Employee{Id=1,Name="Mary",Email="mary@test.com",Department=Dept.HR},
                new Employee{Id=2,Name="John",Email="john@test.com",Department=Dept.IT},
                new Employee{Id=3,Name="Sam",Email="sam@test.com",Department=Dept.Administration}
            };
        }

        public Employee Add(Employee employee)
        {
            employee.Id = _employeeList.Max(e => e.Id) + 1;
            _employeeList.Add(employee);
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee employeeToDelete = _employeeList.FirstOrDefault(e => e.Id.Equals(id));
            if (employeeToDelete != null)
            {
                _employeeList.Remove(employeeToDelete);
            }
            return employeeToDelete;
        }

        public Employee GetEmployee(int id)
        {
            return _employeeList.FirstOrDefault(e => e.Id.Equals(id));
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _employeeList;
        }

        public Employee Update(Employee employeeChanges)
        {
            Employee employeeToUpdate = _employeeList.FirstOrDefault(e => e.Id.Equals(employeeChanges.Id));
            if (employeeToUpdate != null)
            {
                employeeToUpdate.Name = employeeChanges.Name;
                employeeToUpdate.Email = employeeChanges.Email;
                employeeToUpdate.Department = employeeChanges.Department;
            }
            return employeeToUpdate;
        }
    }
}
