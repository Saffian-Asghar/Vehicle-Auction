using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    private readonly IMapper _mapper;

    public AuctionDeletedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine("--SearchService--> Consuming AuctionDeleted event: " + context.Message.Id);

        var result = DB.DeleteAsync<Item>(context.Message.Id);

        if (!result.Result.IsAcknowledged)
            throw new MessageException(typeof(AuctionDeleted), "Failed to delete item in SearchService");

    }

}
