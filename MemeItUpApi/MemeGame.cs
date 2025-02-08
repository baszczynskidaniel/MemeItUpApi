
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace MemeItUpApi
{
    public class Player
    {
        public Guid Id { get; set; }
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public Boolean IsUsingMobileDevice { get; set; }
        public int Score { get; set; } = 0;
        public PlayerStateEnum State { get; set; } = PlayerStateEnum.Lobby;
    }

    public class Vote
    {
        public int Round { get; set; }
        public int Value { get; set; } = 0;
        public Player Voter { get; set; }
        public MemeInGame Meme { get; set; }
    }

    public class MakeVoteDto
    {
        public Guid MemeInGameId { get; set; }
        public int ScoreChange { get; set; }
    }

    public class GameStateVotingDto
    {    
        public int NumberOfRounds { get; set; }
        public int Round { get; set; }
        public GameStateEnum gameStateEnum { get; set; }
        public List<Player> Players { get; set; }    

        public GameStateVotingDto(GameState gameState)
        {
            NumberOfRounds = gameState.Rules.NumberOfRounds;
            Round = gameState.Round;
            Players = gameState.Players;
            gameStateEnum = gameState.State;
        }
    }

    public class LobbyDto
    {
        public List<Player> Players { get; set; }
        public Player Host { get; set; }
        public Rules Rules { get; set; }

        public LobbyDto(GameState gameState)
        {
            this.Players = gameState.Players;
            this.Rules = gameState.Rules;
            this.Host = gameState.Host;
        }
    }


    public class Rules
    {
        public int NumberOfRounds { get; set; } = 5;
        public bool SameMemeForEveryone { get; set; } = false;
        public bool EveryoneIsTheJudge { get; set; } = true;
        public GameMode GameMode { get; set; } = GameMode.FillInBlank;
        public bool StartNextRoundAutomatically { get; set; } = true;
        public int AutomaticRoundStartDelay { get; set; } = 10;
    }

    public enum GameMode
    {
        Cards,
        FillInBlank,
    }

    public class CurrentMemeVotedDto
    {
        public MemeTemplate Meme { get; set; }
        public Guid MemeInGameId { get; set; }
    }

    public enum PlayerStateEnum
    {
        Lobby,
        Playing,
        Waiting,
        Disconnected,
    }

    public enum GameStateEnum
    {
        Lobby,
        Play,
        Vote,
        RoundEnd,
        GameEnd
    }

    public class GameSessionDto
    {
        public Player Player { get; set; }
        public GameStateEnum gameState { get; set; }

        public GameSessionDto(Player player, GameStateEnum gameStateEnum)
        {
            Player = player;
            gameState = gameStateEnum;
        }
    }

        public class MemeToVoteDto
    { 
        public Guid MemeTemplateId { get; set; }
        public MemeTemplate Meme { get; set; }
    }

    public class MemeInGame
    {
        public Guid MemeInGameId { get; set; } = Guid.NewGuid();
        public MemeTemplate Meme { get; set; }
        public int Score { get; set; } = 0;
        public int Round { get; set; } = 0;
        public Player Author { get; set; }
    }

    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public Player Host { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public Rules Rules { get; set; } = new Rules();
        public string Code { get; set; }
        public GameState GameState { get; set; } = new GameState();
        public string? Password { get; set; } = null;
    }

    public class CreateRoomDto
    {
        public string Name { get; set; }
        public string? Password { get; set; }
    }


    public class GameState
    {
       
        public int Round { get; set; } = 1;
        public GameStateEnum State { get; set; } = GameStateEnum.Lobby;
        public Rules Rules { get; set; } = new Rules();

        public Player Host { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();
        public List<MemeInGame> Memes { get; set; } = new List<MemeInGame>();
        public List<Vote> Votes { get; set; } = new List<Vote>();
    }

    public class  GameStatePlayingDto
    {
        public GameStateEnum State { get; set; }
        public int NumberOfRounds { get; set; }
        public int Round { get; set; }
        public List<Player> Players { get; set; }
        public string CallerConnectionId { get; set; }

        public GameStatePlayingDto(GameState gameState)
        {
            State = gameState.State;
            NumberOfRounds = gameState.Rules.NumberOfRounds;
            Round = gameState.Round;
            Players = gameState.Players;
            CallerConnectionId = "";
        }
    }

    public class PlayersDto
    {
        public List<Player> Players { get; set; }
        public PlayersDto(GameState gameState)
        {
            Players = gameState.Players;
        }
    }

  
    


        public class ResultDto
    {
        public List<MemeInGame> Memes { get; set; }
        public int Round { get; set; }
        public int NumberOfRounds { get; set; }
       
       

        public ResultDto(GameState gameState)
        {
            
            Memes = gameState.Memes;
            Round = gameState.Round;
            
            NumberOfRounds = gameState.Rules.NumberOfRounds;
        }
    }
}


// TODO

/***
 * Lobby
 * Rooms
 * Result
 * Organize code
 * 
 * 
 * 
 * 
 * 
 */