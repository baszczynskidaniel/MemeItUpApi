using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeItUpApi
{
    public class MemeTemplate
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<TextPosition> TextPositions { get; set; }
    }

    public class TextPosition
    {
        public Guid Id { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public string Text { get; set; } = "";
        public Guid? MemeTemplateId { get; set; }
        public MemeTemplate MemeTemplate { get; set; }
    }

    public class MemeTemplateDto
    {
        public Guid? Id { get; set; }
        public string ImageUrl { get; set; }
        public ICollection<TextPositionDto> TextPositions { get; set; }

        public MemeTemplate toMemeTemplateDto()
        {
            return new MemeTemplate
            {
                Id = Guid.NewGuid(),
                ImageUrl = ImageUrl,
                TextPositions = TextPositions.Select(tp => new TextPosition
                {
                    Id = Guid.NewGuid(),
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    MemeTemplateId = Guid.NewGuid(),
                    Text = tp.Text
                }).ToList()
            };
        }
    }






    public class TextPositionDto
    {
        public Guid? Id { get; set; }
        public float Top { get; set; }
        public float Bottom { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public string Text { get; set; } = "";
        public Guid? MemeTemplateId { get; set; }

        public TextPosition toTextPositionDto()
        {
            return new TextPosition
            {
                Id = Guid.NewGuid(),
                Top = Top,
                Bottom = Bottom,
                Left = Left,
                Right = Right,
                Text = Text,
                MemeTemplateId = Guid.NewGuid()
            };
        }
    }

    public class ApplicationDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public ApplicationDbContext(IConfiguration configuration, DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Configuration = configuration;
        }

        public DbSet<MemeTemplate> MemeTemplates { get; set; }
        public DbSet<TextPosition> TextPositions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlite(Configuration.GetConnectionString("ApiDatabase"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MemeTemplate>()
                 .HasMany(mt => mt.TextPositions)
                 .WithOne(tp => tp.MemeTemplate)
                 .HasForeignKey(tp => tp.MemeTemplateId);

            modelBuilder.Entity<TextPosition>()
                .HasOne(tp => tp.MemeTemplate)
                .WithMany(mt => mt.TextPositions)
                .HasForeignKey(tp => tp.MemeTemplateId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }

    public interface IMemeTemplateService
    {
        Task<MemeTemplateDto> AddTemplate(MemeTemplateDto memeTemplateDto);
        Task<MemeTemplateDto> GetRandomTemplate();
        Task<IEnumerable<MemeTemplateDto>> GetAllTemplates();
        Task<MemeTemplateDto> GetTemplateById(Guid id);
        Task<MemeTemplateDto> UpdateTemplate(Guid id, MemeTemplateDto memeTemplateDto);
    }


    public interface ITextPositionService
    {
        Task<TextPositionDto> CreateTextPosition(TextPositionDto textPositionDto);
        Task<IEnumerable<TextPositionDto>> GetAllTextPositions();
        Task<TextPositionDto> GetTextPositionById(Guid id);
        Task<TextPositionDto> UpdateTextPosition(Guid id, TextPositionDto textPositionDto);
    }

    public class MemeTemplateService : IMemeTemplateService
    {
        private readonly ApplicationDbContext _context;

        public MemeTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MemeTemplateDto> AddTemplate(MemeTemplateDto memeTemplateDto)
        {

            var memeId = Guid.NewGuid();
            var memeTemplate = new MemeTemplate
            {
                Id = memeId,
                ImageUrl = memeTemplateDto.ImageUrl,
                TextPositions = memeTemplateDto.TextPositions.Select(tp => new TextPosition
                {
                    Id = Guid.NewGuid(),
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    MemeTemplateId = memeId,
                    Text = tp.Text
                }).ToList()
            };

            _context.MemeTemplates.Add(memeTemplate);
            await _context.SaveChangesAsync();

            return memeTemplateDto;
        }

        public async Task<MemeTemplateDto> GetRandomTemplate()
        {
            var count = await _context.MemeTemplates.CountAsync();
            var randomIndex = new Random().Next(count);
            var memeTemplate = await _context.MemeTemplates

                .Include(mt => mt.TextPositions)
                .Skip(randomIndex)
                .FirstOrDefaultAsync();

            if (memeTemplate == null) return null;

            return new MemeTemplateDto
            {
                Id = memeTemplate.Id,
                ImageUrl = memeTemplate.ImageUrl,
                TextPositions = memeTemplate.TextPositions.Select(tp => new TextPositionDto
                {
                    Id = tp.Id,
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    MemeTemplateId = tp.MemeTemplateId,
                    Text = tp.Text
                }).ToList()
            };
        }

        public async Task<IEnumerable<MemeTemplateDto>> GetAllTemplates()
        {
            var memeTemplates = await _context.MemeTemplates.Include(mt => mt.TextPositions).ToListAsync();
            return memeTemplates.Select(mt => new MemeTemplateDto
            {
                Id = mt.Id,
                ImageUrl = mt.ImageUrl,
                TextPositions = mt.TextPositions.Select(tp => new TextPositionDto
                {
                    Id = tp.Id,
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    MemeTemplateId = tp.MemeTemplateId,
                    Text = tp.Text

                }).ToList()
            });
        }

        public async Task<MemeTemplateDto> GetTemplateById(Guid id)
        {
            var memeTemplate = await _context.MemeTemplates.Include(mt => mt.TextPositions)
                .FirstOrDefaultAsync(mt => mt.Id == id);

            if (memeTemplate == null) return null;

            return new MemeTemplateDto
            {
                Id = memeTemplate.Id,
                ImageUrl = memeTemplate.ImageUrl,
                TextPositions = memeTemplate.TextPositions.Select(tp => new TextPositionDto
                {
                    Id = tp.Id,
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    MemeTemplateId = tp.MemeTemplateId,
                    Text = tp.Text

                }).ToList()
            };
        }

        public async Task<MemeTemplateDto> UpdateTemplate(Guid id, MemeTemplateDto memeTemplateDto)
        {
            var memeTemplate = await _context.MemeTemplates.Include(mt => mt.TextPositions)
                .FirstOrDefaultAsync(mt => mt.Id == id);

            if (memeTemplate == null) return null;


            memeTemplate.ImageUrl = memeTemplateDto.ImageUrl;
            memeTemplate.TextPositions = memeTemplateDto.TextPositions.Select(tp =>
                new TextPosition
                {
                    Id = (Guid)tp.Id,
                    Top = tp.Top,
                    Bottom = tp.Bottom,
                    Left = tp.Left,
                    Right = tp.Right,
                    Text = tp.Text,
                    MemeTemplateId = tp.MemeTemplateId
                }
            ).ToList();

            await _context.SaveChangesAsync();

            return memeTemplateDto;
        }
    }

    public class TextPositionService : ITextPositionService
    {
        private readonly ApplicationDbContext _context;

        public TextPositionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TextPositionDto> CreateTextPosition(TextPositionDto textPositionDto)
        {
            var textPosition = new TextPosition
            {
                Id = Guid.NewGuid(),
                Top = textPositionDto.Top,
                Bottom = textPositionDto.Bottom,
                Left = textPositionDto.Left,
                Right = textPositionDto.Right,
                Text = textPositionDto.Text,
                MemeTemplateId = textPositionDto.MemeTemplateId
            };

            _context.TextPositions.Add(textPosition);
            await _context.SaveChangesAsync();

            return textPositionDto;
        }

        public async Task<IEnumerable<TextPositionDto>> GetAllTextPositions()
        {
            var textPositions = await _context.TextPositions.ToListAsync();
            return textPositions.Select(tp => new TextPositionDto
            {
                Id = tp.Id,
                Top = tp.Top,
                Bottom = tp.Bottom,
                Left = tp.Left,
                Right = tp.Right,
                Text = tp.Text,
                MemeTemplateId = tp.MemeTemplateId
            });
        }

        public async Task<TextPositionDto> GetTextPositionById(Guid id)
        {
            var textPosition = await _context.TextPositions.FindAsync(id);

            if (textPosition == null) return null;

            return new TextPositionDto
            {
                Id = textPosition.Id,
                Top = textPosition.Top,
                Bottom = textPosition.Bottom,
                Left = textPosition.Left,
                Right = textPosition.Right,
                Text = textPosition.Text,
                MemeTemplateId = textPosition.MemeTemplateId
            };
        }

        public async Task<TextPositionDto> UpdateTextPosition(Guid id, TextPositionDto textPositionDto)
        {
            var textPosition = await _context.TextPositions.FindAsync(id);

            if (textPosition == null) return null;

            textPosition.Top = textPositionDto.Top;
            textPosition.Bottom = textPositionDto.Bottom;
            textPosition.Left = textPositionDto.Left;
            textPosition.Right = textPositionDto.Right;
            textPosition.MemeTemplateId = textPositionDto.MemeTemplateId;
            textPosition.Text = textPositionDto.Text;

            await _context.SaveChangesAsync();

            return textPositionDto;
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class MemeTemplateController : ControllerBase
    {
        private readonly IMemeTemplateService _memeTemplateService;

        public MemeTemplateController(IMemeTemplateService memeTemplateService)
        {
            _memeTemplateService = memeTemplateService;
        }

        [HttpPost]
        public async Task<ActionResult<MemeTemplateDto>> AddTemplate([FromBody] MemeTemplateDto memeTemplateDto)
        {
            var result = await _memeTemplateService.AddTemplate(memeTemplateDto);
            return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
        }

        [HttpGet("random")]
        public async Task<ActionResult<MemeTemplateDto>> GetRandomTemplate()
        {
            var result = await _memeTemplateService.GetRandomTemplate();
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemeTemplateDto>>> GetAllTemplates()
        {
            var result = await _memeTemplateService.GetAllTemplates();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemeTemplateDto>> GetTemplateById(Guid id)
        {
            var result = await _memeTemplateService.GetTemplateById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MemeTemplateDto>> UpdateTemplate(Guid id, [FromBody] MemeTemplateDto memeTemplateDto)
        {
            var result = await _memeTemplateService.UpdateTemplate(id, memeTemplateDto);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class TextPositionController : ControllerBase
    {
        private readonly ITextPositionService _textPositionService;

        public TextPositionController(ITextPositionService textPositionService)
        {
            _textPositionService = textPositionService;
        }

        [HttpPost]
        public async Task<ActionResult<TextPositionDto>> CreateTextPosition([FromBody] TextPositionDto textPositionDto)
        {
            var result = await _textPositionService.CreateTextPosition(textPositionDto);
            return CreatedAtAction(nameof(GetTextPositionById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TextPositionDto>>> GetAllTextPositions()
        {
            var result = await _textPositionService.GetAllTextPositions();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TextPositionDto>> GetTextPositionById(Guid id)
        {
            var result = await _textPositionService.GetTextPositionById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TextPositionDto>> UpdateTextPosition(Guid id, [FromBody] TextPositionDto textPositionDto)
        {
            var result = await _textPositionService.UpdateTextPosition(id, textPositionDto);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
