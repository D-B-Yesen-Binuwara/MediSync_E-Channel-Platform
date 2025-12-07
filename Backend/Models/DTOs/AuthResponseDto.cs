using System;

namespace Backend.Models.DTOs
{
	public sealed class AuthResponseDto
	{
		public string Token { get; init; } = string.Empty;
		public DateTime ExpiresAtUtc { get; init; }
	}
}

