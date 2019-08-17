namespace NsqClient.Frames
{
    enum ErrorType
    {
        Unknown,
        InvalidState,
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
