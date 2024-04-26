using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Models;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(IAuctionRepository repository, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        return await _repository.GetAuctionsAsync(date);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _repository.GetAuctionByIdAsync(id);

        if (auction == null) return NotFound();

        return auction;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDTO)
    {
        var auction = _mapper.Map<Auction>(auctionDTO);
        auction.Seller = User.Identity.Name;
        
        _repository.AddAuction(auction);
        
        var auctionCreated = _mapper.Map<AuctionDto>(auction);
        
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(auctionCreated));

        var result = await _repository.SaveChangesAsync();


        if (!result) return BadRequest("Could not save changes to the database");

        return CreatedAtAction(nameof(GetAuctionById),
         new { auction.Id },
         _mapper.Map<AuctionDto>(auction));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDTO)
    {
        var auction = await _repository.GetAuctionEntityById(id);

        if(auction == null) return NotFound();
        
        if (auction.Seller != User.Identity.Name) return Unauthorized();

        auction.Item.Make = auctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDTO.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDTO.Year ?? auction.Item.Year;

        var auctionUpdated = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auctionUpdated));

        var result = await _repository.SaveChangesAsync();

        if (!result) return BadRequest("Could not update changes to the database");

        return Ok();

    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _repository.GetAuctionEntityById(id);

        if(auction == null) return NotFound();

        if (auction.Seller != User.Identity.Name) return Unauthorized();

        _repository.RemoveAuction(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new AuctionDeleted{Id = auction.Id.ToString()});

        var result = await _repository.SaveChangesAsync();

        if (!result) return BadRequest("Could not delete auction from the database");

        return Ok();
    }
}
