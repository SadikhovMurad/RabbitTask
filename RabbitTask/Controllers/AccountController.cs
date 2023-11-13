using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitTask.Identity;
using RabbitTask.Models;
using System.Text;
using System.Text.Json;

namespace RabbitTask.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;
        private IHttpContextAccessor httpContextAccessor;
        private IWebHostEnvironment webHost;
        //private IEmailSender _emailSender;

        //private readonly IStorageService _storageService;
        //private readonly ICloudinaryService _cloudService;
        private readonly IConfiguration _configuration;
        Uri coursephotouri = null;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHost, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.httpContextAccessor = httpContextAccessor;
            this.webHost = webHost;
            //_storageService = storageService;
            _configuration = configuration;
            //_cloudService = cloudService;
            //_emailSender = emailSender;
        }





        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            var role = await _userManager.GetRolesAsync(user);

            if (user == null)
            {
                ModelState.AddModelError("", "Hesap Yoxdu");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Username ve yaxud parol sehvdir");

            return View(model);
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model, IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (file != null)
            {

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Images", file.FileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // coursephotouri = await _cloudService.Upload(file);
            }

            var user = new User()
            {

                FirstName = model.FirstName,
                LastName = model.LastName,
                // UserName = model.UserName,
                Email = model.Email,
                // ImageUrl = coursephotouri.ToString()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail", "Account", new { UserId = user.Id, token = code });

                //await _emailSender.SendEmailAsync(model.Email, "Hesabi Dogrulayin", $" Linke tiklayin <a href='https://localhost:5107{url}'>Onayla</a>");

                //await _userManager.AddToRoleAsync(user, "Users");

                ConnectionFactory factory = new()
                {
                    HostName = "localhost"
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare("users", false, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, eventArgs) =>
                {
                    var body = eventArgs.Body.ToArray();
                    var msg = Encoding.UTF8.GetString(body);
                    Console.WriteLine(msg);
                };

                channel.BasicConsume(queue: "users", consumer: consumer);

                return RedirectToAction("Login", "Account");


            }


            ModelState.AddModelError("", "Error");

            //TempData.Add("messages", "Registration can not successfully");
            return View(model);

        }

        public async Task<IActionResult> ConfirmEmail(string UserId, string token)
        {
            if (UserId == null || token == null)
            {
                return View();
            }


            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {

                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return View();
                }

            }

            return View();
        }


        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Login");
        }
    }
}
