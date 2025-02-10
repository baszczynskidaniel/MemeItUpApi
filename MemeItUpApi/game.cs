using Application.Services;
using Core.Interfaces;
using K4os.Compression.LZ4.Internal;
using MemeItUpApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using static MemeItUpApi.MemeTemplateDto;









namespace Core.Interfaces
{
    public interface IGameService
    {
        void JoinLobby(Player player);
        void LeaveLobby(string connectionId);
        IEnumerable<Player> GetPlayers();
        void PostMeme(MemeTemplate memeTemplate, String connectionId);
        GameState GetGameState();
        void Vote(MakeVoteDto voteDto, Player voter);
        void StartGame();

        public Player GetPlayerByConnectionId(string connectionId);
        public MemeInGame GetNextMemeToVoteOn(Player player);
        public bool HasRemainingMemesToVoteFor(Player player);
        public List<MemeInGame> GetMemesInGameInRounds();
        public GameSessionDto GetGameSession(string connectionId);
        public LobbyDto GetLobbyDto();
        public event Func<GameStateEnum, Task> OnGameStateChanged;
        public void UpdateRules(Rules rules);
        public void FinishReviewResult(Player player);
        public void SetCurrentMeme(MemeTemplate? meme);
    }
}

namespace Application.Services
{
    public class GameService : IGameService
    {

        private readonly GameState _gameState = new();
        private readonly IMemeTemplateService _memeTemplateService;

        public GameState GetGameState()
        {
            return _gameState;
        }

        public event Func<GameStateEnum, Task> OnGameStateChanged;

        


        public MemeInGame GetNextMemeToVoteOn(Player player)
        {
           
            var memesToVoteOn = _gameState.Memes
                .Where(
                    m => //m.Author.Id != player.Id && 
                    !_gameState.Votes.Any(v => v.Meme.MemeInGameId == m.MemeInGameId && v.Voter.Id == player.Id))
               .FirstOrDefault();
          

            return memesToVoteOn;
        }

        public void UpdateRules(Rules rules)
        {
            _gameState.Rules = rules;
        }

        public Player GetPlayerByConnectionId(string connectionId)
        {
            return _gameState.Players.First(p => p.ConnectionId == connectionId);
        }

        public LobbyDto GetLobbyDto()
        {
            return new LobbyDto(_gameState);
        }



        public bool HasRemainingMemesToVoteFor(Player player)
        {
            var remainingMemes = _gameState.Memes
                .Where(m => 
               // m.Author.Id != player.Id &&
                !_gameState.Votes.Any(v => v.Meme.MemeInGameId == m.MemeInGameId && v.Voter.Id == player.Id))
                .Any();
            return remainingMemes;
        }


        public void Vote(MakeVoteDto voteDto, Player voter)
        {

            var meme = _gameState.Memes.First(m => m.MemeInGameId == voteDto.MemeInGameId);
            var memeAuthor = meme.Author;

            _gameState.Votes.Add(new Vote
            {
                Value = voteDto.ScoreChange,
                Voter = voter,
                Meme = meme,
                Round = _gameState.Round
            });
            meme.Score += voteDto.ScoreChange;
            memeAuthor.Score += voteDto.ScoreChange;

            if (_gameState.Rules.EveryoneIsTheJudge) { 
                if (!HasRemainingMemesToVoteFor(voter))
                    voter.State = PlayerStateEnum.Waiting;

                if (_gameState.Players.All(p => p.State == PlayerStateEnum.Waiting))
                {
                    foreach (var player in _gameState.Players)
                    {
                        player.State = PlayerStateEnum.Playing;
                    }
                    _gameState.State = GameStateEnum.RoundEnd;

                }
            } else
            {
                foreach (var player in _gameState.Players)
                {
                    player.State = PlayerStateEnum.Playing;
                }
                _gameState.State = GameStateEnum.RoundEndChar;
            }
        }

        public void PostMeme(MemeTemplate memeTemplate, String connectionId)
        {
            var author = _gameState.Players.First(p => p.ConnectionId == connectionId);
            _gameState.Memes.Add(new MemeInGame
            {
                Author = author,
                Meme = memeTemplate,
                Round = _gameState.Round
            });

            author.State = PlayerStateEnum.Waiting;

            if (_gameState.Players.All(p => p.State == PlayerStateEnum.Waiting))
            {
               
                if(_gameState.Rules.EveryoneIsTheJudge)
                {
                    foreach (var player in _gameState.Players)
                    {
                        player.State = PlayerStateEnum.Playing;
                    }
                    _gameState.State = GameStateEnum.Vote;
                }
                else
                {
                    foreach (var player in _gameState.Players)
                    {
                        player.State = PlayerStateEnum.Waiting;
                    }
                    _gameState.State = GameStateEnum.VOTE_CHAR;
                    _gameState.VoterCharState.CurrentChar.State = PlayerStateEnum.Playing;
                }
            
                
            }
        }

