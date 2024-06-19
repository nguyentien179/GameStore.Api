using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.EndPoints;


public static class GamesEndPoints
{
    const string GetGameEndPointName = "GetGame";
    public static RouteGroupBuilder MapGamesEndPoints(this WebApplication app)
    {

        var group = app.MapGroup("games");
        group.MapGet("/", (GameStoreContext dbContext) =>
                dbContext.Games.Include(game => game.Genre)
                         .Select(game => game.ToGameSummaryDto())
                         .AsNoTracking()
                         .ToListAsync());

        group.MapGet("/find-by-id/{id}", async (int id, GameStoreContext dbContext) =>
        {
            Game? game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
        })
            .WithName(GetGameEndPointName);

        group.MapPost("/create", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {

            Game game = newGame.ToEntity();
            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetGameEndPointName, new { id = game.Id }, game.ToGameSummaryDto());
        })
        .WithParameterValidation();

        group.MapPut("/update/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGames = await dbContext.Games.FindAsync(id);

            if (existingGames is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingGames)
                        .CurrentValues
                        .SetValues(updatedGame.ToEntity(id));

            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/delete/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();
            return Results.NoContent();
        });

        return group;
    }
}
