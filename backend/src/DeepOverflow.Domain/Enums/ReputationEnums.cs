namespace DeepOverflow.Domain.Enums;

public enum ReputationAction
{
    QuestionUpvoted = 0,        // +5
    QuestionDownvoted = 1,      // -2
    AnswerUpvoted = 2,          // +10
    AnswerDownvoted = 3,        // -2
    AnswerAccepted = 4,         // +15
    AcceptAnswer = 5,           // +2
    EditApproved = 6,           // +2
    BountyAwarded = 7,          // +variable
    PenaltySpam = 8,            // -100
    PenaltyOffensive = 9        // -100
}
