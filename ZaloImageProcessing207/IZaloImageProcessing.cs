using ZaloCommunityDev.Models;
using ZaloImageProcessing207.Structures;

namespace ZaloImageProcessing207
{


    public interface IZaloImageProcessing
    {
        ProfileMessage GetProfile(string fileImage, ScreenInfo info);
        bool IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(string fileImage, ScreenInfo info);
        FriendPositionMessage[] GetListFriendName(string captureFiles, ScreenInfo screen);
        FriendPositionMessage[] GetFriendProfileList(string fileName, ScreenInfo screen);
    }
}
