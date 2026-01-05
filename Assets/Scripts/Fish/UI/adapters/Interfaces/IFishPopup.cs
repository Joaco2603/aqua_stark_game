public interface IFishPopupReceiver
{
    // Implement this to receive the domain FishData when a popup/menu is opened for a fish
    void SetData(FishData fish);
    void OnExpChanged(float val);
    void OnHungerChanged(float val);
    void UpdateTexts();
    void OnCloseClicked();
}