        public void JoinLobby(Player player)
        {
            if(_gameState.Players.Count == 0)
            {
                _gameState.Host = player;
            }
            _gameState.Players.Add(player);
        }




        public void LeaveLobby(string connectionId)
        {
            // If there is only one player
            if (_gameState.Players.Count == 1)
            {
                _gameState.Rules = new Rules();
                _gameState.Memes.Clear();
                _gameState.Round = 1;
                _gameState.State = GameStateEnum.Lobby;
                _gameState.Host = null;
                _gameState.Players.Clear();
                _gameState.Votes.Clear();
                _gameState.CurrentMeme = null;
                _gameState.VoterCharState = null;


            }
            else if (_gameState.Host != null)
            {
                // Remove player based on connectionId
                _gameState.Players.RemoveAll(p => p.ConnectionId == connectionId);

                // Re-assign host if the host left
                if (_gameState.Host.ConnectionId == connectionId)
                {
                    if (_gameState.Players.Count > 0)
                    {
                        _gameState.Host = _gameState.Players.First();
                    }
                    else
                    {
                        _gameState.Host = null;
                    }
                }

            }
            if (_gameState.Players.Count == 1)
            {
                _gameState.State = GameStateEnum.Lobby;
            }
        }


        public IEnumerable<Player> GetPlayers()
        {
            return _gameState.Players;

        }

        public void StartGame()
        {
            
            _gameState.Players.ForEach(p => p.State = PlayerStateEnum.Playing);
            _gameState.State = GameStateEnum.Play;
      
            if(!_gameState.Rules.EveryoneIsTheJudge)
            {
                _gameState.VoterCharState = new VoterCharState(_gameState);
                _gameState.VoterCharState.CurrentChar.State = PlayerStateEnum.Waiting;
            } else
            {
                _gameState.VoterCharState = null;
            }
        }

        


        public List<MemeInGame> GetMemesInGameInRounds()
        {
            return _gameState.Memes.Where(m => m.Round == _gameState.Round).ToList();
       
        }

        public void FinishReviewResult(Player player)
        {
            player.State = PlayerStateEnum.Waiting;
            
            if(_gameState.Players.All(p => p.State == PlayerStateEnum.Waiting))
            {
                if (!_gameState.Rules.EveryoneIsTheJudge)
                {
                   
                    _gameState.VoterCharState.NextChar();
                    _gameState.Round++;
                }
                else
                {
                    _gameState.Round++;
                }
                if (_gameState.Round>=_gameState.Rules.NumberOfRounds)
                {
                    _gameState.State = GameStateEnum.Lobby;
                    foreach (var p in _gameState.Players)
                    {
                        p.State = PlayerStateEnum.Lobby;
                    }
                }
                else
                {
                    _gameState.State = GameStateEnum.Play;
                    foreach (var p in _gameState.Players)
                    {
                        p.State = PlayerStateEnum.Playing;
                    }

                    if (!_gameState.Rules.EveryoneIsTheJudge)
                    {
                        _gameState.VoterCharState.CurrentChar.State = PlayerStateEnum.Waiting;
                    }
                }
              
            }
        }

        public GameSessionDto GetGameSession(string connectionId)
        {
            var player = _gameState.Players.First(p => p.ConnectionId == connectionId);
            return new GameSessionDto(player, _gameState);
        }
        
        public void FinishRound(Player player)
        {
            player.State = PlayerStateEnum.Waiting;
            if (_gameState.Players.All(p => p.State == PlayerStateEnum.Waiting))
            {
                _gameState.Round++;
                if (_gameState.Round > _gameState.Rules.NumberOfRounds)
                {
                    _gameState.State = GameStateEnum.GameEnd;
                    SetPlayersState(PlayerStateEnum.Playing);
               
                    return;
                }
                _gameState.State = GameStateEnum.Play;
              
            }   
        }

    

        private void SetPlayersState(PlayerStateEnum state)
        {
            foreach (var player in _gameState.Players)
            {
                player.State = state;
            }
        }

        public  void SetCurrentMeme(MemeTemplate? meme)
        {
            _gameState.CurrentMeme = meme;
        }

    }
   
}


