using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

// /register?login=testeruser2&password=qwerty&email=asd@qwe.qw
// /login?login=testeruser2&password=qwerty

namespace WalrusMessenger
{
    public partial class Program
    {
        static readonly private RSACryptoServiceProvider RSAKeys = new(2048);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connection = builder.Configuration.GetConnectionString("DefaultConnection")!;

            builder.Services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opts =>
                opts.LoginPath = "/login");
            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<AppContext>(opts => opts.UseSqlServer(connection));
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();
            builder.Services.AddCors(opts =>
            {
                opts.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("https://localhost:7064", "http://localhost:5131")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            builder.Services.AddSignalR();

            var app = builder.Build();

            app.UseCors();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();

            app.MapHub<MessageHub>("/messagehub");
            

            app.Use(async (context, next) =>
            {
                await next.Invoke();
                foreach (var e in context.Response.Headers)
                    Console.WriteLine(e.Key + " - " + e.Value);
            });

            app.Map("/register", async (string login, string password, string email, AppContext db, HttpContext context) => //POST
            {
                if (!(LoginIsValid(login))) return Results.BadRequest("Invalid login");
                if (!EmailRegex().IsMatch(email)) return Results.BadRequest("Invalid email");

                var user = db.Users.FirstOrDefault(x => x.Login == login);

                if (user is not null) return Results.Conflict("Login taken");

                user = new User(login, login, email, new List<long>(), new List<long>(), null, null, password, DateTime.Now, true);
                db.Users.Attach(user);
                db.SaveChanges();

                var claims = new List<Claim> { new(ClaimTypes.Name, user.Login) };
                var identity = new ClaimsIdentity(claims, "Cookies");
                await context.SignInAsync("Cookies", new ClaimsPrincipal(identity));

                return Results.Json(user);
            });

            app.Map("/login", async (string login, string password, AppContext db, HttpContext context) => //GET
            {
                var user = db.Users.FirstOrDefault(x => x.Login == login);
                if (user is null) return Results.NotFound("null");
                if (user.Password != password) return Results.Unauthorized();

                var claims = new List<Claim> { new(ClaimTypes.Name, user.Login) };
                var identity = new ClaimsIdentity(claims, "Cookies");
                await context.SignInAsync("Cookies", new ClaimsPrincipal(identity));

                user.IsOnline = true;
                db.Users.Entry(user).State = EntityState.Modified;
                db.SaveChanges();

                Console.WriteLine(login);
                var j = Results.Json(user, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return j;
            });

            app.MapGet("/getuser", async (long userId, AppContext db) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
                if (user is null) return Results.NotFound("null");
                return Results.Json(user);
            });

            app.Map("/logout", [Authorize] async (long id, AppContext db, HttpContext context) => //PUT
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var user = db.Users.Find(id);
                if (user is not null)
                {
                    user.LastLogin = DateTime.Now;
                    user.IsOnline = false;
                    db.Users.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Results.Json(user);
            });

            app.Map("/search", [Authorize] (string prompt, long user, AppContext db, HttpContext context) => //GET
            {
                var users = db.Users.Where(x => (x.Name.Contains(prompt) || x.Login.Contains(prompt))
                    && x.Id != user);
                return Results.Json(users);
            });

            app.MapGet("/getkey", [Authorize] (HttpContext context) =>
            {
                return Results.Ok(RSAKeys.ExportRSAPublicKey());
            });

            app.MapPost("/createprivatechat", [Authorize] (long id1, long id2, AppContext db) =>
            {
                var user1 = db.Users.SingleOrDefault(x => x.Id == id1);
                var user2 = db.Users.SingleOrDefault(y => y.Id == id2);
                if (user1 is null || user2 is null) return Results.NotFound();
                var chat = new PrivateChat(id1, id2);
                db.PrivateChats.Add(chat);
                db.SaveChanges();
                var chatId = db.PrivateChats.Entry(chat).Entity.Id;
                user1.Chats.Add(chatId);
                user2.Chats.Add(chatId);
                db.Users.Entry(user1).State = EntityState.Modified;
                db.Users.Entry(user2).State = EntityState.Modified;
                db.SaveChanges();
                return Results.Json(chat);
            });
            
            app.MapPost("/sendmessage/touser", [Authorize] (long sender, long reciever, string message, AppContext db, 
                IHubContext<MessageHub> hub) =>
            {
                var chat = db.PrivateChats.Where(x => x.Id == reciever).FirstOrDefault();
                if (chat is null) return Results.NotFound();
                var m = new Message(DateTime.Now, message, sender, reciever)
                {
                    Chat = chat
                };
                m.ReadBy.Add(sender);
                chat.AddMessage(m);
                db.PrivateChats.Entry(chat).State = EntityState.Modified;
                db.SaveChanges();
                
                hub.Clients.All.SendAsync("RecieveMessage", chat.Id, m);
                return Results.Json(m);
            });

            app.MapGet("/getmessages/fromuser", [Authorize] (long id, int count, int skip, AppContext db, HttpContext context) =>
            {
                var chat = db.PrivateChats.SingleOrDefault(x => x.Id == id);
                if (chat is null) return Results.NotFound();
                //count = Math.Min(count, chat.Messages.Count - skip);
                try
                {
                    return Results.Json(chat.GetMessages(db).Skip(skip).Take(count));
                }
                catch (ArgumentNullException)
                {
                    return Results.BadRequest();
                }
            });

            app.MapGet("/getprivatechat", [Authorize] (long id1, long id2, AppContext db) =>
            {
                var chat = db.PrivateChats.FirstOrDefault(x => x.Users.Count == 2 &&
                    x.Users.Contains(id1) && x.Users.Contains(id2));
                if (chat is null) return Results.NotFound();
                return Results.Json(chat);
            });

            app.MapGet("/getchats", [Authorize] (long userId, AppContext db) =>
            {
                var user = db.Users.FirstOrDefault(x => x.Id == userId);
                if (user is null) return Results.NotFound();
                var chats = db.Chats.Where(x => x.Id != 0 && user.Chats.Contains(x.Id)).ToArray();
                return Results.Json(chats);
            });

            app.MapGet("/getchat", [Authorize] (long id1, long id2, AppContext db) => 
            {
                var user1 = db.Users.FirstOrDefault(x => x.Id == id1);
                var user2 = db.Users.FirstOrDefault(x => x.Id == id2);
                if (user1 is null || user2 is null) return Results.NotFound();
                var chat = db.Chats.FirstOrDefault(x => user1.Chats.Contains(x.Id) && user2.Chats.Contains(x.Id));
                if (chat is null) return Results.NotFound();
                return Results.Json(chat);
            });

            app.Run();
        }

        static bool LoginIsValid(string login)
        {
            return login.Length > 3 && login.Length < 25 && login.All(e => char.IsLetterOrDigit(e) || e == '_');
        }

        [GeneratedRegex("\\w+@\\w+\\.\\w+")]
        private static partial Regex EmailRegex();
    }
}