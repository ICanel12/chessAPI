using Autofac;
using Autofac.Extensions.DependencyInjection;
using chessAPI.dataAccess.providers.postgreSQL;
using chessAPI;
using chessAPI.business.interfaces;
using chessAPI.models.player;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Serilog.Events;


//Serilog logger (https://github.com/serilog/serilog-aspnetcore)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("chessAPI starting");
    var builder = WebApplication.CreateBuilder(args);

    var connectionStrings = new connectionStrings();
    builder.Services.AddOptions();
    builder.Services.Configure<connectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
    builder.Configuration.GetSection("ConnectionStrings").Bind(connectionStrings);

    // Two-stage initialization (https://github.com/serilog/serilog-aspnetcore)
    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom
             .Configuration(context.Configuration)
             .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning).ReadFrom
             .Services(services).Enrich
             .FromLogContext().WriteTo
             .Console());

    // Autofac como inyección de dependencias
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
    builder.Host.ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule(new chessAPI.dependencyInjection<int, int>()));
    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseMiddleware(typeof(chessAPI.customMiddleware<int>));
    app.MapGet("/", () =>
    {
        return "hola mundo";
    });

    app.MapPost("player", 
    [AllowAnonymous] async(IPlayerBusiness<int> bs, clsNewPlayer newPlayer) => Results.Ok(await bs.addPlayer(newPlayer)));

    app.MapPost("/jugador", (clsNewPlayer newPlayer)=> {
        var query = "CREATE TABLE player(id_player int primary key default as identity,email varchar(50) not null);" +                             
                    "INSERT INTO player (email) values('@email');";
        var petition = query.Replace("@email",newPlayer.email);            
        Conexion jugador = new Conexion();
        jugador.ExecuteNonQuery(petition);
    });

    app.MapPost("/juego", (clsNewGame newGame)=> {
        var petition = "CREATE TABLE game(id_game int as identity, firstplayerscore int, secondplayerscore int, id_firstplayer int, id_secondplayer int, constraint fk_gamefp foreign key (id_firstplayer) references player(id_player),constraint fk_gamesp foreign key (id_secondplayer) references player(id_player));" +                             
                    "insert into game(firstplayerscore,secondplayerscore,id_firstplayer,id_secondplayer) values (0,0,@idOne,@idTwo);";    
        var query = petition.Replace("@idOne",Convert.ToString(newGame.id_player_One)); 
        query = query.Replace("@idTwo",Convert.ToString(newGame.id_player_Two));            
        GenerateConnection generarJugador = new GenerateConnection();
        generarJugador.ExecuteNonQuery(query);
    });

    app.MapGet("/obtenerJugador", () =>
    {
        clsGenerateConnection obtenerJugadores = new clsGenerateConnection();
        return obtenerJugadores.ExecuteQuery("select * from player");
    });

    app.MapGet("/obtenerJuegos", () =>
    {
        clsGenerateConnection obtenerJuegos = new clsGenerateConnection();
        return obtenerJuegos.ReadGames("select * from game");
    });

    app.MapPut("/updategame",(clsGame game) => {
        var query = "update game set playerOneScore = @pos, secondplayerscore = @pst where id_game = @id;";
        var query = query.Replace("@pos",Convert.ToString(game.playerOneScore));
        petition = query.Replace("@pts",Convert.ToString(game.playerTwoScore));
        petition = query.Replace("@id",Convert.ToString(game.id));    
        clsGenerateConnection modificarJugador = new clsGenerateConnection();
        modificarJugador.ExecuteNonQuery(petition);
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "chessAPI terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
