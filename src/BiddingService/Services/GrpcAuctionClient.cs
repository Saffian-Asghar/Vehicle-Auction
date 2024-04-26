using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
    private readonly ILogger<GrpcAuctionClient> _logger;
    private readonly IConfiguration _config;

    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Auction GetAuction(string id)
    {
        _logger.LogInformation($"==> GRPC Client ==> GetAuction: {id}");

        var channel = GrpcChannel.ForAddress(_config["GrpcAuction"]);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest { Id = id };

        try
        {
            var response = client.GetAuction(request);
            var auction = new Auction
            {
                ID = response.Auction.Id,
                Seller = response.Auction.Seller,
                AuctionEnd = DateTime.Parse(response.Auction.AuctionEnd),
                ReservePrice = response.Auction.ReservePrice,
            };
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError($"GRPC Error: {ex.Message}");
            return null;
        }
    }

}
