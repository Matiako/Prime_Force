namespace PrimeForce.Combat.Interfaces;

/// <summary>
/// ISP split: the UI only needs this to submit an answer;
/// it does not need to know about CalculateDamageAsync.
/// </summary>
public interface IChallengeAnswerReceiver
{
    void SubmitAnswer(string playerAnswer);
}
