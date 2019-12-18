using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        List<Employee> _empmloyeeList;

        public MockEmployeeRepository()
        {
            _empmloyeeList = new List<Employee>
            {
                new Employee{Id=1,Name="Mary",Email="mary@test.com",Department=Dept.HR},
                new Employee{Id=2,Name="John",Email="john@test.com",Department=Dept.IT},
                new Employee{Id=3,Name="Sam",Email="sam@test.com",Department=Dept.Administration}
            };
        }

        public Employee GetEmployee(int id)
        {
            return _empmloyeeList.FirstOrDefault(e => e.Id.Equals(id));
        }

        public IEnumerable<Employee> GetEmployees()
        {
            return _empmloyeeList;
        }
    }
}
