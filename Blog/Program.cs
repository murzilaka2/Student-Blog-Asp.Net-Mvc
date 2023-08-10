using Blog.Data;
using Blog.Interfaces;
using Blog.Models;
using Blog.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //Подключаем базу и Identity
            IConfigurationRoot _confString = new ConfigurationBuilder().
            SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();

            builder.Services.AddDbContext<ApplicationContext>(options =>
                           options.UseSqlServer(_confString.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>(opts =>
            {
                opts.Password.RequiredLength = 5;   // минимальная длина
                opts.Password.RequireNonAlphanumeric = false;   // требуются ли не алфавитно-цифровые символы
                opts.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
                opts.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
                opts.Password.RequireDigit = false; // требуются ли цифры
            })
            .AddEntityFrameworkStores<ApplicationContext>();

            //Подключаем базу и Identity Конец

            builder.Services.AddTransient<IMembership, MembershipRepository>();
            builder.Services.AddTransient<ICategory, CategoryRepository>();
            builder.Services.AddTransient<IPublication, PublicationRepository>();
            builder.Services.AddTransient<ISubscriber, SubscriberRepository>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await RoleInitializer.InitializeAsync(userManager, rolesManager);

                    var applicationContext = services.GetRequiredService<ApplicationContext>();
                    await ContentInitializer.InitializeAsync(applicationContext);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();  //подключение аутентификации
            app.UseAuthorization();  //подключение авторизации

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }
    }
}