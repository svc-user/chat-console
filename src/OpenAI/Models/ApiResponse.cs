namespace OpenAI.Models;

public class ApiResponse<TRes>
{
    public TRes Response { get; set; } = default!;
    public bool Success { get; set; }
    public ApiError? Error { get; set; }
    public int HttpCode { get; set; }
}