public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameHub> _logger;
    private readonly IMemeTemplateService _memeTemplateService;

    public async Task BroadcastGameStateVotingDto()
    {
        var gameState = _gameService.GetGameState();
        var gameStateVotingDto = new GameStateVotingDto(gameState);
        foreach (Player player in gameState.Players)
        {


            await Clients.Client(player.ConnectionId).SendAsync("BroadcastGameStateVotingDto", gameStateVotingDto);
        }
    }

    public async Task BroadcastNextMemeForVoting()
    {
        var gameState = _gameService.GetGameState();

        foreach (Player player in gameState.Players)
        {
            var memeToVote = _gameService.GetNextMemeToVoteOn(player);
            await Clients.Client(player.ConnectionId).SendAsync("ReceiveNextMemeForVoting", memeToVote);
        }
    }

    public async Task SendGameStateVotingCharDto()
    {
        var gameState = _gameService.GetGameState();
        var gameStateVotingCharDto = new GameStateVotingCharDto(gameState);
        await Clients.Caller.SendAsync("ReceiveGameStateVotingCharDto", gameStateVotingCharDto);
    }



    public async Task MakeVote(string MemeInGameId, int scoreChange)
    {
        var gameState = _gameService.GetGameState();
        var callerPlayer = _gameService.GetPlayerByConnectionId(Context.ConnectionId);
        _gameService.Vote(new MakeVoteDto
        {
            MemeInGameId = Guid.Parse(MemeInGameId),
            ScoreChange = scoreChange
        },
        callerPlayer

        );


        if (callerPlayer.State == PlayerStateEnum.Playing)
        {
            var memeToVote = _gameService.GetNextMemeToVoteOn(callerPlayer);
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveNextMemeForVoting", memeToVote);

        }
        else
        {
            var gameStateVotingDto = new GameStateVotingDto(gameState);
            await Clients.All.SendAsync("BroadcastGameStateVotingDto", gameStateVotingDto);
         
        }
        await SendGameSessionToAllPlayers();
    }

    public async Task SendRoundResult()
    {
        var gameState = _gameService.GetGameState();
        var resultDto = new ResultDto(gameState);
        
        await Clients.Caller.SendAsync(HubMessages.RECEIVE_ROUND_RESULT, resultDto); 
    }

    public async Task FinishReviewResult()
    {
        var caller = _gameService.GetPlayerByConnectionId(Context.ConnectionId);
        _gameService.FinishReviewResult(caller);
        if (_gameService.GetGameState().Rules.SameMemeForEveryone)
        {
            var memeDto = await _memeTemplateService.GetRandomTemplate();
            if (memeDto != null)
            {
                var meme = memeDto.ToMemeTemplate();
                _gameService.SetCurrentMeme(meme);
            }
        }


        await SendGameSessionToAllPlayers();
    }

    public async Task SendLobby()
    {
        await Clients.Caller.SendAsync(HubMessages.RECEIVE_LOBBY, _gameService.GetLobbyDto());
    }

    public async Task SendAllLobby()
    {      
         await Clients.All.SendAsync(HubMessages.RECEIVE_LOBBY, _gameService.GetLobbyDto());      
    }

    public async Task UpdateRules(Rules rules)
    {
        _gameService.UpdateRules(
          
                rules
            
        );
        await SendAllLobby();
    }

    public async Task SendPlayers()
    {
        await Clients.Caller.SendAsync(HubMessages.RECEIVE_PLAYERS, new PlayersDto(_gameService.GetGameState()));
    }


    public async Task SendAllPlayers()
    {   
        await Clients.All.SendAsync(HubMessages.RECEIVE_PLAYERS, new PlayersDto(_gameService.GetGameState()));
    }



 

    public async Task StartGame()
    {
       
        _gameService.StartGame();
        if(_gameService.GetGameState().Rules.SameMemeForEveryone)
        {
            var memeDto = await _memeTemplateService.GetRandomTemplate();
            if (memeDto != null)
            {
                var meme = memeDto.ToMemeTemplate();
                _gameService.SetCurrentMeme(meme);
            }
        }
        else
        {
            _gameService.SetCurrentMeme(null);
        }
        await SendGameSessionToAllPlayers();

    }

    public async Task SendMeme()
    {
        
        var meme = _gameService.GetGameState().CurrentMeme;
        if (meme == null)
        {
            var memeDto = await _memeTemplateService.GetRandomTemplate();
            if (memeDto != null)
            {
                meme = memeDto.ToMemeTemplate();
                _gameService.SetCurrentMeme(meme);
            }
        }
        await Clients.Caller.SendAsync("ReceiveMeme", meme);

    }


    public async Task BroadcastInitialMemesForVoting()
    {
        foreach (var player in _gameService.GetPlayers())
        {
            var firstMeme = _gameService.GetNextMemeToVoteOn(player);
            if (firstMeme != null)
            {
                await Clients.Client(player.ConnectionId).SendAsync("ReceiveNextMemeForVoting", firstMeme);
            }
            else
            {
                // no meme to vote on
            }
        }
    }



    public async Task PostMeme(MemeTemplateDto memeTemplateDto)
    {
        _gameService.PostMeme(memeTemplateDto.toMemeTemplateDto(), Context.ConnectionId);
        var gameState = _gameService.GetGameState();
      
         await SendGameSessionToAllPlayers();

    }

    public GameHub(IGameService gameService, ILogger<GameHub> logger, IMemeTemplateService memeTemplateService)
    {
        _gameService = gameService;
        _logger = logger;
        _gameService.OnGameStateChanged += OnGameStateChanged;
        _memeTemplateService = memeTemplateService;
    }

    public async Task OnGameStateChanged(GameStateEnum gameState)
    {    
        await SendGameSessionToAllPlayers();
    }

    public async Task JoinLobby(string playerName, bool isMobile)
    {
        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            Name = playerName,
            IsUsingMobileDevice = isMobile,
            Id = Guid.NewGuid(),
            State = PlayerStateEnum.Playing,
            Score = 0,

        };
        _gameService.JoinLobby(player);
        var players = _gameService.GetPlayers();
       
        await Clients.All.SendAsync(HubMessages.RECEIVE_LOBBY, _gameService.GetLobbyDto());
        await SendGameSessionToAllPlayers();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {


        _gameService.LeaveLobby(Context.ConnectionId);
        var players = _gameService.GetPlayers();
        await base.OnDisconnectedAsync(exception);
        await Clients.All.SendAsync(HubMessages.RECEIVE_LOBBY, _gameService.GetLobbyDto());
        await SendGameSessionToAllPlayers();
    }

    public async Task LeaveLobby()
    {
        _gameService.LeaveLobby(Context.ConnectionId);
        await SendGameSessionToAllPlayers();
    }

    public async Task SendGameSessionToCaller()
    {
        var sessionDto = _gameService.GetGameSession(Context.ConnectionId);
        await Clients.Caller.SendAsync(HubMessages.RECEIVE_GAME_SESSION, sessionDto);
    }

    public async Task SendGameSessionToAllPlayers()
    {
        var players = _gameService.GetPlayers();
        foreach(Player player in players)
        {
            var sessionDto = _gameService.GetGameSession(player.ConnectionId);
            await Clients.Client(player.ConnectionId).SendAsync(HubMessages.RECEIVE_GAME_SESSION, sessionDto);
        }
          
      
    }
}

