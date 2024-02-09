using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDBContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDBContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDTO>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
        .Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

        return _mapper.Map<AuctionDTO>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDto auctionDTO)
    {
        var auction = _mapper.Map<Auction>(auctionDTO);
        auction.Seller = "test";
        _context.Auctions.Add(auction);

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the database");

        return CreatedAtAction(nameof(GetAuctionById),
         new { auction.Id },
         _mapper.Map<AuctionDTO>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDTO)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

        if(auction == null) return NotFound();
        // todo: add authorization check and add user

        auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not update changes to the database");

        return Ok();

    }

}
