namespace NsqClient.Frames
{
    enum ErrorType
    {
        Unknown,
        Invalid,
        BadBody,
        BadTopic,
        BadChannel,
        BadMessage,
        PubFailed,
        MPubFailed,
        DPubFailed,
        FinFailed,
        ReqFailed,
        TouchFailed,
    }
}
