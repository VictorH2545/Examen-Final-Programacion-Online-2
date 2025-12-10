using Fusion;
using System.Linq;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;
    [Networked] public int _currentHealth { get; set; }

    public override void Spawned()
    {
        _currentHealth = maxHealth;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage, PlayerRef victim)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            HandleDeath(victim);
        }
    }

    private void HandleDeath(PlayerRef killer)
    {
        if (gameObject.CompareTag("Target"))
        {

            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();

            if (killer != PlayerRef.None)
            {
                PlayerRef[] players = Runner.ActivePlayers.ToArray();  // Obtener lista de jugadores ordenada

                if (players.Length >= 1)
                {
                    if (killer == players[0])
                    {
                        scoreManager.RPC_ScorePlayer1();
                    }
                    else if (players.Length >= 2 && killer == players[1])
                    {
                        scoreManager.RPC_ScorePlayer2();
                    }
                }
            }
        }

        if (Object != null && Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

}


