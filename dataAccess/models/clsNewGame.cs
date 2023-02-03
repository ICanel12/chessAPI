using chessAPI.dataAccess.common;
namespace chessAPI.models.player;

public sealed class clsNewGame
{
    public int? id_player_One {get; set;}
    public int? id_player_Two {get; set;}
    public int? playerOneScore {get; set;}
    public int? playerTwoScore {get; set;}

    public clsNewGame(){
        playerOneScore = 0;
        playerTwoScore = 0;
    }
}