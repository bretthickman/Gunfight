using System.Collections;

public interface IGameMode
{
    void StartRound();
    void EndRound();
    void ToLobby();
    void PlayerDied(PlayerController player);
    void QuitGame();
    IEnumerator QuitCountdown();
    IEnumerator DelayedEndRound();
    IEnumerator PreroundCountdown();
    void CheckWinCondition(int oldAliveNum, int newAliveNum);
    void SpawnWeaponsInGame();
    void DeleteWeaponsInGame();
    bool CheckIfGameNeedsStart();
    void InitializeGameMode();
    void DecrementCurrentNumberOfEnemies();
    void RankingList();


    bool GetUseCards();
    void SetUseCards(bool usingCards);
    int GetAliveNum();
    void SetAliveNum(int num);
    void SetTotalRounds(int totalRounds);
    void SetQuitClicked(bool b);
}
