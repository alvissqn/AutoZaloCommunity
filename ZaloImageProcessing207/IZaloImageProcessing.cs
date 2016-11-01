using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.ImageProcessing
{


    public interface IZaloImageProcessing
    {

        ProfileMessage GetProfile(string fileImage, ScreenInfo info);
        bool IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(string fileImage, ScreenInfo info);
        FriendPositionMessage[] GetListFriendName(string captureFiles, ScreenInfo screen);
        FriendPositionMessage[] GetFriendProfileList(string fileName, ScreenInfo screen);
        bool HasFindButton();
    }
}
