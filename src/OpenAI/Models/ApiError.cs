namespace OpenAI.Models;

public class ApiError
{
    public string Message { get; set; } = string.Empty; // "message": "None is not of type 'object' - 'logit_bias'",
    public string Type { get; set; } = string.Empty; //  "type": "invalid_request_error",
    public string? Param { get; set; } //  "param": null,
    public string? Code { get; set; } // "code": null
}