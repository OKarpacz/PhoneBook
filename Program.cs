using Microsoft.EntityFrameworkCore;
using PhoneBook.Data;
using PhoneBook.Services;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
builder.Services.AddRazorPages(); 
builder.Services.AddControllers();
builder.Services.AddDbContext<PhoneBookContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ContactService>();
builder.Services.AddHttpClient<OpenAiService>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();