public static class HubMessages
{
    public const string RECEIVE_GAME_SESSION = "ReceiveGameSession";
    public const string RECEIVE_LOBBY = "ReceiveLobby";
    public const string RECEIVE_ROUND_RESULT = "ReceiveRoundResult";
    public const string RECEIVE_PLAYERS = "ReceivePlayers";
}


[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IHubContext<GameHub> _hubContext;

   
   



    public LobbyController(IGameService lobbyService, IHubContext<GameHub> hubContext)
    {
        _gameService = lobbyService;
        _hubContext = hubContext;
    }
    [HttpGet("roundResult")]
    public IActionResult GetResultDto()
    {
      
        return Ok(_gameService.GetGameState().Memes);
    }


    [HttpGet("connectionId")]
    public IActionResult GetConnectionId()
    {
        return Ok(new { ConnectionId = HttpContext.Connection.Id });
    }

    [HttpGet("players")]
    public async Task<IActionResult> GetPlayers()
    {
        var players = _gameService.GetPlayers();
        //await _hubContext.Clients.All.SendAsync("BroadcastPlayers", new LobbyPlayersDto(players.ToList()));
        return Ok(_gameService.GetLobbyDto());
    }

    [HttpPost("udpateRules")]
    public async Task<IActionResult> UpdateRules([FromBody] Rules rules)
    {
        _gameService.UpdateRules(rules);
        //await _hubContext.Clients.All.SendAsync("BroadcastPlayers", new LobbyPlayersDto(players.ToList()));
        return Ok(_gameService.GetLobbyDto());
    }



    //[HttpPost("join")]
    //public async Task<IActionResult> JoinLobby([FromBody] Player playerDto)
    //{
    //    var player = new Player
    //    {   
    //        Name = playerDto.Name,
    //        IsUsingMobileDevice = playerDto.IsUsingMobileDevice,
    //        Id = Guid.NewGuid(),


    //    };
    //    _lobbyService.JoinLobby(player);

    //    var players = _lobbyService.GetPlayers();

    //    await _hubContext.Clients.All.SendAsync("BroadcastPlayers", new LobbyPlayersDto(players.ToList()));
    //    return Ok(player.Id);
    //}

}
