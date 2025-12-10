using Fusion;
using TMPro;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreP1;
    [SerializeField] private TextMeshProUGUI scoreP2;

    [Networked] private int P1Score { get; set; }
    [Networked] private int P2Score { get; set; }

    [SerializeField] private GameObject bgResults;
    [SerializeField] private TextMeshProUGUI textAdP1;
    [SerializeField] private TextMeshProUGUI textAdP2;

    [SerializeField] private int winScore;
    [SerializeField] private string writeAdWinP1;
    [SerializeField] private string writeAdWinP2;
    [SerializeField] private string writeAdLoseP1;
    [SerializeField] private string writeAdLoseP2;

    public override void Spawned()
    {
        UpdateScoreDisplay();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (P1Score >= winScore || P2Score >= winScore)
        {
            RPC_MostrarPanel(P1Score >= winScore ? 1 : 2);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ScorePlayer1()
    {
        if (Object.HasStateAuthority)
        {
            P1Score++;
        }
        UpdateScoreDisplay();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ScorePlayer2()
    {
        if (Object.HasStateAuthority)
        {
            P2Score++;
        }
        UpdateScoreDisplay();
    }

    public void UpdateScoreDisplay()
    {
        if (scoreP1 != null) scoreP1.text = P1Score.ToString();
        if (scoreP2 != null) scoreP2.text = P2Score.ToString();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_MostrarPanel(int winner)
    {
        bgResults.SetActive(true);

        if (winner == 1)
        {
            textAdP1.text = writeAdWinP1;
            textAdP2.text = writeAdLoseP2;
        }
        else
        {
            textAdP2.text = writeAdWinP2;
            textAdP1.text = writeAdLoseP1;
        }
    }
}