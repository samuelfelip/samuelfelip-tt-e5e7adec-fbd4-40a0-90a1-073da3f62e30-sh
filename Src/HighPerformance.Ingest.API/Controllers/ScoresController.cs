using HighPerformance.Ingest.Application.Commands;
using HighPerformance.Ingest.Application.DTOs;
using HighPerformance.Ingest.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HighPerformance.Ingest.API.Controllers;

/// <summary>
/// Leaderboard: ingest score events (single or bulk) and query aggregated totals within a configurable time window.
/// </summary>
/// <remarks>
/// Writes are append-only and go through the same bulk-ingest pipeline (binary COPY on PostgreSQL, EF on SQLite).
/// Optional <c>timestamp</c> must include timezone information (ISO-8601 with <c>Z</c> or offset); values are stored in UTC.
/// </remarks>
[ApiController]
[Route("api")]
public sealed class ScoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public ScoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Register a single score event.</summary>
    /// <param name="request">User identifier, non-negative score, and optional instant (omit to use server UTC now).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The identifier of the persisted row.</returns>
    /// <response code="201">Score stored.</response>
    /// <response code="400">Validation failed (empty userId, negative score, timestamp without timezone, etc.).</response>
    [HttpPost("scores")]
    [ProducesResponseType(typeof(RegisterScoreResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterScoreResponse>> RegisterScore(
        [FromBody] RegisterScoreRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new RegisterScoreCommand(request.UserId, request.Score, request.Timestamp),
            cancellationToken);
        return Created($"/api/scores/{response.EntryId}", response);
    }

    /// <summary>Register many score events in one request.</summary>
    /// <param name="request">Batch payload; size is capped by <c>LeaderboardSettings:MaxScoreBatchSize</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of rows accepted and their generated identifiers (same order as <c>entries</c>).</returns>
    /// <response code="201">All rows in the batch were persisted.</response>
    /// <response code="400">Validation failed (empty batch, over limit, invalid line, timestamp without timezone, etc.).</response>
    [HttpPost("scores/bulk")]
    [ProducesResponseType(typeof(RegisterScoresBulkResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterScoresBulkResponse>> RegisterScoresBulk(
        [FromBody] RegisterScoresBulkRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new RegisterScoresBulkCommand(request.Entries), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Top players by total score inside the configured rolling window (<c>LeaderboardSettings:WindowDays</c>).</summary>
    /// <param name="top">Maximum number of ranks to return (must be positive; defaults to 10 when invalid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Ranked list with user id, total score, and rank (1-based).</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] int top = 10,
        CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetLeaderboardQuery(top), cancellationToken);
        return Ok(response);
    }

    /// <summary>Total score for one user inside the configured rolling window.</summary>
    /// <param name="id">User identifier (same value as <c>userId</c> on ingest).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">User found with a total in the window.</response>
    /// <response code="404">No scores for this user in the window.</response>
    [HttpGet("users/{id}/score")]
    [ProducesResponseType(typeof(UserScoreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserScoreDto>> GetUserScore(string id, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetUserScoreQuery(id), cancellationToken);
        if (response is null)
            return NotFound();
        return Ok(response);
    }
}
