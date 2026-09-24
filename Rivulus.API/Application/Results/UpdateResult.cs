using System.Data;
using Rivulus.Application.Results;

public class UpdateResult
{
    public bool Sucess => FailureReason == UpdateFailureReason.None; 
    public bool Modified { get; set; }
    public UpdateFailureReason FailureReason { get; set; } = UpdateFailureReason.None;
}