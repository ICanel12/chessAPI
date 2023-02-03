using Npgsql;
using chessAPI.models.player;

namespace chessAPI.dataAccess.providers.postgreSQL
{
    public class GenerateConnection
    {
        NpgsqlConnection generateConnection = new NpgsqlConnection();

        public NpgsqlConnection conect()
        {
            try
            {
                generateConnection.ConnectionString = "Server=localhost;Port=5432;Database=chessDB;User Id=postgres;Password=misDatos2023!;Pooling=true;MinPoolSize=3;MaxPoolSize=20;Max Auto Prepare=15;Enlist=false;Auto Prepare Min Usages=3";
                generateConnection.Open();
                Console.WriteLine("Se ha realizado la conexión exitosamente");
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine("Se ha producido un error en la conexión");
                Console.WriteLine("Error:", e);
            }
            return generateConnection;
        }

        public async Task<List<clsNewPlayer>>ExecuteQuery (string petition)
        {
            conect();
            await using var command = new NpgsqlCommand(petition, generateConnection);
            await using var reader = await command.ExecuteReaderAsync();
            List<clsNewPlayer> querylist = new List<clsNewPlayer>();
            while(await reader.ReadAsync())
            {
                clsNewPlayer pl = ReadPlayer(reader);
                querylist.Add(pl);    
            }

            generateConnection.Close();
            return querylist;
        }

        private static clsNewPlayer ReadPlayer(NpgsqlDataReader reader)
        {
            string _email = reader["email"] as string;

            clsNewPlayer readedplayer = new clsNewPlayer
            {
                email = _email
            };

            return readedplayer;
        }
        private static clsNewGame ReadGame(NpgsqlDataReader reader)
        {
            int? readPlayerOneScore = reader["playerOneScore"] as int?;
            int? readPlayerTwoScore = reader["playerSecondScore"] as int?;

            clsNewGame game= new clsNewGame
            {               
                playerOneScore = readPlayerOneScore,
                playerTwoScore = readPlayerTwoScore,
            };

            return game;

        }
    }
}