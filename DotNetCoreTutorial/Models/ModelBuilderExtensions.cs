using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Models
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    Id = 1,
                    Name = "George",
                    Email = "george@test.com",
                    Department = Dept.IT
                },
                new Employee
                {
                    Id = 2,
                    Name = "Mark",
                    Email = "mark@test.com",
                    Department = Dept.Administration                }
            );
        }
    }
}
