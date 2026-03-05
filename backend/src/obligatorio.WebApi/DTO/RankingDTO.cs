using System;

namespace obligatorio.WebApi.DTO;

public record RankingEntryDto(
    Guid UserId,
    string Name,
    string Surname,
    string Email,
    int TotalPoints
);
