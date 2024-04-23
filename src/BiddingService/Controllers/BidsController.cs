using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService;

[ApiController]
[Route("api/[controller]")]
public class BidsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly GrpcAuctionClient _grpcClient;

    public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcClient)
    {
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _grpcClient = grpcClient;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);

        if (auction == null)
        {
            auction = _grpcClient.GetAuction(auctionId);

            if (auction == null)
            {
                return BadRequest("Cannot accept bids on this auction at this time");
            }
        }

        if (auction.Seller == User.Identity.Name)
        {
            return BadRequest("You can't bid on your own auction");
        }

        var bid = new Bid
        {
            AuctionId = auctionId,
            Bidder = User.Identity.Name,
            Amount = amount,
        };

        if (auction.AuctionEnd < DateTime.UtcNow)
        {
            bid.BidStatus = BidStatus.Finished;
        }
        else
        {
            var highestBid = await DB.Find<Bid>()
                        .Match(b => b.AuctionId == auctionId)
                        .Sort(b => b.Descending(b => b.Amount))
                        .Limit(1)
                        .ExecuteFirstAsync();

            if (highestBid != null && amount > highestBid.Amount || highestBid == null)
            {
                bid.BidStatus = amount >= auction.ReservePrice ? BidStatus.Accepted : BidStatus.AcceptedBelowReserve;
            }

            if (highestBid != null && amount <= highestBid.Amount)
            {
                bid.BidStatus = BidStatus.TooLow;
            }
        }

        await DB.SaveAsync(bid);

        await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

        return Ok(_mapper.Map<BidDto>(bid));
    }

    [HttpGet("{auctionId}")]
    public async Task<ActionResult<IEnumerable<BidDto>>> GetBids(string auctionId)
    {
        var auction = await DB.Find<Auction>().OneAsync(auctionId);

        if (auction == null)
        {
            return NotFound();
        }

        var bids = await DB.Find<Bid>()
            .Match(b => b.AuctionId == auctionId)
            .Sort(b => b.Descending(b => b.BidTime))
            .ExecuteAsync();

        return bids.Select(_mapper.Map<BidDto>).ToList();
    }

}
