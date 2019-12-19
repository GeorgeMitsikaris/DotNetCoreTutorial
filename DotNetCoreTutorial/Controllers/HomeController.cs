using DotNetCoreTutorial.Models;
using DotNetCoreTutorial.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreTutorial.Controllers
{
    public class HomeController : Controller
    {
        private IEmployeeRepository employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository;
        }

        public ViewResult Index()
        {
            var model = employeeRepository.GetEmployees();
            return View(model);
        }

        public ViewResult Details(int id)
        {
            var model = new HomeDetailsViewModel();
            model.PageTitle = "Employee Details";
            model.Employee = employeeRepository.GetEmployee(id);
            return View(model);
        }

        [HttpGet]
        public ViewResult Create()
        {
            return View();
        }

        [HttpPost]
        public RedirectToActionResult Create(Employee employee)
        {
            employeeRepository.Add(employee);
            return RedirectToAction("Details", new { id = employee.Id });
        }
    }
}
