using Fish.Entities;

namespace Fish.Feed.UI.Interfaces
{
    // Implement this to receive the domain Fish when a popup/menu is opened for a fish
    public interface IFishPopup
    {
        void SetData(FishEntity fish);
        void OnExpChanged(float val);
        void OnHungerChanged(float val);
        void UpdateTexts();
        void OnCloseClicked();
    }
}
