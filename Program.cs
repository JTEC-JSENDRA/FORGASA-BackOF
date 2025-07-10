
// Importamos el espacio de nombres donde están definidos nuestros controladores de la API.
using API_SAP.Controllers;

// Creamos el "builder", que es el objeto que se encarga de configurar la aplicación.
var builder = WebApplication.CreateBuilder(args);

// Agregamos servicios al contenedor de dependencias.
// En este caso, agregamos el soporte para controladores (es decir, las rutas de la API).

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Estas líneas agregan y configuran Swagger/OpenAPI,
// una herramienta muy útil para probar y documentar la API automáticamente en el navegador.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Aquí construimos la aplicación usando toda la configuración que acabamos de hacer.
var app = builder.Build();

// Configuramos el "pipeline" de la aplicación, que es como una cadena de pasos
// que se ejecutan con cada solicitud HTTP que llega a la API.

// Si estamos en modo desarrollo (no producción), activamos Swagger
// para que podamos ver y probar la API desde una interfaz web.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirigimos automáticamente a HTTPS si alguien accede por HTTP.
// Esto es una medida de seguridad.
app.UseHttpsRedirection();

// Configuramos CORS (Cross-Origin Resource Sharing), lo cual es importante
// si vamos a acceder a la API desde otros dominios (por ejemplo, desde una app frontend).

app.UseCors(options =>
{
    options.AllowAnyOrigin()   // Permitir cualquier origen
           .AllowAnyMethod()   // Permitir cualquier tipo de método HTTP: GET, POST, etc.
           .AllowAnyHeader();  // Permitir cualquier encabezado HTTP.
});

// Habilitamos la autorización (aunque en este ejemplo no hay reglas definidas).
app.UseAuthorization();
// Le decimos a la app que busque los controladores definidos y los use como rutas.
app.MapControllers();
// Finalmente, ejecutamos la aplicación web.
app.Run();
