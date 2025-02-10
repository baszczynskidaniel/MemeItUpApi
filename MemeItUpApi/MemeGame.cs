
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
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

    public class GameStateVotingCharDto
    {
        public List<MemeInGame> MemesToVote { get; set; }
        public Player CurrentChar { get; set; }
        public GameStateVotingCharDto(GameState gameState)
        {
            CurrentChar = gameState.VoterCharState.CurrentChar;
            MemesToVote = gameState.Memes.Where(
                    m => m.Round == gameState.Round).ToList();
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
       // public bool StartNextRoundAutomatically { get; set; } = true;
        //public int AutomaticRoundStartDelay { get; set; } = 10;
    }

    public enum GameMode
    {
        Cards = 0,
        FillInBlank = 1,
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
       
        GameEnd,
        UNKNOWN,
        VOTE_CHAR,
        RoundEndChar,
    }

    public class GameSessionDto
    {
        public List<Player> Players { get; set; }
        public Player Player { get; set; }
        public GameStateEnum GameState { get; set; }
        public int Round { get; set; }
        public int NumberOfRounds { get; set; }

        public GameSessionDto(Player player, GameState gameState)
        {
            this.Player = player;
            this.GameState = gameState.State;
            this.Round = gameState.Round;
            this.NumberOfRounds = gameState.Rules.NumberOfRounds;
            this.Players = gameState.Players;

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

    public class VoterCharState
    {
        public int currentIndex { get; set; } = 0;
        public List<Player> ShuffledPlayers { get; set; } = new List<Player>();
        public Player CurrentChar { get; set; } = null;

        public VoterCharState(GameState gameState)
        {
            var random = new Random();
            ShuffledPlayers = gameState.Players.OrderBy(x => random.Next()).ToList();
            CurrentChar = ShuffledPlayers[currentIndex];
        }

    

        public void NextChar()
        {
            currentIndex++;
            if (currentIndex >= ShuffledPlayers.Count)
            {
                currentIndex = 0;
                CurrentChar = ShuffledPlayers[currentIndex];
              
            }
            CurrentChar = ShuffledPlayers[currentIndex];
           
        }
    }


    public class GameState
    {
      
        public int Round { get; set; } = 1;
        public GameStateEnum State { get; set; } = GameStateEnum.Lobby;

        public MemeTemplate CurrentMeme { get; set; } = null;

        public VoterCharState VoterCharState { get; set; } = null;

        public Rules Rules { get; set; } = new Rules();

        public Player Host { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();
        public List<MemeInGame> Memes { get; set; } = new List<MemeInGame>();
        public List<Vote> Votes { get; set; } = new List<Vote>();
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