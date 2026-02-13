namespace DeepOverflow.Domain.Enums;

public enum FlagReason
{
    Spam = 0,
    Offensive = 1,
    LowQuality = 2,
    NeedsModeratorAttention = 3,
    Duplicate = 4,
    OffTopic = 5,
    NotAnAnswer = 6
}

public enum FlagStatus
{
    Pending = 0,
    Reviewed = 1,
    Declined = 2,
    Helpful = 3
}

public enum FlagTargetType
{
    Question = 0,
    Answer = 1,
    Comment = 2
}
